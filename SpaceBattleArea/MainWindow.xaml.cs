using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Packaging;
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
using System.IO;

using System.Windows.Threading;

namespace SpaceBattleArea
{
    public partial class MainWindow : Window
    {
        Random random = new Random();
        DispatcherTimer dispatchertimer = new DispatcherTimer();
        bool MoveLeft;  // Движение влево
        bool MoveRight; // Движение вправо

        // cредство для удаления предметов
        List<Rectangle> ItemRemover = new List<Rectangle>();

        int enemiesSpriteCounter = 0;
        int enemiesCounter = 100;
        int SpeedOfPlayer = 15;
        int limit = 50;
        int score = 0;
        int health = 100;
        int SpeedOfEnemies = 10;

        string BestScoreStr;
        int BestScoreInt = 0;
        

        Rect pleyerHitBox;

        public MainWindow()
        {
            InitializeComponent();

            BestScoreStr = File.ReadAllText("TheBestOfScore.txt");
            
            dispatchertimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatchertimer.Tick += Dispatchertimer_Tick;
            dispatchertimer.Start();

            MyCanvas.Focus();
            //"D:\Рабочий стол\ННГУ_УЧЕБА\Технология Визуального Программирования\4ЛР_SpaceBattle\4ЛР_SpaceBattle\purple.png"
            // Font
            ImageBrush background = new ImageBrush();
            background.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\purple.png"));
            background.TileMode = TileMode.Tile;
            background.Viewport = new Rect(0,0, 0.15, 0.15);
            background.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            MyCanvas.Background = background;

            // Player
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\player.png"));
            player.Fill = playerImage;

        }

        private void Dispatchertimer_Tick(object sender, EventArgs e)
        {
            pleyerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            enemiesCounter -= 1;

            Score.Content = "Score: " + score + " (Best Score: " + BestScoreStr + " )";
            Health.Content = "Health: " + health;

            if (enemiesCounter < 0)
            {
                CreateEnemies();
                enemiesCounter = limit;
            }

            if (MoveLeft == true && Canvas.GetLeft(player) > 5)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - SpeedOfPlayer);
            }
            if (MoveRight == true && Canvas.GetLeft(player) + 95 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + SpeedOfPlayer);
            }


            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);
                    Rect bulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (Canvas.GetTop(x) < 10)
                    {
                        ItemRemover.Add(x);
                    }

                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        if(y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bulletHitBox.IntersectsWith(enemyHit))
                            {
                                ItemRemover.Add(x);
                                ItemRemover.Add(y);
                                score++;
                            }
                        }
                    }
                }

                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + SpeedOfEnemies);

                    if (Canvas.GetTop(x) > 750)
                    {
                        ItemRemover.Add(x);
                        health -= 10;
                    }

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (pleyerHitBox.IntersectsWith(enemyHitBox))
                    {
                        ItemRemover.Add(x);
                        health -= 5;
                    }
                }
            }

            foreach (Rectangle i in ItemRemover)
            {
                MyCanvas.Children.Remove(i);
            }

            if (score > 100)
            {
                limit = 20;
                SpeedOfEnemies = 15;
            }

            if (health <= 0)
            {
                if (score > Convert.ToInt32(BestScoreStr))
                {
                    BestScoreInt = score;
                    File.WriteAllText("TheBestOfScore.txt", BestScoreInt.ToString());
                }
                dispatchertimer.Stop();
                Health.Content = "Health: 0";
                Health.Foreground = Brushes.Red;
                MessageBox.Show("You have destroyed " + score + " Alien Ships" + Environment.NewLine + 
                    "Press Ok to Play Again", "Moo says: ");

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                MoveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                MoveRight = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                MoveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                MoveRight = false;
            }

            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };

                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + (player.Width / 2));
                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);

                MyCanvas.Children.Add(newBullet);
            }
        }

        private void CreateEnemies()
        {
            ImageBrush enemiesSprite = new ImageBrush();

            enemiesSpriteCounter = random.Next(1, 5);

            switch (enemiesSpriteCounter)
            {
                case 1:
                    enemiesSprite.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\1.png"));
                    break;
                case 2:
                    enemiesSprite.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\2.png"));
                    break;
                case 3:
                    enemiesSprite.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\3.png"));
                    break;
                case 4:
                    enemiesSprite.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\4.png"));
                    break;
                case 5:
                    enemiesSprite.ImageSource = new BitmapImage(new Uri("D:\\Рабочий стол\\ННГУ_УЧЕБА\\Технология Визуального Программирования\\4ЛР_SpaceBattle\\4ЛР_SpaceBattle\\5.png"));
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy", 
                Height = 60, 
                Width = 65, 
                Fill = enemiesSprite
            };
            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, random.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);
        }
    }
}
