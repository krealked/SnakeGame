using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Effects;

namespace SnakeGame.Client.Game;

public class GameEngine
{
    private readonly Canvas _canvas;
    private readonly Action<int> _onGameOver;
    private readonly Action<int> _onScoreChanged;

    private readonly DispatcherTimer _logicTimer;
    private readonly DispatcherTimer _renderTimer;

    private const int CellSize = 20;
    private const double RenderTickMs = 16;

    private double _logicTickMs = 140; // будет ускоряться
    private double _lerp;
    private double _gameOverFade;
    private double _shakeOffset;
    private double _eatAnimationProgress;
    private double _waveOffset;

    // ДЛЯ ПЛАВНОГО ПОВОРОТА ГОЛОВЫ
    private Direction _targetDirection;
    private Direction _currentRenderDirection;
    private double _directionLerp;

    private GameState _state = default!;
    private bool _isGameOverAnimating;
    private bool _darkTheme = false;

    private readonly Random _rnd = new Random();

    // Для анимации появления новых частей тела
    private List<(Point Position, double BirthTime)> _newSegments = new();
    private const double NewSegmentDuration = 0.3; // секунды

    public GameEngine(Canvas canvas, Action<int> onGameOver, Action<int> onScoreChanged)
    {
        _canvas = canvas;
        _onGameOver = onGameOver;
        _onScoreChanged = onScoreChanged;

        _logicTimer = new DispatcherTimer();
        _logicTimer.Tick += LogicTick;

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(RenderTickMs)
        };
        _renderTimer.Tick += RenderTick;
    }

    public void Start()
    {
        int cols = (int)(_canvas.ActualWidth / CellSize);
        int rows = (int)(_canvas.ActualHeight / CellSize);

        if (cols <= 0 || rows <= 0)
        {
            cols = 30;
            rows = 20;
            _canvas.Width = cols * CellSize;
            _canvas.Height = rows * CellSize;
        }

        // Слегка бело-голубой фон (светлее, чем раньше)
        var bgBrush = new SolidColorBrush(Color.FromRgb(240, 250, 255)); // #F0FAFF
        _canvas.Background = bgBrush;

        _state = new GameState
        {
            Width = cols,
            Height = rows,
            Snake = new Snake(cols / 2, rows / 2),
            Food = GenerateFood(cols, rows, null),
            Score = 0,
            IsGameOver = false
        };

        _targetDirection = _state.Snake.CurrentDirection;
        _currentRenderDirection = _state.Snake.CurrentDirection;
        _directionLerp = 1;

        _logicTickMs = 140;
        _lerp = 1;
        _gameOverFade = 0;
        _shakeOffset = 0;
        _eatAnimationProgress = 0;
        _waveOffset = 0;
        _isGameOverAnimating = false;
        _newSegments.Clear();

        _logicTimer.Interval = TimeSpan.FromMilliseconds(_logicTickMs);
        _logicTimer.Start();
        _renderTimer.Start();
    }

    public void Stop()
    {
        _logicTimer.Stop();
        _renderTimer.Stop();
    }

    public void HandleKey(System.Windows.Input.Key key)
    {
        if (_state.IsGameOver) return;

        Direction newDir = _state.Snake.CurrentDirection;
        switch (key)
        {
            case System.Windows.Input.Key.Up:
                if (_state.Snake.CurrentDirection != Direction.Down)
                    newDir = Direction.Up;
                break;
            case System.Windows.Input.Key.Down:
                if (_state.Snake.CurrentDirection != Direction.Up)
                    newDir = Direction.Down;
                break;
            case System.Windows.Input.Key.Left:
                if (_state.Snake.CurrentDirection != Direction.Right)
                    newDir = Direction.Left;
                break;
            case System.Windows.Input.Key.Right:
                if (_state.Snake.CurrentDirection != Direction.Left)
                    newDir = Direction.Right;
                break;
        }

        if (newDir != _state.Snake.CurrentDirection)
        {
            _targetDirection = newDir;
            _directionLerp = 0; // начать анимацию поворота
        }

        _state.Snake.CurrentDirection = newDir;
    }

    private void LogicTick(object? sender, EventArgs e)
    {
        if (_state.IsGameOver) return;

        var head = _state.Snake.Head;
        var newHead = head with
        {
            X = head.X + (_state.Snake.CurrentDirection == Direction.Left ? -1 :
                          _state.Snake.CurrentDirection == Direction.Right ? 1 : 0),
            Y = head.Y + (_state.Snake.CurrentDirection == Direction.Up ? -1 :
                          _state.Snake.CurrentDirection == Direction.Down ? 1 : 0)
        };

        if (newHead.X < 0 || newHead.Y < 0 ||
            newHead.X >= _state.Width || newHead.Y >= _state.Height ||
            _state.Snake.Body.Any(p => p.X == newHead.X && p.Y == newHead.Y))
        {
            StartGameOver();
            return;
        }

        bool ate = newHead.X == _state.Food.X && newHead.Y == _state.Food.Y;

        _lerp = 0;
        _state.Snake.Move(newHead, ate);

        if (ate)
        {
            _state.Score++;
            _onScoreChanged(_state.Score);

            // Анимация поедания
            _eatAnimationProgress = 1.0;

            // Ускоряем игру
            _logicTickMs = Math.Max(70, _logicTickMs - 3);
            _logicTimer.Interval = TimeSpan.FromMilliseconds(_logicTickMs);

            _state.Food = GenerateFood(_state.Width, _state.Height, _state.Snake);
        }
    }

    private void RenderTick(object? sender, EventArgs e)
    {
        if (_isGameOverAnimating)
        {
            _gameOverFade += 0.03;
            _shakeOffset = Math.Sin(DateTime.Now.Millisecond / 50.0) * 5;
            if (_gameOverFade >= 1)
            {
                Stop();
                _onGameOver(_state.Score);
            }
        }

        // Плавный поворот головы
        if (_directionLerp < 1)
        {
            _directionLerp = Math.Min(1, _directionLerp + 0.1); // 10 шагов до конца
            _currentRenderDirection = InterpolateDirection(_state.Snake.CurrentDirection, _targetDirection, _directionLerp);
        }

        // Анимация поедания
        if (_eatAnimationProgress > 0)
        {
            _eatAnimationProgress -= 0.08;
            if (_eatAnimationProgress < 0) _eatAnimationProgress = 0;
        }

        _waveOffset += 0.05;

        _lerp = Math.Min(1, _lerp + RenderTickMs / _logicTickMs);
        Draw();
    }

    // Плавная интерполяция между направлениями (для поворота головы)
    private Direction InterpolateDirection(Direction from, Direction to, double t)
    {
        // Отображаем направления в углы: Up=0°, Right=90°, Down=180°, Left=270°
        int FromAngle() => from switch { Direction.Up => 0, Direction.Right => 90, Direction.Down => 180, Direction.Left => 270, _ => 0 };
        int ToAngle() => to switch { Direction.Up => 0, Direction.Right => 90, Direction.Down => 180, Direction.Left => 270, _ => 0 };

        int a1 = FromAngle();
        int a2 = ToAngle();

        // Кратчайший путь (например, 270 → 0 = 90° по часовой, а не 270° против)
        int diff = ((a2 - a1 + 180 + 360) % 360) - 180;
        int angle = (a1 + (int)(diff * t) + 360) % 360;

        return angle switch
        {
            < 45 or >= 315 => Direction.Up,
            < 135 => Direction.Right,
            < 225 => Direction.Down,
            _ => Direction.Left
        };
    }

    private void Draw()
    {
        _canvas.Children.Clear();

        DrawGrid();
        DrawFood();

        var body = _state.Snake.Body;

        // Рисуем хвостовой след (полупрозрачные копии)
        DrawTailTrail(body);

        // Плавное движение — используем список, чтобы не вызывать ToList() каждый раз
        var bodyList = body.ToList();

        for (int i = 0; i < bodyList.Count; i++)
        {
            var curr = bodyList[i];
            var prev = i + 1 < bodyList.Count ? bodyList[i + 1] : curr;

            // ВАЖНО: теперь используем предыдущую логическую позицию для плавности
            double x = Lerp(prev.X, curr.X, _lerp) * CellSize;
            double y = Lerp(prev.Y, curr.Y, _lerp) * CellSize;

            // Волна (опционально — можно убрать, если мешает)
            double waveY = Math.Sin(i * 0.5 + _waveOffset) * 1.5;

            bool isHead = i == 0;
            DrawSegment(x, y + waveY, isHead, i, bodyList.Count);
        }

        DrawNewSegments();
    }

    private void DrawGrid()
    {
        Brush gridBrush = _darkTheme ? Brushes.DarkGray : Brushes.LightGray;

        for (int x = 0; x <= _state.Width; x++)
        {
            var line = new Line
            {
                X1 = x * CellSize,
                Y1 = 0,
                X2 = x * CellSize,
                Y2 = _state.Height * CellSize,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            _canvas.Children.Add(line);
        }

        for (int y = 0; y <= _state.Height; y++)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y * CellSize,
                X2 = _state.Width * CellSize,
                Y2 = y * CellSize,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            _canvas.Children.Add(line);
        }
    }

    private void DrawTailTrail(LinkedList<Point> body)
    {
        if (body.Count < 2) return;

        var list = body.ToList();
        for (int i = 1; i < list.Count; i++)
        {
            var curr = list[i];
            var prev = i + 1 < list.Count ? list[i + 1] : curr;

            double x = Lerp(prev.X, curr.X, _lerp) * CellSize;
            double y = Lerp(prev.Y, curr.Y, _lerp) * CellSize;

            var trailRect = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                RadiusX = 4,
                RadiusY = 4,
                Fill = Brushes.LimeGreen,
                Opacity = 0.15
            };

            Canvas.SetLeft(trailRect, x + 1);
            Canvas.SetTop(trailRect, y + 1);
            _canvas.Children.Add(trailRect);
        }
    }

    private void DrawSegment(double x, double y, bool head, int index, int total)
    {
        Color baseColor = head ? Colors.LimeGreen : Color.FromRgb(144, 238, 144);
        double opacity = 1 - _gameOverFade;

        if (head && _eatAnimationProgress > 0)
        {
            // Усиленное надувание: 1.0 → 1.5 → 1.0
            double scale = 1 + 0.5 * Math.Sin(_eatAnimationProgress * Math.PI);

            // Лёгкая дрожь при поедании (реалистичнее)
            double shakeX = (_eatAnimationProgress > 0.3) ? Math.Sin(_eatAnimationProgress * 20) * 1.5 : 0;
            double shakeY = (_eatAnimationProgress > 0.3) ? Math.Cos(_eatAnimationProgress * 25) * 1.5 : 0;

            var headRect = new Rectangle
            {
                Width = (CellSize - 2) * scale,
                Height = (CellSize - 2) * scale,
                RadiusX = Math.Max(6, 12 * scale - 6), // голова становится круглее при увеличении
                RadiusY = Math.Max(6, 12 * scale - 6),
                Fill = new SolidColorBrush(Colors.LimeGreen) { Opacity = opacity },
                Effect = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 2, Opacity = 0.4 }
            };

            double offsetX = (headRect.Width - (CellSize - 2)) / 2;
            double offsetY = (headRect.Height - (CellSize - 2)) / 2;

            Canvas.SetLeft(headRect, x + 1 - offsetX + shakeX);
            Canvas.SetTop(headRect, y + 1 - offsetY + shakeY);
            _canvas.Children.Add(headRect);

            // ЯЗЫЧОК — появляется в середине анимации и бьёт в сторону еды
            if (_eatAnimationProgress > 0.2 && _eatAnimationProgress < 0.8)
            {
                double tongueLength = 12 * (1 - Math.Abs(_eatAnimationProgress - 0.5) * 2); // треугольный импульс

                // Направление языка — к еде
                double dx = (_state.Food.X + 0.5) * CellSize - (x + CellSize / 2);
                double dy = (_state.Food.Y + 0.5) * CellSize - (y + CellSize / 2);
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len > 0.1)
                {
                    dx /= len; dy /= len;
                }

                var tongue = new Line
                {
                    X1 = x + CellSize / 2 + shakeX,
                    Y1 = y + CellSize / 2 + shakeY,
                    X2 = x + CellSize / 2 + dx * tongueLength + shakeX,
                    Y2 = y + CellSize / 2 + dy * tongueLength + shakeY,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2.5,
                    StrokeEndLineCap = PenLineCap.Round
                };
                _canvas.Children.Add(tongue);
            }

            DrawEyes(
                x + 1 - offsetX + shakeX,
                y + 1 - offsetY + shakeY,
                _currentRenderDirection,
                headRect.Width
            );
            return;
        }

        Brush fill = new LinearGradientBrush(
            Color.FromRgb((byte)(baseColor.R - index * 3), (byte)(baseColor.G - index * 3), baseColor.B),
            baseColor,
            45)
        { Opacity = opacity };

        var rect = new Rectangle
        {
            Width = CellSize - 2,
            Height = CellSize - 2,
            RadiusX = head ? 8 : 5,
            RadiusY = head ? 8 : 5,
            Fill = fill,
            Effect = new DropShadowEffect { Color = Colors.Black, ShadowDepth = 2, Opacity = head ? 0.4 : 0.2 }
        };

        double finalX = x + 1 + (_isGameOverAnimating ? _shakeOffset : 0);
        double finalY = y + 1 + (_isGameOverAnimating ? _shakeOffset : 0);

        Canvas.SetLeft(rect, finalX);
        Canvas.SetTop(rect, finalY);
        _canvas.Children.Add(rect);

        if (head && !_isGameOverAnimating)
        {
            DrawEyes(finalX, finalY, _currentRenderDirection, CellSize - 2);
        }
    }

    private void DrawEyes(double x, double y, Direction direction, double headSize)
    {
        double eyeSize = 4;
        double offset = headSize / 4.0;

        // Позиции глаз относительно направления (с учётом поворота)
        (double, double) GetEyePosition(Direction dir, bool isLeft)
        {
            return dir switch
            {
                Direction.Up => (isLeft ? offset : headSize - offset - eyeSize, offset),
                Direction.Down => (isLeft ? offset : headSize - offset - eyeSize, headSize - offset - eyeSize),
                Direction.Left => (offset, isLeft ? offset : headSize - offset - eyeSize),
                Direction.Right => (headSize - offset - eyeSize, isLeft ? offset : headSize - offset - eyeSize),
                _ => (offset, offset)
            };
        }

        var (lEyeX, lEyeY) = GetEyePosition(direction, true);
        var (rEyeX, rEyeY) = GetEyePosition(direction, false);

        var leftEye = new Ellipse { Width = eyeSize, Height = eyeSize, Fill = Brushes.Black };
        Canvas.SetLeft(leftEye, x + lEyeX);
        Canvas.SetTop(leftEye, y + lEyeY);
        _canvas.Children.Add(leftEye);

        var rightEye = new Ellipse { Width = eyeSize, Height = eyeSize, Fill = Brushes.Black };
        Canvas.SetLeft(rightEye, x + rEyeX);
        Canvas.SetTop(rightEye, y + rEyeY);
        _canvas.Children.Add(rightEye);
    }

    private void DrawFood()
    {
        // Если анимация поедания активна — еда "втягивается" в голову
        if (_eatAnimationProgress > 0)
        {
            // Интерполяция: от позиции еды → к центру головы
            double headX = _state.Snake.Head.X + 0.5;
            double headY = _state.Snake.Head.Y + 0.5;
            double foodX = _state.Food.X + 0.5;
            double foodY = _state.Food.Y + 0.5;

            double t = _eatAnimationProgress; // 1 → 0
            double easedT = 1 - Math.Pow(1 - t, 3); // ease-in (быстро в начале)

            // Позиция: плавно к голове
            double x = Lerp(foodX, headX, easedT) * CellSize;
            double y = Lerp(foodY, headY, easedT) * CellSize;

            // Масштаб: уменьшается до 0
            double scale = 1 - easedT * 0.85; // 1 → 0.15

            // Яркость: сначала вспышка, потом затухание
            double brightness = 1 + 0.7 * Math.Sin(t * Math.PI * 4); // 2 вспышки
            Color foodColor = Color.FromRgb(
                (byte)Math.Min(255, 255 * brightness),
                (byte)Math.Min(100 + 155 * brightness, 255),
                (byte)(50 * brightness)
            );

            var food = new Ellipse
            {
                Width = (CellSize - 4) * scale,
                Height = (CellSize - 4) * scale,
                Fill = new SolidColorBrush(foodColor),
                Opacity = scale * (1 - _gameOverFade)
            };

            Canvas.SetLeft(food, x - food.Width / 2);
            Canvas.SetTop(food, y - food.Height / 2);
            _canvas.Children.Add(food);

            // Ореол при поедании
            if (t > 0.5)
            {
                var glow = new Ellipse
                {
                    Width = food.Width * 1.6,
                    Height = food.Height * 1.6,
                    Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 100)),
                    Effect = new BlurEffect { Radius = 8 }
                };
                Canvas.SetLeft(glow, x - glow.Width / 2);
                Canvas.SetTop(glow, y - glow.Height / 2);
                _canvas.Children.Add(glow);
            }
        }
        else
        {
            // Обычная пульсация
            double scale = 1 + 0.12 * Math.Sin(DateTime.Now.Millisecond / 100.0);
            var food = new Ellipse
            {
                Width = (CellSize - 4) * scale,
                Height = (CellSize - 4) * scale,
                Fill = new RadialGradientBrush(Colors.OrangeRed, Colors.DarkRed)
                {
                    Center = new System.Windows.Point(0.3, 0.3),
                    GradientOrigin = new System.Windows.Point(0.3, 0.3)
                }
            };

            double centerX = (_state.Food.X + 0.5) * CellSize;
            double centerY = (_state.Food.Y + 0.5) * CellSize;

            Canvas.SetLeft(food, centerX - food.Width / 2);
            Canvas.SetTop(food, centerY - food.Height / 2);
            _canvas.Children.Add(food);

            // Блеск
            var highlight = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.White,
                Opacity = 0.9
            };
            Canvas.SetLeft(highlight, centerX - 3);
            Canvas.SetTop(highlight, centerY - 4);
            _canvas.Children.Add(highlight);
        }
    }

    private void DrawNewSegments()
    {
        var toRemove = new List<(Point Position, double BirthTime)>();
        var now = DateTime.Now.TimeOfDay.TotalSeconds;

        foreach (var segment in _newSegments)
        {
            double age = now - segment.BirthTime;
            if (age > NewSegmentDuration)
            {
                toRemove.Add(segment);
            }
            else
            {
                double t = age / NewSegmentDuration;
                double easedT = 1 - Math.Pow(1 - t, 3); // ease-out кубическая

                double x = segment.Position.X * CellSize;
                double y = segment.Position.Y * CellSize;

                var rect = new Rectangle
                {
                    Width = (CellSize - 2) * easedT,
                    Height = (CellSize - 2) * easedT,
                    RadiusX = 5,
                    RadiusY = 5,
                    Fill = Brushes.ForestGreen,
                    Opacity = easedT * (1 - _gameOverFade)
                };

                Canvas.SetLeft(rect, x + 1 + (CellSize - 2) * (1 - easedT) / 2);
                Canvas.SetTop(rect, y + 1 + (CellSize - 2) * (1 - easedT) / 2);
                _canvas.Children.Add(rect);
            }
        }

        foreach (var seg in toRemove) _newSegments.Remove(seg);
    }

    private void StartGameOver()
    {
        _state.IsGameOver = true;
        _isGameOverAnimating = true;
        _logicTimer.Stop();
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private Point GenerateFood(int cols, int rows, Snake? snake)
    {
        Point p;
        do
        {
            p = new Point(_rnd.Next(cols), _rnd.Next(rows));
        }
        while (snake != null && snake.Body.Any(b => b.X == p.X && b.Y == p.Y));

        // Анимация появления еды — добавляем в список новых сегментов
        _newSegments.Add((p, DateTime.Now.TimeOfDay.TotalSeconds));

        return p;
    }
}