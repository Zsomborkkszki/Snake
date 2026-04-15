using Snake.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _cols;
        private int _rows;
        private Point _applePos;
        private int _cellSize = 40;
        private SnakeController _snake;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => InitGameArea();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _snake?.HandleKeyDown(e.Key);
        }

        private void InitGameArea()
        {
            int cellSize = 40;

            // Ablak belső területéből számoljuk a cellákat
            _cols = (int)(this.ActualWidth / cellSize);
            _rows = (int)(this.ActualHeight / cellSize);
            // Lefele kerekítünk, hogy pontosan cellSize többszöröse legyen

            double fixedW = _cols * cellSize;
            double fixedH = _rows * cellSize;

            // Fix méret beállítása
            BgCanvas.Width = fixedW;
            BgCanvas.Height = fixedH;
            GameCanvas.Width = fixedW;
            GameCanvas.Height = fixedH;

            DrawGridBackground(fixedW, fixedH, cellSize);
        }

        private void DrawGridBackground(double totalW, double totalH, int cellSize)
        {
            bool toggle = false;

            Color light = Color.FromRgb(180, 230, 110);
            Color dark = Color.FromRgb(160, 210, 85);  // #84c540
            double canvasHeight = GameCanvas.ActualHeight;
            double canvasWidth = GameCanvas.ActualWidth;
            for (int y = 0; y < ActualHeight; y += cellSize)
            {
                toggle = (y / cellSize) % 2 == 0;
                for (int x = 0; x < ActualWidth; x += cellSize)
                {
                    var cell = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = new SolidColorBrush(toggle ? light : dark)
                    };
                    Canvas.SetLeft(cell, x);
                    Canvas.SetTop(cell, y);
                    BgCanvas.Children.Add(cell);
                    toggle = !toggle;
                }
            }
        }

        public void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            BgCanvas.Opacity = 1.0;
            GameCanvas.Visibility = Visibility.Visible;
            GameCanvas.Children.Clear();
            _snake = new SnakeController(
            gameCanvas: GameCanvas,
            onGameOver: () => Dispatcher.Invoke(ShowMenu),   // játék vége → menü
            onAppleEaten: () => Dispatcher.Invoke(SpawnApple)  // új alma
             );

            SpawnApple();
            _snake.PlaceSnake();
            _snake.Start();
        }
        private void ShowMenu()
        {
            GameCanvas.Visibility = Visibility.Collapsed;
            BgCanvas.Opacity = 0.12;         // bring the dark overlay back
            MenuPanel.Visibility = Visibility.Visible;
        }
        private void SpawnApple()
        {

            if (_cols <= 0 || _rows <= 0) return;

            var rng = new Random();
            _applePos = new Point(rng.Next(0, _cols) * _cellSize, rng.Next(0, _rows) * _cellSize);

            var apple = new Image
            {
                Width = _cellSize,
                Height = _cellSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/Assets/alma2.png")),
                Tag = "apple"
            };

            Canvas.SetLeft(apple, _applePos.X);
            Canvas.SetTop(apple, _applePos.Y);
            GameCanvas.Children.Add(apple);
        }


        public void HighScores_Click(object sender, RoutedEventArgs e) { }
        public void Quit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
