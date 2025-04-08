using System.Windows;

namespace Tetris;

public class Shape
{
    public int[,] Matrix { get; set; }

    public Shape(int[,] matrix)
    {
        Matrix = matrix;
    }

    public IEnumerable<Point> GetBlocks()
    {
        for (int r = 0; r < Matrix.GetLength(0); r++)
            for (int c = 0; c < Matrix.GetLength(1); c++)
                if (Matrix[r, c] > 0)
                    yield return new Point(c, r);
    }
}
