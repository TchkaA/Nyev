public class WordPrediction
{
    private Dictionary<String, Dictionary<String, int>> _transitions = new();
    
    // Обучить на тексте
    public void Train(string text, int contextSize = 2)
    {
        string[] Tokens = text.Split(' '); // public class Main

        for(int i = contextSize - 1; i < Tokens.Length; i++)
        { 
            string contextKey = string.Join(" ", Tokens.Skip(i - contextSize + 1).Take(contextSize - 1));

            if (!_transitions.ContainsKey(contextKey))
            {
                _transitions[contextKey] = new Dictionary<string, int>();
            }
            if (!_transitions[contextKey].ContainsKey(Tokens[i])) 
            {
                _transitions[contextKey][Tokens[i]] = 0;
            }
            _transitions[contextKey][Tokens[i]]++;
        }
    }
    
    
    // Предсказать следующий символ
    //public char Predict(string context)
    //{
        
    //}
}