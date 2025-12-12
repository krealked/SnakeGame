using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

namespace SnakeGame.Client.Game;

public class GameEngine
{
    private readonly Canvas _canvas;
    private readonly Action<int> _onGameOver;
    private readonly Action<int> _onScoreChanged;
    private readonly DispatcherTimer _timer;
    private readonly int _cellSize = 20;
    private GameState _state = default!;

    public GameEngine(Canvas canvas, Action<int> onGameOver, Action<int> onScoreChanged)
    {
        _canvas = canvas;
        _onGameOver = onGameOver;
        _onScoreChanged = onScoreChanged;
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(120);
        _timer.Tick += Timer_Tick;
    }

    public void Start()
    {
        var cols = (int)(_canvas.ActualWidth / _cellSize);
        var rows = (int)(_canvas.ActualHeight / _cellSize);
        if (cols <= 0 || rows <= 0)
        {
            cols = 30;
            rows = 20;
            _canvas.Width = cols * _cellSize;
            _canvas.Height = rows * _cellSize;
        }
        var startX = cols / 2;
        var startY = rows / 2;
        _state = new GameState
        {
            Width = cols,
            Height = rows,
            Snake = new Snake(startX, startY),
            IsGameOver = false,
            Score = 0,
            Food = GenerateFood(cols, rows, null!)
        };
        Draw();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void HandleKey(System.Windows.Input.Key key)
    {
        switch (key)
        {
            case System.Windows.Input.Key.Up:
                if (_state.Snake.CurrentDirection != Direction.Down)
                    _state.Snake.CurrentDirection = Direction.Up;
                break;
            case System.Windows.Input.Key.Down:
                if (_state.Snake.CurrentDirection != Direction.Up)
                    _state.Snake.CurrentDirection = Direction.Down;
                break;
            case System.Windows.Input.Key.Left:
                if (_state.Snake.CurrentDirection != Direction.Right)
                    _state.Snake.CurrentDirection = Direction.Left;
                break;
            case System.Windows.Input.Key.Right:
                if (_state.Snake.CurrentDirection != Direction.Left)
                    _state.Snake.CurrentDirection = Direction.Right;
                break;
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_state.IsGameOver) return;
        var head = _state.Snake.Head;
        var newHead = head with
        {
            X = head.X + (_state.Snake.CurrentDirection == Direction.Left ? -1 : _state.Snake.CurrentDirection == Direction.Right ? 1 : 0),
            Y = head.Y + (_state.Snake.CurrentDirection == Direction.Up ? -1 : _state.Snake.CurrentDirection == Direction.Down ? 1 : 0)
        };

        if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= _state.Width || newHead.Y >= _state.Height)
        {
            GameOver();
            return;
        }

        if (_state.Snake.Body.Any(p => p.X == newHead.X && p.Y == newHead.Y))
        {
            GameOver();
            return;
        }

        var ate = newHead.X == _state.Food.X && newHead.Y == _state.Food.Y;
        _state.Snake.Move(newHead, ate);

        if (ate)
        {
            _state.Score += 1;
            _onScoreChanged?.Invoke(_state.Score);
            _state.Food = GenerateFood(_state.Width, _state.Height, _state.Snake);
        }

        Draw();
    }

    private Point GenerateFood(int cols, int rows, Snake? snake)
    {
        var rnd = new Random();
        Point pt;
        do
        {
            pt = new Point(rnd.Next(0, cols), rnd.Next(0, rows));
        }
        while (snake != null && snake.Body.Any(b => b.X == pt.X && b.Y == pt.Y));
        return pt;
    }

    private void Draw()
    {
        _canvas.Children.Clear();
        foreach (var p in _state.Snake.Body)
        {
            var rect = new Rectangle
            {
                Width = _cellSize - 1,
                Height = _cellSize - 1,
                Stroke = System.Windows.Media.Brushes.Black,
                Fill = System.Windows.Media.Brushes.Green
            };
            Canvas.SetLeft(rect, p.X * _cellSize);
            Canvas.SetTop(rect, p.Y * _cellSize);
            _canvas.Children.Add(rect);
        }

        var foodRect = new Rectangle
        {
            Width = _cellSize - 1,
            Height = _cellSize - 1,
            Stroke = System.Windows.Media.Brushes.Black,
            Fill = System.Windows.Media.Brushes.Red
        };
        Canvas.SetLeft(foodRect, _state.Food.X * _cellSize);
        Canvas.SetTop(foodRect, _state.Food.Y * _cellSize);
        _canvas.Children.Add(foodRect);
    }

    private void GameOver()
    {
        _state.IsGameOver = true;
        _timer.Stop();
        _onGameOver?.Invoke(_state.Score);
    }
}