using Snake.Controllers;
using Snake.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        private int _cols;
        private int _rows;
        private Point _applePos;
        private int _cellSize = 40;
        private SnakeController _snake;

        private class LeaderboardRow
        {
            public int Helyezes { get; set; }
            public string Nev { get; set; }
            public int Pontszam { get; set; }
        }

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
            _cols = (int)(this.ActualWidth / _cellSize);
            _rows = (int)(this.ActualHeight / _cellSize);
            double fixedW = _cols * _cellSize;
            double fixedH = _rows * _cellSize;
            BgCanvas.Width = fixedW;
            BgCanvas.Height = fixedH;
            GameCanvas.Width = fixedW;
            GameCanvas.Height = fixedH;
            DrawGridBackground(fixedW, fixedH, _cellSize);
        }

        private void DrawGridBackground(double totalW, double totalH, int cellSize)
        {
            bool toggle = false;
            Color light = Color.FromRgb(180, 230, 110);
            Color dark = Color.FromRgb(160, 210, 85);
            for (int y = 0; y < ActualHeight; y += cellSize)
            {
                toggle = (y / cellSize) % 2 == 0;
                for (int x = 0; x < ActualWidth; x += cellSize)
                {
                    var cell = new Rectangle { Width = cellSize, Height = cellSize, Fill = new SolidColorBrush(toggle ? light : dark) };
                    Canvas.SetLeft(cell, x);
                    Canvas.SetTop(cell, y);
                    BgCanvas.Children.Add(cell);
                    toggle = !toggle;
                }
            }
        }

        private void HideAllPanels()
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            NameInputPanel.Visibility = Visibility.Collapsed;
            LeaderboardPanel.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Collapsed;
            BgCanvas.Opacity = 0.12;
        }

        private void ShowMenu()
        {
            HideAllPanels();
            MenuPanel.Visibility = Visibility.Visible;
        }

        public void StartGame_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            BgCanvas.Opacity = 1.0;
            GameCanvas.Visibility = Visibility.Visible;
            GameCanvas.Children.Clear();
            _snake = new SnakeController(
                gameCanvas: GameCanvas,
                onGameOver: () => Dispatcher.Invoke(OnGameOver),
                onAppleEaten: () => Dispatcher.Invoke(SpawnApple)
            );
            SpawnApple();
            _snake.PlaceSnake();
            _snake.Start();
        }

        private void OnGameOver()
        {
            HideAllPanels();
            GameOverScoreText.Text = $"Játék vége!  Pontszám: {_snake.Score}";
            NameInputBox.Text = "";
            NameInputPanel.Visibility = Visibility.Visible;
            NameInputBox.Focus();
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

        private void NameInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SaveAndShowLeaderboard_Click(sender, e);
        }

        private void SaveAndShowLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            string nev = NameInputBox.Text.Trim();
            if (string.IsNullOrEmpty(nev))
            {
                MessageBox.Show("Kérlek add meg a neved!", "Hiányzó név", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                LeaderboardController.SaveScore(nev, _snake?.Score ?? 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Adatbázis hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            OpenLeaderboard();
        }

        public void HighScores_Click(object sender, RoutedEventArgs e)
        {
            OpenLeaderboard();
        }

        private void OpenLeaderboard()
        {
            List<User> users;
            try
            {
                users = LeaderboardController.GetTopScores();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Adatbázis hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var rows = new List<LeaderboardRow>();
            for (int i = 0; i < users.Count; i++)
                rows.Add(new LeaderboardRow { Helyezes = i + 1, Nev = users[i].Nev, Pontszam = users[i].Pontszam });

            LeaderboardGrid.ItemsSource = rows;
            HideAllPanels();
            LeaderboardPanel.Visibility = Visibility.Visible;
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e) => ShowMenu();

        public void Quit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}