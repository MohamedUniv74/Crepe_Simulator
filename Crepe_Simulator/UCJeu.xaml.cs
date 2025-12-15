using System;
using System.Diagnostics.Eventing.Reader;
using System.Media;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Crepe_Simulator
{
    /// <summary>
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        private DispatcherTimer timer;
        private TimeSpan tempsRestant;
        DispatcherTimer timerPreparation;
        int tempsRestantPreparation = 10;

        public UCJeu()
        {
            InitializeComponent();
            InitialiserTimer();

            timerPreparation = new DispatcherTimer();
            timerPreparation.Interval = TimeSpan.FromSeconds(1);
            timerPreparation.Tick += Timer_Preparation;
        }

        private void InitialiserTimer()
        {
            tempsRestant = TimeSpan.FromMinutes(UCTemps.TempsChoisi);
            label_timer.Text = tempsRestant.ToString(@"mm\:ss");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tempsRestant = tempsRestant.Subtract(TimeSpan.FromSeconds(1));
            label_timer.Text = tempsRestant.ToString(@"mm\:ss");

            if ((int)tempsRestant.TotalSeconds == 50)
            {
                imgClient2.Visibility = Visibility.Visible;
                imgcmd1.Visibility = Visibility.Visible;
            }

            if ((int)tempsRestant.TotalSeconds == 40)
            {
                imgClient3.Visibility = Visibility.Visible;
                imgcmd2.Visibility = Visibility.Visible;
            }

            if ((int)tempsRestant.TotalSeconds == 30)
            {
                imgClient4.Visibility = Visibility.Visible;
                imgcmd3.Visibility = Visibility.Visible;
            }

            if (tempsRestant.TotalSeconds <= 0)
            {
                timer.Stop();
                label_timer.Text = "00:00";

                Window mainWindow = Window.GetWindow(this);
                if (mainWindow != null && mainWindow.Content is Grid grid)
                {
                    grid.Children.Clear();
                    grid.Children.Add(new UCScore());
                }
            }
        }

        private void butQuitter(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
            }
            Application.Current.Shutdown();
        }

        private async void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe2.Visibility == Visibility.Hidden)
            {
                double nouvellePositionX = 395;
                double nouvellePositionY = 292;

                Canvas.SetLeft(imgPoele, nouvellePositionX);
                Canvas.SetTop(imgPoele, nouvellePositionY);

                PoeleRotation.Angle = 90;

                imgCrepe1.Visibility = Visibility.Visible;

                tempsRestantPreparation = 10;
                txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";
                timerPreparation.Start();
            }
            else
            {
                labelMessageErreur.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                labelMessageErreur.Visibility = Visibility.Hidden;
            }
        }

        private void Timer_Preparation(object sender, EventArgs e)
        {
            tempsRestantPreparation--;
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";

            if (tempsRestantPreparation <= 0)
            {
                timerPreparation.Stop();

                imgCrepe2.Visibility = Visibility.Visible;
                imgCrepe1.Visibility = Visibility.Hidden;

                Canvas.SetLeft(imgPoele, 312);
                Canvas.SetTop(imgPoele, 276);
                PoeleRotation.Angle = 0;

                txtTimer.Text = "";
            }
        }

        // NOUVELLE MÉTHODE - Bouton Vendre
        private async void bouton_vendre_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe2.Visibility == Visibility.Visible)
            {
                // Récupérer le nom de l'image de la crêpe
                string crepeActuelle = imgCrepe2.Source.ToString();

                bool crêpeTrouvée = false; // Booléen pour savoir si une crêpe correspond

                // Vérifier quelle crêpe correspond et faire disparaître le client
                if (crepeActuelle.Contains("crepe_nutella") && imgClient2.Visibility == Visibility.Visible)
                {
                    imgClient2.Visibility = Visibility.Hidden;
                    imgcmd1.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;
                    crêpeTrouvée = true;
                }
                else if (crepeActuelle.Contains("crepe_caramele") && imgClient3.Visibility == Visibility.Visible)
                {
                    imgClient3.Visibility = Visibility.Hidden;
                    imgcmd2.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;
                    crêpeTrouvée = true;
                }
                else if (crepeActuelle.Contains("crepe_chevremiel") && imgClient4.Visibility == Visibility.Visible)
                {
                    imgClient4.Visibility = Visibility.Hidden;
                    imgcmd3.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;
                    crêpeTrouvée = true;
                }
                else if (crepeActuelle.Contains("crepe_confitture") && imgClient5.Visibility == Visibility.Visible)
                {
                    imgClient5.Visibility = Visibility.Hidden;
                    imgcmd4.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;
                    crêpeTrouvée = true;
                }
                else if (crepeActuelle.Contains("crepe_sucre") && imgClient6.Visibility == Visibility.Visible)
                {
                    imgClient6.Visibility = Visibility.Hidden;
                    imgcmd5.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;
                    crêpeTrouvée = true;
                }

                // Si aucune crêpe ne correspond, afficher le message
                if (!crêpeTrouvée)
                {
                    labelMessageErreurVente.Visibility = Visibility.Visible; // Afficher le label
                    await Task.Delay(3000);
                    labelMessageErreurVente.Visibility = Visibility.Hidden; // Cacher le label si une crêpe a été trouvée
                }
                

            }
        }

        private void but_nuttela(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0 && imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_nutella.png", UriKind.Relative));
            }
        }

        private void but_caramel(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0 && imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_caramele.png", UriKind.Relative));
            }
        }

        private void but_confutture(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0 && imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_confitture.png", UriKind.Relative));
            }
        }

        private void but_cmiel(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0 && imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_chevremiel.png", UriKind.Relative));
            }
        }

        private void but_sucre(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0 && imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_sucre.png", UriKind.Relative));
            }
        }
    }
}