using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Snake.Controllers
{
    internal class SnakeController
    {
        
        private const int CellSize = 40;
        private readonly Canvas _gameCanvas;
        private readonly Action _onGameOver;
        private readonly Action _onAppleEaten;

        private List<Point> _segments = new List<Point>();
        private List<Image> _segmentImages = new List<Image>();

        private Point _direction = new Point(1, 0);
        private Point _nextDirection = new Point(1, 0);

        private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);

        private readonly BitmapImage _headBitmap =
            new BitmapImage(new Uri("pack://application:,,,/Assets/fej.png"));
        private readonly BitmapImage _bodyBitmap =
            new BitmapImage(new Uri("pack://application:,,,/Assets/test.png"));


        /// <summary>
        /// Visszaadja a jelenlegi pontszámot, ami a kígyó hosszából származik (3 kezdeti szegmens miatt -3).
        /// </summary>
        public int Score
        {
            get { return _segments.Count - 3; }
        }

        /// <summary>
        /// Konstruktor, ami inicializálja a kígyó vezérlőjét a játék vásznával és a szükséges callback-ekkel.
        /// </summary>
        /// <param name="gameCanvas"></param>
        /// <param name="onGameOver"></param>
        /// <param name="onAppleEaten"></param>

        public SnakeController(Canvas gameCanvas, Action onGameOver, Action onAppleEaten)
        {
            _gameCanvas = gameCanvas;
            _onGameOver = onGameOver;
            _onAppleEaten = onAppleEaten;

            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += (s, e) => Tick();
        }


        /// <summary>
        /// Ez a metódus helyezi el a kígyó kezdeti pozícióját a vászon közepére, és inicializálja a kígyó szegmenseit.
        /// </summary>

        public void PlaceSnake()
        {
            _timer.Stop();
            ClearSnakeFromCanvas();
            _segments.Clear();
            _segmentImages.Clear();

            int cols = (int)(_gameCanvas.ActualWidth / CellSize);
            int rows = (int)(_gameCanvas.ActualHeight / CellSize);
            int startX = (cols / 2) * CellSize;
            int startY = (rows / 2) * CellSize;

            _segments.Add(new Point(startX, startY));
            _segments.Add(new Point(startX - CellSize, startY));
            _segments.Add(new Point(startX - CellSize * 2, startY));

            for (int i = 0; i < _segments.Count; i++)
            {
                AddImageToCanvas(i == 0 ? _headBitmap : _bodyBitmap, _segments[i]);
            }

            _direction = _nextDirection = new Point(1, 0);
        }

        /// <summary>
        /// Ez a metódus kezeli a billentyűleütéseket, és frissíti a kígyó irányát. Nem engedi meg az ellentétes irányba való fordulást.
        /// </summary>
        /// <param name="key"></param>
        public void HandleKeyDown(Key key)
        {
            Point d;

            if (key == Key.Up || key == Key.W) d = new Point(0, -1);
            else if (key == Key.Down || key == Key.S) d = new Point(0, 1);
            else if (key == Key.Left || key == Key.A) d = new Point(-1, 0);
            else if (key == Key.Right || key == Key.D) d = new Point(1, 0);
            else return;


            bool opposite = d.X == -_direction.X && d.Y == -_direction.Y;
            if (!opposite)
                _nextDirection = d;
        }

        /// <summary>
        /// Ez a metódus növeli a kígyó hosszát egy új szegmens hozzáadásával a farokhoz, és értesíti a pontszám változásáról.
        /// </summary>
        private void GrowSnake()
        {
            Point tail = _segments[_segments.Count - 1];
            Point preTail = _segments.Count > 1
                ? _segments[_segments.Count - 2]
                : tail;

            Point newSeg = new Point(
                tail.X - (preTail.X - tail.X),
                tail.Y - (preTail.Y - tail.Y)
            );

            _segments.Add(newSeg);
            AddImageToCanvas(_bodyBitmap, newSeg);

            if (_onAppleEaten != null)
                _onAppleEaten.Invoke();
        }
        /// <summary>
        /// Ez a metódus ellenőrzi, hogy a kígyó új feje ütközik-e a saját testével, ami a játék végét jelenti.
        /// </summary>
        /// <param name="newHead"></param>
        /// <returns></returns>
        private bool CheckSelfCollision(Point newHead)
        {
            for (int i = 1; i < _segments.Count; i++)
            {
                if (_segments[i] == newHead)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Ez a metódus ellenőrzi, hogy a kígyó új feje ütközik-e a vászon szélével, ami a játék végét jelenti.
        /// </summary>
        /// <param name="newHead"></param>
        /// <returns></returns>

        private bool CheckWallCollision(Point newHead)
        {
            double maxX = Math.Floor(_gameCanvas.ActualWidth / CellSize) * CellSize;
            double maxY = Math.Floor(_gameCanvas.ActualHeight / CellSize) * CellSize;

            return newHead.X < 0
                || newHead.Y < 0
                || newHead.X >= _gameCanvas.ActualWidth
                || newHead.Y >= _gameCanvas.ActualHeight;
        }

        /// <summary>
        /// Ez a metódus kezeli a játék végét, megállítja a timert, és meghívja a játék vége callback-et egy adott okkal.
        /// Ez lehet ütközés a fallal, önmagával, vagy az, hogy nincs több lépési lehetőség.
        /// </summary>
        /// <param name="reason"></param>
        private void TriggerGameOver(string reason)
        {
            _timer.Stop();
            System.Diagnostics.Debug.WriteLine("Game Over: " + reason + " | Pontszám: " + Score);
            if (_onGameOver != null)
                _onGameOver.Invoke();
        }
        /// <summary>
        /// Ez a metódus ellenőrzi, hogy a kígyó új feje körül nincs-e olyan mező, amire léphetne, ami akkor fordul elő, ha a kígyó teljesen körbe van zárva a saját testével vagy a falakkal. Ez egy speciális eset, amikor a játék véget érhet, még akkor is,
        /// ha a kígyó nem ütközik közvetlenül.
        /// </summary>
        /// <returns></returns>
        private bool CheckNoMovesLeft()
        {
            Point[] directions =
            {
                new Point( 1,  0),
                new Point(-1,  0),
                new Point( 0,  1),
                new Point( 0, -1)
            };

            foreach (Point dir in directions)
            {
                Point neighbor = new Point(
                    _segments[0].X + dir.X * CellSize,
                    _segments[0].Y + dir.Y * CellSize
                );

                if (!CheckWallCollision(neighbor) && !CheckSelfCollision(neighbor))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ez a metódus a játék fő ciklusa, ami minden timer tick-kor fut le.
        /// Frissíti a kígyó pozícióját, ellenőrzi az ütközéseket, kezeli az almaevést, és frissíti a vászon megjelenését.
        /// Ha bármilyen ütközés történik, vagy nincs több lépési lehetőség, akkor meghívja a játék vége callback-et.
        /// </summary>
        private void Tick()
        {
            System.Diagnostics.Debug.WriteLine(
    $"Canvas: {_gameCanvas.ActualWidth}x{_gameCanvas.ActualHeight} | Head: {_segments[0]}");

            _timer.Stop();
            _direction = _nextDirection;

            Point newHead = new Point(
                _segments[0].X + _direction.X * CellSize,
                _segments[0].Y + _direction.Y * CellSize
            );

            if (CheckWallCollision(newHead))
            {
                TriggerGameOver("Falnak mentél!");
                return;
            }

            if (CheckSelfCollision(newHead))
            {
                TriggerGameOver("Magadba mentél!");
                return;
            }

            if (IsAppleCollision(newHead))
            {
                RemoveAppleFromCanvas();
                MoveSegments(newHead);
                GrowSnake();
            }
            else
            {
                MoveSegments(newHead);
            }

            RefreshCanvas();

            if (CheckNoMovesLeft())
            {
                TriggerGameOver("Nincs több szabad mező!");
                return;
            }
            _timer.Start();
        }
        
        /// Ez a metódus elindítja a játékot, azaz elindítja a timer-t, ami elkezdi a kígyó mozgását.
        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }

        /// <summary>
        /// Ez a metódus frissíti a kígyó szegmenseinek pozícióját a listában, úgy hogy a fej új pozíciója lesz az első elem, és minden szegmens követi az előzőt.
        /// Ez a kígyó mozgásának alapja, ahol a fej új helyre lép, és a test követi azt.
        /// </summary>
        /// <param name="newHead"></param>
        private void MoveSegments(Point newHead)
        {
            for (int i = _segments.Count - 1; i > 0; i--)
                _segments[i] = _segments[i - 1];
            _segments[0] = newHead;
        }
        /// <summary>
        /// Ez a metódus frissíti a kígyó szegmenseinek megjelenését a vásznon, úgy hogy minden szegmens képe a megfelelő pozícióra kerül.
        /// Emellett forgatja a fej képét az aktuális irányba.
        /// </summary>
        private void RefreshCanvas()
        {
            for (int i = 0; i < _segmentImages.Count; i++)
            {
                Canvas.SetLeft(_segmentImages[i], _segments[i].X);
                Canvas.SetTop(_segmentImages[i], _segments[i].Y);
            }
            RotateHead();
        }
        /// <summary>
        /// Ez a metódus forgatja a kígyó fejét a jelenlegi irányba, hogy vizuálisan is jelezze, merre halad a kígyó.
        /// Ez egy egyszerű forgatás a 0, 90, 180 vagy 270 fokos szögekre az X és Y irány alapján.
        /// </summary>
        private void RotateHead()
        {
            double angle;

            if (_direction.X == 1 && _direction.Y == 0) angle = 0;
            else if (_direction.X == -1 && _direction.Y == 0) angle = 180;
            else if (_direction.X == 0 && _direction.Y == -1) angle = 270;
            else if (_direction.X == 0 && _direction.Y == 1) angle = 90;
            else angle = 0;

            _segmentImages[0].RenderTransformOrigin = new Point(0.5, 0.5);
            _segmentImages[0].RenderTransform = new RotateTransform(angle);
        }
        /// <summary>
        /// Ez a metódus hozzáad egy új képet a vászonhoz egy adott bitmap és pozíció alapján.
        /// Ez a kígyó szegmenseinek megjelenítésére szolgál, amikor új szegmenst adunk hozzá, vagy amikor a kígyó helyet változtat.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="pos"></param>
        private void AddImageToCanvas(BitmapImage bitmap, Point pos)
        {
            Image img = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = bitmap,
                Tag = "snake"
            };
            Canvas.SetLeft(img, pos.X);
            Canvas.SetTop(img, pos.Y);
            _gameCanvas.Children.Add(img);
            _segmentImages.Add(img);
        }
        /// <summary>
        /// Ez a metódus eltávolítja a kígyó összes szegmensének képét a vászonról, és törli a szegmens képek listáját.
        /// </summary>
        private void ClearSnakeFromCanvas()
        {
            foreach (Image img in _segmentImages)
                _gameCanvas.Children.Remove(img);
        }
        /// <summary>
        /// Ez a metódus eltávolítja az almát a vászonról, amikor a kígyó megeszi azt.
        /// Keres egy olyan képet a vászon gyerekei között, aminek a Tag-je "apple", és eltávolítja azt.
        /// </summary>
        private void RemoveAppleFromCanvas()
        {
            for (int i = _gameCanvas.Children.Count - 1; i >= 0; i--)
            {
                Image img = _gameCanvas.Children[i] as Image;
                if (img != null && (string)img.Tag == "apple")
                {
                    _gameCanvas.Children.RemoveAt(i);
                    break;
                }
            }
        }
        /// <summary>
        /// Ez a metódus ellenőrzi, hogy a kígyó új feje ütközik-e az almával, ami azt jelenti, hogy a kígyó megeszi az almát.
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        private bool IsAppleCollision(Point head)
        {
            foreach (UIElement child in _gameCanvas.Children)
            {
                Image img = child as Image;
                if (img != null && (string)img.Tag == "apple")
                {
                    double ax = Canvas.GetLeft(img);
                    double ay = Canvas.GetTop(img);
                    if (Math.Abs(head.X - ax) < 1 && Math.Abs(head.Y - ay) < 1)
                        return true;
                }
            }
            return false;
        }
    }
}