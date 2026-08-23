using System;

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

wp.Train(sampleCode);
Console.WriteLine("Введи слово для угадывания!");
var word = Console.ReadLine();
Console.WriteLine("\n Вероятное слово - " + wp.Predict(word));

wp.Test();