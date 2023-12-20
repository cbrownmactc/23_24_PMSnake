using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;


namespace PMSnake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource>
            gridValToImage = new ()
            {
                { GridValue.Empty, Images.Empty },
                { GridValue.Snake, Images. Body },
                { GridValue.Food, Images.Food },
                { GridValue.Wall, Images.Wall }
            };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 15;
        private readonly int cols = 15;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private int highScore = 0;
        private Random random = new Random();
        private int boostSpeed = 0;

        public MainWindow()
        {
            InitializeComponent();

            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
            LoadHighScore();
        }
        
        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100-boostSpeed);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r=0; r < rows; r++)
            {
                for (int c=0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(.5, .5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
                case Key.Space:
                    //boostSpeed = (boostSpeed == 0) ? 50 : 0;
                    if (boostSpeed == 0)
                    {
                        boostSpeed = GameSettings.BoostSpeed;
                    }
                    else
                    {
                        boostSpeed = 0;
                    }

                    break;
            }
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score {gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r=0; r < rows; r++)
            {
                for (int c=0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            ShakeWindow(2000);
            Audio.GameOver.Play();

            if (gameState.Score > highScore)
            {
                highScore = gameState.Score;
                SaveHighScore();
            }

            HighScoreText.Text = $"High Score: {highScore}";

            await DrawDeadSnake();
            await Task.Delay(1000);
            OverlayText.Text = "Press Any Key To Start";
            Overlay.Visibility = Visibility.Visible;
        }

        private void LoadHighScore()
        {
            string fileName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "highscore.txt");

            if (File.Exists(fileName))
            {
                StreamReader sr = new StreamReader(fileName);
                highScore = int.Parse(sr.ReadLine());
                sr.Close();
                HighScoreText.Text = $"High Score: {highScore}";
            }
            else
            {
                StreamWriter sw = new StreamWriter(fileName);
                sw.WriteLine(highScore);
                sw.Close();
            }
        }
        private void SaveHighScore()
        {
            StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\highscore.txt");
            sw.WriteLine(highScore);
            sw.Close();
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = 
                new List<Position>(gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source =
                    (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(Math.Max(50-(i*3),1));

            }
        }

        private async Task ShakeWindow(int durationMs)
        {
            var oLeft = this.Left;
            var oTop = this.Top;

            var shakeTimer = new DispatcherTimer(DispatcherPriority.Send);
            shakeTimer.Tick += (sender, args) =>
            {
                this.Left = oLeft + random.Next(-10, 11);
                this.Top = oTop + random.Next(-10, 11);
            };

            shakeTimer.Interval = TimeSpan.FromMilliseconds(200);
            shakeTimer.Start();

            await Task.Delay(durationMs);
            shakeTimer.Stop();
        }
    }
}
