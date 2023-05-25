using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SpaceBattle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer _gameTimer = new DispatcherTimer();
        bool _moveLeft, _moveRight;
        List<Rectangle> _itemRemover = new List<Rectangle>();

        Random _rand = new Random();

        int _enemySpriteCounter = 0;

        //enemyCounter and limit are the seconds between spawns
        //enemyCounter should start higher than limit so that the player has a few more seconds before game starts
        double _enemyCounter = 10;
        int _limit = 5;

        //scalar multiplier to control how fast enemies spawn. a higher multiplier means spawning faster
        double _enemySpawnRate = 1.0;

        int _enemySpeed = 250;
        int _playerSpeed = 300;
        int _bulletSpeed = 600;

        int _score = 160;
        int _damage = 0;

        int _edgePadding = 10;

        Rect _playerHitBox;

        int _targetFps = 144;

        Stopwatch _timeManager = Stopwatch.StartNew();
        float _deltaTime = 0.0f;

        List<ImageBrush> _enemySprites = new List<ImageBrush>();

        public MainWindow()
        {
            InitializeComponent();

            _gameTimer.Interval = TimeSpan.FromSeconds(1.0 / _targetFps);

            //assigning the gameTimer Tick event call to custom function GameLoop 
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();

            GameScreen.Focus();

            //loading and setting background image
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/purple.png"));
            bg.TileMode = TileMode.Tile;
            
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            GameScreen.Background = bg;

            //loading player image
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/player.png"));
            Player.Fill = playerImage;

            //load all enemy sprites in initalization
            for (int i = 1; i <= 2; i++)
            {
                ImageBrush temp = new ImageBrush();

                string uri = string.Format("pack://application:,,,/Assets/{0}.png", i);
                temp.ImageSource = new BitmapImage(new Uri(uri));
                _enemySprites.Add(temp);
            }

            //initializing labels
            lbl_ScoreText.Content = "Score: " + _score;
            lbl_DamageText.Content = "Damage: " + _damage;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            //calculating deltaTime
            _timeManager.Stop();
            _deltaTime = _timeManager.ElapsedMilliseconds / 1000.0f;
            _timeManager.Restart();

            _playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);

            _enemyCounter -= _enemySpawnRate * _deltaTime;

            if (_enemyCounter < 0)
            {
                MakeEnemy();
                _enemyCounter = _limit;
            }

            if (_moveLeft && _playerHitBox.X > 0 + _edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - _playerSpeed * _deltaTime);
            }

            //for some reason, frame is always 17 pixels wider than specified
            if (_moveRight && _playerHitBox.X + _playerHitBox.Width < Application.Current.MainWindow.Width - 17 - _edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + _playerSpeed * _deltaTime);
            }

            foreach (Rectangle r in GameScreen.Children.OfType<Rectangle>())
            {
                if ((string)r.Tag == "bullet")
                {

                    Canvas.SetTop(r, Canvas.GetTop(r) - _bulletSpeed * _deltaTime);

                    Rect bulletHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (Canvas.GetTop(r) + r.Width < 0)
                    {
                        _itemRemover.Add(r);
                    }

                    foreach (Rectangle r1 in GameScreen.Children.OfType<Rectangle>())
                    {
                        if ((string)r1.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(r1), Canvas.GetTop(r1) + r1.Height / 4, r1.Width, r1.Height - r1.Height / 2);

                            if (bulletHitBox.IntersectsWith(enemyHit))
                            {
                                _itemRemover.Add(r);
                                _itemRemover.Add(r1);

                                _score++;

                                //TODO: make enemy spawn rate calculations into a function
                                if (_score >= 5)
                                {
                                    _playerSpeed = 400;
                                    _enemySpawnRate = 5.0;
                                    _enemySpeed = 245;

                                    if (_score >= 100)
                                    {
                                        _enemySpawnRate = 20.0;
                                        if (_score >= 250)
                                        {
                                            _playerSpeed = 420;
                                            _enemySpawnRate = 50.0;
                                            _enemySpeed = 230;
                                        }
                                    }
                                }

                                //TODO: make updating labels into a function
                                lbl_ScoreText.Content = "Score: " + _score;
                            }
                        }
                    }
                }

                if ((string)r.Tag == "enemy")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) + _enemySpeed * _deltaTime);

                    //TODO: make taking damage as well as updating labels into two seperate functions

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (_playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        _itemRemover.Add(r);

                        //if the enemy hits you, you take damage
                        _damage += 5;
                        lbl_DamageText.Content = "Damage: " + _damage;
                    }
                }
            }

            //TODO: make removing items into a function
            if (_itemRemover.Count > 0)
            {
                foreach (Rectangle r in _itemRemover)
                {
                    GameScreen.Children.Remove(r);
                }
                _itemRemover.Clear();
            }

            //TODO: change the restart game code to a function, and make it so that the current instance resets, not opening a new process
            if (_damage > 99)
            {
                _gameTimer.Stop();
                _timeManager.Stop();
                lbl_DamageText.Content = "Damage: 100";
                lbl_DamageText.Foreground = Brushes.Red;

                MessageBox.Show("Captain! You have destroyed " + _score + " Alien Ships\n Press OK to Play Again", "Shooter Game:");
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                _moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                _moveRight = true;
            }

            if (e.Key == Key.Space)
            {
                //TODO: potentially turn bullet into a class with custom constructor for custom images, speed, damage, etc.
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + Player.Width / 2 - newBullet.Width / 2);
                Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);

                GameScreen.Children.Add(newBullet);
            }
            
            if (e.Key == Key.P || e.Key == Key.P)
            {
                TogglePause();
            }
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                _moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                _moveRight = false;
            }
        }

        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void TogglePause()
        {
            switch (_gameTimer.IsEnabled)
            {
                case true:
                    _timeManager.Stop();
                    _gameTimer.Stop();
                    break;
                case false:
                    _timeManager.Restart();
                    _gameTimer.Start();
                    break;
                default:
                    Console.WriteLine("Cosmic bit flip...");
                    break;
            }
        }

        private void MakeEnemy()
        {
            _enemySpriteCounter = _rand.Next(0, 2);

            //TODO: turn enemy into a class with custom constructor for different enemy types, health, etc. 
               Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 55,
                Width = 60,
                Fill = _enemySprites[_enemySpriteCounter],
            };

            Canvas.SetLeft(newEnemy, _rand.Next(30, 730));
            Canvas.SetTop(newEnemy, -100);
            GameScreen.Children.Add(newEnemy);

            /*
            watch.Stop();
            Console.WriteLine("Spawned new enemy at time=" + watch.ElapsedMilliseconds / 1000.0);
            watch.Restart();
            */
        }
    }
}
