using System;
using System.Text;

// Включаем поддержку UTF-8, чтобы красиво рисовались рамки и символы
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    Console.Title = "Nyev — умное предсказывание";
}
catch { /* Title не поддерживается на некоторых платформах */ }

var wp = new WordPrediction();

string sampleCode = @"
public class ExampleClass  
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Hello World);
    }
}

public class MyClass
{
    private int value;
    public void SetValue(int newValue)
    {
        value = newValue;
    }
}
";

ShowBanner();
Info("Обучаюсь на примере кода...");
wp.Train(sampleCode);
wp.Train("public class Program public static void Main");
Success("Модель обучена. Добро пожаловать!");

while (true)
{
    ShowMenu();

    var choice = ReadChoice();
    switch (choice)
    {
        case "1":
            TrainMode(wp);
            break;
        case "2":
            PredictMode(wp);
            break;
        case "3":
            SmartPredictMode(wp);
            break;
        case "4":
            SaveModel(wp);
            break;
        case "5":
            LoadModel(wp);
            break;
        case "6":
            ShowStats(wp);
            break;
        case "7":
            DemoMode(wp);
            break;
        case "8":
            Goodbye();
            return;
        default:
            Error("Неизвестный пункт меню. Попробуй ещё раз.");
            Pause();
            break;
    }
}

// ================================ РЕЖИМЫ ================================

static void TrainMode(WordPrediction wp)
{
    Section("ОБУЧЕНИЕ");
    Info("Введи текст (или код) — и я запомню, какие слова идут после каких.");
    Info("Для выхода в меню введи пустую строку.");

    int trained = 0;
    while (true)
    {
        Prompt("> ");
        var text = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(text))
        {
            break;
        }

        wp.Train(text);
        trained++;
        Success($"Принято! Текст #{trained} добавлен в модель.");
    }

    if (trained > 0)
    {
        Success($"Обучение завершено, обработано текстов: {trained}.");
    }
    else
    {
        Warn("Ничего не обучено — вернулись в меню.");
    }
    Pause();
}

static void PredictMode(WordPrediction wp)
{
    Section("ПРЕДСКАЗАНИЕ СЛОВА");
    Info("Введи контекст (слово или несколько слов) — я угадаю следующее слово.");
    Info("Для выхода в меню введи пустую строку.");

    while (true)
    {
        Line();
        Prompt("Контекст: ");
        var context = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(context))
        {
            break;
        }

        var result = wp.Predict(context.Trim().ToLower());
        if (string.IsNullOrEmpty(result))
        {
            Warn($"Не нашёл, что идёт после «{context}». Может, стоит дообучить модель?");
        }
        else
        {
            Answer($"После «{context}» скорее всего идёт → «{result}»");
        }
    }
}

static void SmartPredictMode(WordPrediction wp)
{
    Section("УМНОЕ ПРЕДСКАЗАНИЕ");
    Info("Могу угадать слово даже по неполному контексту (отброшу лишние слова слева).");
    Info("Для выхода в меню введи пустую строку.");

    while (true)
    {
        Line();
        Prompt("Контекст: ");
        var context = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(context))
        {
            break;
        }

        var result = wp.PredictSmart(context);
        if (string.IsNullOrEmpty(result))
        {
            Warn($"Не нашёл подходящего продолжения для «{context}».");
        }
        else
        {
            Answer($"Умное предсказание для «{context}» → «{result}»");
        }
    }
}

static void SaveModel(WordPrediction wp)
{
    Section("СОХРАНЕНИЕ МОДЕЛИ");
    Info($"Папка: {wp.folderPath}");
    Info($"Файл:  {wp.filePath}");

    try
    {
        wp.save();
        if (File.Exists(wp.filePath))
        {
            Success("Модель успешно сохранена!");
        }
        else
        {
            Warn("Файл уже существует — сохранение пропущено (модель не перезаписана).");
        }
    }
    catch (Exception ex)
    {
        Error($"Не удалось сохранить модель: {ex.Message}");
    }
    Pause();
}

static void LoadModel(WordPrediction wp)
{
    Section("ЗАГРУЗКА МОДЕЛИ");
    try
    {
        wp.Load();
        Success("Модель загружена! Можно предсказывать.");
        ShowStats(wp);
    }
    catch (Exception ex)
    {
        Error($"Не удалось загрузить модель: {ex.Message}");
    }
    Pause();
}

static void ShowStats(WordPrediction wp)
{
    Section("СТАТИСТИКА МОДЕЛИ");

    var transitions = GetTransitions(wp);
    int totalTransitions = 0;
    foreach (var kv in transitions)
    {
        totalTransitions += kv.Value.Count;
    }

    Info($"Уникальных контекстов: {transitions.Count}");
    Info($"Всего переходов:       {totalTransitions}");
    Line();
}

static void DemoMode(WordPrediction wp)
{
    Section("ДЕМО");
    Info("Сейчас покажу, что модель умеет, на знакомом коде.");

    string[] contexts = { "public", "class", "super duper public", "private int" };
    foreach (var ctx in contexts)
    {
        var smart = wp.PredictSmart(ctx);
        var plain = wp.Predict(ctx.ToLower());

        Line($"  Контекст: «{ctx}»");
        if (!string.IsNullOrEmpty(plain))
        {
            Info($"    Обычное предсказание:   «{plain}»");
        }
        if (!string.IsNullOrEmpty(smart))
        {
            Answer($"    Умное предсказание:     «{smart}»");
        }
    }

    Pause();
}

// ================================ HELPERS ================================

// Показываем статистику по тому, что модель реально сохранила в файле (если он есть),
// либо по текущему состоянию в памяти через сохранение во временный JSON.
static Dictionary<string, Dictionary<string, int>> GetTransitions(WordPrediction wp)
{
    if (File.Exists(wp.filePath))
    {
        try
        {
            var json = File.ReadAllText(wp.filePath);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json);
            if (data != null)
            {
                return data;
            }
        }
        catch { /* файл может быть занят или битый */ }
    }
    return new Dictionary<string, Dictionary<string, int>>();
}

static string ReadChoice()
{
    Prompt("Выбор: ");
    var input = Console.ReadLine();
    if (int.TryParse(input, out _))
    {
        return input!.Trim();
    }
    return input?.Trim() ?? "";
}

static void ClearScreen()
{
    try
    {
        Console.Clear();
    }
    catch { /* вывод перенаправлен/нет консоли — просто продолжаем */ }
}

static void ShowBanner()
{
    ClearScreen();
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("-0.1v");
    Console.WriteLine();
    Console.WriteLine("   ███╗   ██╗██╗   ██╗███████╗██╗   ██╗");
    Console.WriteLine("   ████╗  ██║╚██╗ ██╔╝██╔════╝██║   ██║");
    Console.WriteLine("   ██╔██╗ ██║ ╚████╔╝ █████╗  ██║   ██║");
    Console.WriteLine("   ██║╚██╗██║  ╚██╔╝  ██╔══╝  ╚██╗ ██╔╝");
    Console.WriteLine("   ██║ ╚████║   ██║   ███████╗ ╚████╔╝ ");
    Console.WriteLine("   ╚═╝  ╚═══╝   ╚═╝   ╚══════╝  ╚═══╝  ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("   ──── Узконаправленная модель ────");
    Console.ForegroundColor = c;
    Console.WriteLine();
}

static void ShowMenu()
{
    Console.WriteLine();
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("   ╔═══════════════════════════════════════════╗");
    Console.WriteLine("   ║            ГЛАВНОЕ МЕНЮ                   ║");
    Console.WriteLine("   ╠═══════════════════════════════════════════╣");
    Console.ForegroundColor = c;
    Console.WriteLine("   ║  1. Обучение на тексте                    ║");
    Console.WriteLine("   ║  2. Предсказание слова                    ║");
    Console.WriteLine("   ║  3. Умное предсказание                    ║");
    Console.WriteLine("   ║  4. Сохранить модель                      ║");
    Console.WriteLine("   ║  5. Загрузить модель                      ║");
    Console.WriteLine("   ║  6. Статистика модели                     ║");
    Console.WriteLine("   ║  7. Демо-режим                            ║");
    Console.WriteLine("   ║  8. Выйти                                 ║");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("   ╚═══════════════════════════════════════════╝");
    Console.ForegroundColor = c;
    Console.WriteLine();
}

static void Section(string title)
{
    ClearScreen();
    Console.WriteLine();
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("   ┌─────────────────────────────────────────────┐");
    Console.WriteLine($"   │ {title.PadRight(43)} │");
    Console.WriteLine("   └─────────────────────────────────────────────┘");
    Console.ForegroundColor = c;
    Console.WriteLine();
}

static void Info(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"   ℹ  {msg}");
    Console.ForegroundColor = c;
}

static void Success(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✔  {msg}");
    Console.ForegroundColor = c;
}

static void Answer(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"   ➜  {msg}");
    Console.ForegroundColor = c;
}

static void Warn(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"   ⚠  {msg}");
    Console.ForegroundColor = c;
}

static void Error(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"   ✖  {msg}");
    Console.ForegroundColor = c;
}

static void Prompt(string msg)
{
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"   {msg}");
    Console.ForegroundColor = c;
}

static void Line(string msg = "")
{
    Console.WriteLine($"   {msg}");
}

static void Pause()
{
    Line();
    Info("Нажми Enter, чтобы продолжить...");
    Console.ReadLine();
}

static void Goodbye()
{
    ClearScreen();
    Console.WriteLine();
    var c = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("   До встречи! Модель живёт в памяти, пока работает программа.");
    Console.WriteLine("   Не забудь сохранить её (пункт 4), чтобы не потерять обучение.");
    Console.ForegroundColor = c;
    Console.WriteLine();
}