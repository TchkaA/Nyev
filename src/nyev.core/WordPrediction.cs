using System.Text.RegularExpressions;

public class WordPrediction
{
    private Dictionary<String, Dictionary<String, int>> _transitions = new();
    
    // Обучить на тексте
    public void Train(string text, int contextSize = 2)
    {
        string[] tokens = Regex.Matches(text.ToLower(), @"\w+")
        .Select(m => m.Value)
        .ToArray();

        for (int i = contextSize - 1; i < tokens.Length; i++)
        { 
            string contextKey = string.Join(" ", tokens.Skip(i - contextSize + 1).Take(contextSize - 1));

            if (!_transitions.ContainsKey(contextKey))
            {
                _transitions[contextKey] = new Dictionary<string, int>();
            }
            if (!_transitions[contextKey].ContainsKey(tokens[i])) 
            {
                _transitions[contextKey][tokens[i]] = 0;
            }
            _transitions[contextKey][tokens[i]]++;
        }
    }


    /// <summary>
    /// Предсказать следующee слово
    /// </summary>
    /// <param name="context">Слово идущее до</param>
    /// <returns></returns>
    public string Predict(string context)
    {
        if (!_transitions.ContainsKey(context))
        {
            return "";
        }
        var nextWords = _transitions[context];
        string bestWord = nextWords.OrderByDescending(x => x.Value).First().Key;
        return bestWord;
    }

    public string PredictSmart(string context)
    {
        string[] parts = context.ToLower().Split(' ');
        
        for (int i = 0; i < parts.Length; i++)
        {
            var currentContextParts = parts.Skip(i);
            var key = string.Join(" ", currentContextParts);
            
            if (_transitions.ContainsKey(key))
            {
                var nextWords = _transitions[key];
                if (nextWords.Any()) 
                {
                    string bestWord = nextWords.OrderByDescending(x => x.Value).First().Key;
                    return bestWord;
                }
            }
        }
        
        return "";
    }

    public void Test()
    {
        Train("public class Program public static void Main");
        
        string prediction = Predict("public");
        Console.WriteLine($"После 'public' предсказано: {prediction}");
        
        prediction = Predict("class");
        Console.WriteLine($"После 'class' предсказано: {prediction}");
    }
}