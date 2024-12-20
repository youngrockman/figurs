using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace belonging.Views
{
    public partial class MainWindow : Window
    {
        private readonly Canvas _canvas;
        private readonly List<Shape> _shapes = new List<Shape>();
        private Shape? _draggedShape;
        private Point _dragStart;
        private TextBlock? _messageTextBlock;

        // Генератор случайных чисел для позиционирования фигур
        private static readonly Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _canvas = this.FindControl<Canvas>("DrawingCanvas");

            // Установка обработчиков событий для взаимодействия с холстом
            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerMoved += OnPointerMoved;
            _canvas.PointerReleased += OnPointerReleased;

            _canvas.Loaded += OnCanvasLoaded;
        }

        private void OnCanvasLoaded(object? sender, RoutedEventArgs e)
        {
            // Удаляем обработчик события, чтобы он не вызывался повторно
            _canvas.Loaded -= OnCanvasLoaded;

            
            AddRandomShape(CreateSquare());
            AddRandomShape(CreatePentagon());
            AddRandomShape(CreateHexagon());
            AddRandomShape(CreateOctagon());
        }

        private void AddRandomShape(Shape shape)
        {
            
            var randomPosition = GenerateRandomPosition(shape);
            Canvas.SetLeft(shape, randomPosition.X);
            Canvas.SetTop(shape, randomPosition.Y);
            AddShape(shape);
        }

        private Rectangle CreateSquare()
        {
            
            return new Rectangle
            {
                Width = 80,
                Height = 80,
                Fill = Brushes.Blue
            };
        }

        private Polygon CreatePentagon()
        {
            
            return new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(40, 0),
                    new Point(80, 30),
                    new Point(65, 80),
                    new Point(15, 80),
                    new Point(0, 30)
                },
                Fill = Brushes.Green
            };
        }

        private Polygon CreateHexagon()
        {
            
            return new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(40, 0),
                    new Point(80, 20),
                    new Point(80, 60),
                    new Point(40, 80),
                    new Point(0, 60),
                    new Point(0, 20)
                },
                Fill = Brushes.Red
            };
        }

        private Polygon CreateOctagon()
        {
            
            return new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(30, 0),
                    new Point(50, 0),
                    new Point(80, 30),
                    new Point(80, 50),
                    new Point(50, 80),
                    new Point(30, 80),
                    new Point(0, 50),
                    new Point(0, 30)
                },
                Fill = Brushes.Purple
            };
        }

        private void AddShape(Shape shape)
        {
            // Добавляем фигуру в список и на холст
            _shapes.Add(shape);
            _canvas.Children.Add(shape);
        }

        private void ClearShapes()
        {
            
            _shapes.Clear();
            _canvas.Children.Clear();
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(_canvas);
            _draggedShape = null;

            // Проверяем, на какую фигуру нажал пользователь
            foreach (var shape in _shapes)
            {
                if (shape.Bounds.Contains(point))
                {
                    _draggedShape = shape;
                    _dragStart = point;
                    break;
                }
            }

            if (_draggedShape == null)
            {
                ShowMessage("Не попал!");
            }
            else
            {
                
                string shapeName = _draggedShape is Rectangle ? "квадрат" :
                                   _draggedShape is Polygon polygon ? (polygon.Points.Count == 5 ? "пятиугольник" :
                                   polygon.Points.Count == 6 ? "шестиугольник" : "восьмиугольник") :
                                   "фигуру";

                ShowMessage($"Ты попал в {shapeName}");
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_draggedShape != null)
            {
                // Перемещаем фигуру вслед за указателем мыши
                var currentPoint = e.GetPosition(_canvas);
                var delta = currentPoint - _dragStart;

                var newLeft = Canvas.GetLeft(_draggedShape) + delta.X;
                var newTop = Canvas.GetTop(_draggedShape) + delta.Y;

                // Проверка границ холста, чтобы фигура не выходила за его пределы
                if (newLeft < 0) newLeft = 0;
                if (newTop < 0) newTop = 0;
                if (newLeft + _draggedShape.Bounds.Width > _canvas.Bounds.Width) newLeft = _canvas.Bounds.Width - _draggedShape.Bounds.Width;
                if (newTop + _draggedShape.Bounds.Height > _canvas.Bounds.Height) newTop = _canvas.Bounds.Height - _draggedShape.Bounds.Height;

                Canvas.SetLeft(_draggedShape, newLeft);
                Canvas.SetTop(_draggedShape, newTop);

                _dragStart = currentPoint;
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _draggedShape = null;
        }

        private async void ShowMessage(string message)
        {
            
            if (_messageTextBlock != null)
            {
                _canvas.Children.Remove(_messageTextBlock);
                _messageTextBlock = null;
            }

            
            _messageTextBlock = new TextBlock
            {
                Text = message,
                FontSize = 40,
                Foreground = Brushes.Black,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                Margin = new Thickness(0, 30, 0, 0)
            };

            // Выравниваем по центру наше сообщение
            Canvas.SetLeft(_messageTextBlock, (_canvas.Bounds.Width - _messageTextBlock.Bounds.Width) / 2);
            Canvas.SetTop(_messageTextBlock, 0);

            _canvas.Children.Add(_messageTextBlock);

            // Удаляем сообщение через 3 сек
            await Task.Delay(3000);
            if (_messageTextBlock != null)
            {
                _canvas.Children.Remove(_messageTextBlock);
                _messageTextBlock = null;
            }
        }

        private Point GenerateRandomPosition(Shape shape)
        {
            // Случайная генерации фигуры и контроль выхода за пределы границ
            double x = _random.NextDouble() * (_canvas.Bounds.Width - shape.Bounds.Width);
            double y = _random.NextDouble() * (_canvas.Bounds.Height - shape.Bounds.Height);
            return new Point(x, y);
        }

        
        private void OnDrawSquareClick(object? sender, RoutedEventArgs e)
        {
            ClearShapes();

            var square = new Rectangle
            {
                Width = 80,
                Height = 80,
                Fill = Brushes.Blue
            };

            Canvas.SetLeft(square, (DrawingCanvas.Bounds.Width - square.Width) / 2);
            Canvas.SetTop(square, (DrawingCanvas.Bounds.Height - square.Height) / 2);

            AddShape(square);
        }

        private void OnDrawPentagonClick(object? sender, RoutedEventArgs e)
        {
            ClearShapes();

            var pentagon = new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(40, 0),
                    new Point(80, 30),
                    new Point(65, 80),
                    new Point(15, 80),
                    new Point(0, 30)
                },
                Fill = Brushes.Green
            };

            Canvas.SetLeft(pentagon, (DrawingCanvas.Bounds.Width - 80) / 2);
            Canvas.SetTop(pentagon, (DrawingCanvas.Bounds.Height - 80) / 2);

            AddShape(pentagon);
        }

        private void OnDrawHexagonClick(object? sender, RoutedEventArgs e)
        {
            ClearShapes();

            var hexagon = new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(40, 0),
                    new Point(80, 20),
                    new Point(80, 60),
                    new Point(40, 80),
                    new Point(0, 60),
                    new Point(0, 20)
                },
                Fill = Brushes.Red
            };

            Canvas.SetLeft(hexagon, (DrawingCanvas.Bounds.Width - 80) / 2);
            Canvas.SetTop(hexagon, (DrawingCanvas.Bounds.Height - 80) / 2);

            AddShape(hexagon);
        }

        private void OnDrawOctagonClick(object? sender, RoutedEventArgs e)
        {
            ClearShapes();

            var octagon = new Polygon
            {
                Points = new Avalonia.Collections.AvaloniaList<Point>
                {
                    new Point(30, 0),
                    new Point(50, 0),
                    new Point(80, 30),
                    new Point(80, 50),
                    new Point(50, 80),
                    new Point(30, 80),
                    new Point(0, 50),
                },
                Fill = Brushes.Purple
            };

            Canvas.SetLeft(octagon, (DrawingCanvas.Bounds.Width - 80) / 2);
            Canvas.SetTop(octagon, (DrawingCanvas.Bounds.Height - 80) / 2);

            AddShape(octagon);
        }
    }
}