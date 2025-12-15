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

        // AJOUT : Variable pour le score
        public static int Score { get; set; } = 0;
        private const int PRIX_CREPE = 5; // Prix par crêpe vendue

        // AJOUT : Système d'amélioration
        private int tempsCuissonActuel = 10; // Temps de cuisson actuel
        private const int COUT_AMELIORATION = 50; // Coût de l'amélioration
        private const int REDUCTION_TEMPS = 2; // Réduction de temps par amélioration

        public UCJeu()
        {
            InitializeComponent();
            InitialiserTimer();

            /*Initialiser le score à 100 au début du jeu pour essayer le système d'amélioration
            Score = 100;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();*/

            timerPreparation = new DispatcherTimer();
            timerPreparation.Interval = TimeSpan.FromSeconds(1);
            timerPreparation.Tick += Timer_Preparation;
        }

        // AJOUT : Méthode pour mettre à jour l'affichage du score
        private void MettreAJourAffichageScore()
        {
            label_argent.Text = $"{Score}€";
        }

        // AJOUT : Méthode pour mettre à jour le bouton d'amélioration
        private void MettreAJourBoutonAmelioration()
        {
            if (bouton_ameliorer != null)
            {
                int tempsApresAmelioration = tempsCuissonActuel - REDUCTION_TEMPS;
                bouton_ameliorer.Content = $"⚡ Améliorer ({COUT_AMELIORATION}€)\nTemps: {tempsApresAmelioration}s";

                // Désactiver le bouton si pas assez d'argent ou temps minimum atteint
                bouton_ameliorer.IsEnabled = Score >= COUT_AMELIORATION && tempsCuissonActuel > 2;

                // Changer l'opacité si désactivé
                bouton_ameliorer.Opacity = (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2) ? 1.0 : 0.5;
            }
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

            int tempsTotalSecondes = UCTemps.TempsChoisi * 60; // Conversion en secondes


            if ((int)tempsRestant.TotalSeconds == (int)(tempsTotalSecondes * 0.83))
            {
                imgClient2.Visibility = Visibility.Visible;
                imgcmd1.Visibility = Visibility.Visible;
            }

            if ((int)tempsRestant.TotalSeconds == (int)(tempsTotalSecondes * 0.66))
            {
                imgClient3.Visibility = Visibility.Visible;
                imgcmd2.Visibility = Visibility.Visible;
            }

            if ((int)tempsRestant.TotalSeconds == (int)(tempsTotalSecondes * 0.50))
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

                // MODIFIÉ : Utiliser le temps de cuisson actuel
                tempsRestantPreparation = tempsCuissonActuel;
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

        // NOUVEAU : Bouton Améliorer
        private async void bouton_ameliorer_Click(object sender, RoutedEventArgs e)
        {
            if (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2)
            {
                // Déduire le coût
                Score -= COUT_AMELIORATION;

                // Réduire le temps de cuisson
                tempsCuissonActuel -= REDUCTION_TEMPS;

                // Mettre à jour l'affichage
                MettreAJourAffichageScore();
                MettreAJourBoutonAmelioration();

                // Afficher un message de confirmation
                if (labelMessageConfirmation != null)
                {
                    labelMessageConfirmation.Content = $"Amélioration achetée ! Temps: {tempsCuissonActuel}s";
                    labelMessageConfirmation.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    labelMessageConfirmation.Visibility = Visibility.Hidden;
                }
            }
        }

        // MODIFIÉ : Bouton Vendre avec augmentation du score
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

                // AJOUT : Si une crêpe a été vendue, augmenter le score
                if (crêpeTrouvée)
                {
                    Score += PRIX_CREPE;
                    MettreAJourAffichageScore();
                    MettreAJourBoutonAmelioration(); // Mettre à jour le bouton d'amélioration
                }
                else
                {
                    // Si aucune crêpe ne correspond, afficher le message
                    labelMessageErreurVente.Visibility = Visibility.Visible;
                    await Task.Delay(3000);
                    labelMessageErreurVente.Visibility = Visibility.Hidden;
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