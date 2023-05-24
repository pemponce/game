using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;

namespace SpaceBattle
{
    public partial class MainWindow : Window
    {
        private bool moveLeft;
        private bool moveRight;
        private double playerSpeed = 10;
        private double enemySpeed = 1.5;
        private int score = 0;
        private int damage = 0;
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private Random rand = new Random();
        private List<ImageBrush> enemySprites = new List<ImageBrush>();
        private double enemySpawnRate = 2.0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            LoadEnemySprites();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();
        }

        private void LoadEnemySprites()
        {
            enemySprites.Add(new ImageBrush(new BitmapImage(new Uri("1.png", UriKind.Relative))));
            enemySprites.Add(new ImageBrush(new BitmapImage(new Uri("2.png", UriKind.Relative))));
        }

        private void GameLoop(object sender, EventArgs e)
        {
            double deltaTime = gameTimer.Interval.TotalSeconds;

            if (moveLeft && Canvas.GetLeft(Player) > 0)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - playerSpeed);
            }

            if (moveRight && Canvas.GetLeft(Player) + Player.Width < GameScreen.ActualWidth)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeed);
            }

            List<Rectangle> itemRemover = new List<Rectangle>();

            foreach (var r in GameScreen.Children.OfType<Rectangle>())
            {
                if ((string)r.Tag == "bullet")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) - 20);

                    if (Canvas.GetTop(r) < 10)
                    {
                        itemRemover.Add(r);
                    }
                }

                if ((string)r.Tag == "enemy")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) + enemySpeed * deltaTime);

                    Rect playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);
                    Rect enemyHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        damage++;
                        lbl_DamageText.Content = "Damage: " + damage;
                        itemRemover.Add(r);

                        if (damage >= 3)
                        {
                            gameTimer.Stop();
                            MessageBox.Show("Game Over!" + Environment.NewLine + "Your score is: " + score, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                            ShowMainMenu();
                        }
                    }

                    if (Canvas.GetTop(r) > 750)
                    {
                        itemRemover.Add(r);
                    }
                }
            }

            foreach (Rectangle i in itemRemover)
            {
                GameScreen.Children.Remove(i);
            }

            if (rand.NextDouble() < enemySpawnRate * deltaTime)
            {
                ImageBrush enemySprite = enemySprites[rand.Next(0, enemySprites.Count)];
                Rectangle enemy = new Rectangle
                {
                    Tag = "enemy",
                    Width = 50,
                    Height = 50,
                    Fill = enemySprite
                };
                Canvas.SetTop(enemy, -enemy.Height);
                Canvas.SetLeft(enemy, rand.Next(0, (int)(GameScreen.ActualWidth - enemy.Width)));
                GameScreen.Children.Add(enemy);
            }

            lbl_ScoreText.Content = "Score: " + score;
            score++;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }

            if (e.Key == Key.Right)
            {
                moveRight = true;
            }

            if (e.Key == Key.Escape)
            {
                gameTimer.Stop();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }

            if (e.Key == Key.Right)
            {
                moveRight = false;
            }

            if (e.Key == Key.Escape)
            {
                gameTimer.Stop();
            }
        }



        private void ShowMainMenu()
        {
            MainMenuWindow mainMenu = new MainMenuWindow();
            mainMenu.Show();
            Close();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            MainMenuWindow mainMenu = new MainMenuWindow();
            mainMenu.Show();
        }
    }
}
