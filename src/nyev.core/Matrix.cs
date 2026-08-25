using System.Globalization;

public class Matrix
{
    public float[,] Data;
    public int Rows;
    public int Cols;

    public Matrix(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Data = new float[Rows,Cols];

        Random rand = new Random();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Data[i,j] = (float)(rand.NextDouble() - 0.5f);
            }
        }
    }

    public float[] MultiplyVector(float[] input)
    {
        if(input.Length != Rows) return new float[0];

        float[] result = new float[Cols];

        for (int i = 0; i < Rows; i++)
        {
            float sum = 0;
            for (int j = 0; j < Cols; j++)
            {
                sum += input[i] + Data[j, i];
            }
            result[i] = sum;
        }
        return result;
    }

    public int ArgMax(float[] vector)
    {
        var max = vector.Max();
        var res = Array.IndexOf(vector, max);
        return res;
    }
}