using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Tetris;

public partial class MainWindow : Window
{
    private readonly int rows = 20, cols = 10;
    private readonly Rectangle[,] grid;
    private readonly int cellSize = 30;
    private int[,] gameGrid;
    private Shape currentShape;
    private Point currentPosition;
    private DispatcherTimer timer;

    public MainWindow()
    {
        InitializeComponent();
        grid = new Rectangle[rows, cols];
        gameGrid = new int[rows, cols];
        InitializeGameCanvas();
        StartNewGame();
    }

    private void InitializeGameCanvas()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Gray
                };
                GameCanvas.Children.Add(rect);
                Canvas.SetTop(rect, r * cellSize);
                Canvas.SetLeft(rect, c * cellSize);
                grid[r, c] = rect;
            }
        }
    }

    private void StartNewGame()
    {
        ClearGrid();
        SpawnNewShape();
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        timer.Tick += GameLoop;
        timer.Start();
    }

    private void ClearGrid()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                gameGrid[r, c] = 0;
    }

    private void SpawnNewShape()
    {
        var shapes = new List<int[,]> {
            new int[,] { {1,1}, {1,1} },     // O shape
            new int[,] { {0,1,0}, {1,1,1} }, // T shape
            new int[,] { {1,1,0}, {0,1,1} }, // Z shape
            new int[,] { {0,0,0}, {1,1,1} }  // line shape
        };

        Random rand = new();
        int[,] shape = shapes[rand.Next(shapes.Count)];
        currentShape = new Shape(shape);
        currentPosition = new Point(cols / 2 - shape.GetLength(1) / 2, 0);

        if (!CanMove(currentShape.Matrix, currentPosition))
        {
            timer.Stop();
            MessageBox.Show("Game Over!");
            StartNewGame();
        }
    }

    private void GameLoop(object sender, EventArgs e)
    {
        if (CanMove(currentShape.Matrix, new Point(currentPosition.X, currentPosition.Y + 1)))
        {
            currentPosition.Y++;
        }
        else
        {
            PlaceShape();
            CheckLines();
            SpawnNewShape();
        }
        DrawGame();
    }

    private bool CanMove(int[,] shape, Point position)
    {
        for (int r = 0; r < shape.GetLength(0); r++)
            for (int c = 0; c < shape.GetLength(1); c++)
                if (shape[r, c] > 0)
                {
                    int newX = (int)(position.X + c);
                    int newY = (int)(position.Y + r);
                    if (newX < 0 || newX >= cols || newY >= rows || (newY >= 0 && gameGrid[newY, newX] > 0))
                        return false;
                }
        return true;
    }

    private void PlaceShape()
    {
        for (int r = 0; r < currentShape.Matrix.GetLength(0); r++)
            for (int c = 0; c < currentShape.Matrix.GetLength(1); c++)
                if (currentShape.Matrix[r, c] > 0)
                {
                    int x = (int)(currentPosition.X + c);
                    int y = (int)(currentPosition.Y + r);
                    if (y >= 0)
                        gameGrid[y, x] = currentShape.Matrix[r, c];
                }
    }

    private void CheckLines()
    {
        for (int r = rows - 1; r >= 0; r--)
        {
            bool fullLine = true;
            for (int c = 0; c < cols; c++)
                if (gameGrid[r, c] == 0)
                {
                    fullLine = false;
                    break;
                }

            if (fullLine)
            {
                for (int ry = r; ry > 0; ry--)
                    for (int c = 0; c < cols; c++)
                        gameGrid[ry, c] = gameGrid[ry - 1, c];

                r++; // Проверяем эту же строку снова
            }
        }
    }

    private void DrawGame()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                if (gameGrid[r, c] > 0)
                    grid[r, c].Fill = Brushes.Blue;
                else
                    grid[r, c].Fill = Brushes.Transparent;
            }

        foreach (var block in currentShape.GetBlocks())
        {
            int x = (int)(block.X + currentPosition.X);
            int y = (int)(block.Y + currentPosition.Y);
            if (y >= 0)
                grid[y, x].Fill = Brushes.Red;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Left && CanMove(currentShape.Matrix, new Point(currentPosition.X - 1, currentPosition.Y)))
            currentPosition.X--;
        else if (e.Key == Key.Right && CanMove(currentShape.Matrix, new Point(currentPosition.X + 1, currentPosition.Y)))
            currentPosition.X++;
        else if (e.Key == Key.Down && CanMove(currentShape.Matrix, new Point(currentPosition.X, currentPosition.Y + 1)))
            currentPosition.Y++;
        else if (e.Key == Key.Up)
        {
            var rotated = RotateMatrix(currentShape.Matrix);
            if (CanMove(rotated, currentPosition))
                currentShape.Matrix = rotated;
        }

        DrawGame();
    }

    private int[,] RotateMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int[,] rotated = new int[cols, rows];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                rotated[c, rows - r - 1] = matrix[r, c];

        return rotated;
    }
}
