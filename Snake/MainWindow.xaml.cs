using System;
using System.Collections.Generic;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => DrawGridBackground();
        }

        private void DrawGridBackground()
        {
            int cellSize = 20;
            var stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4caf50

            for (int x = 0; x < ActualWidth; x += cellSize)
            {
                var line = new Line { X1 = x, Y1 = 0, X2 = x, Y2 = ActualHeight, Stroke = stroke, StrokeThickness = 0.5 };
                BgCanvas.Children.Add(line);
            }
            for (int y = 0; y < ActualHeight; y += cellSize)
            {
                var line = new Line { X1 = 0, Y1 = y, X2 = ActualWidth, Y2 = y, Stroke = stroke, StrokeThickness = 0.5 };
                BgCanvas.Children.Add(line);
            }
        }

        public void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;
            // StartGame();
        }

        public void HighScores_Click(object sender, RoutedEventArgs e) { }
        public void Quit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}
