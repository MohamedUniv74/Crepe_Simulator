using System;
using System.Diagnostics.Eventing.Reader;
using System.Media;
using System.Numerics;
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
            // Initialiser le temps voulu 
            tempsRestant = TimeSpan.FromMinutes(UCTemps.TempsChoisi);//changer la valeur entre parenthese pour modifier le temps de jeu
            label_timer.Text = tempsRestant.ToString(@"mm\:ss");



            // Créer le timer qui se déclenche chaque seconde
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Décrémenter d'une seconde
            tempsRestant = tempsRestant.Subtract(TimeSpan.FromSeconds(1));

            // Mettre à jour l'affichage
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







            // Vérifier si le temps est écoulé
            if (tempsRestant.TotalSeconds <= 0)
            {
                timer.Stop();
                label_timer.Text = "00:00";

                // Ouvrir la page de score
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


        //Bouton préparation

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


        //Timer préparation

        private void Timer_Preparation(object sender, EventArgs e)
        {
            tempsRestantPreparation--;
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";

            if (tempsRestantPreparation <= 0)
            {
                timerPreparation.Stop();

                imgCrepe2.Visibility = Visibility.Visible;
                imgCrepe1.Visibility = Visibility.Hidden;

                //Retour position initial de la poele
                Canvas.SetLeft(imgPoele, 312);
                Canvas.SetTop(imgPoele, 276);
                PoeleRotation.Angle = 0;

                txtTimer.Text = "";
            }
        }




        private void but_nuttela(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0)
            {
                // changer l’image de la crêpe
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_nutella.png", UriKind.Relative));


            }
        }

        private void but_caramel(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0)
            {
                // changer l’image de la crêpe
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_caramele.png", UriKind.Relative));
            }
        }

        private void but_confutture(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0)
            {
                // changer l’image de la crêpe
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_confitture.png", UriKind.Relative));
            }
        }

        private void but_cmiel(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0)
            {
                // changer l’image de la crêpe
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_chevremiel.png", UriKind.Relative));
            }
        }

        private void but_sucre(object sender, RoutedEventArgs e)
        {
            if (tempsRestantPreparation <= 0)
            {
                // changer l’image de la crêpe
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepes/crepe_sucre.png", UriKind.Relative));
            }

        }
    }
}