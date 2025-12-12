using System;
using System.Windows;
using System.Windows.Controls;
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

        public UCJeu()
        {
            InitializeComponent();
            InitialiserTimer();
        }

        private void InitialiserTimer()
        {
            // Initialiser le temps voulu 
            tempsRestant = TimeSpan.FromMinutes(1);//changer la valeur entre parenthese pour modifier le temps de jeu
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

        private void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            double nouvellePositionX = 395;
            double nouvellePositionY = 285;

            double dx = 5;
            double dy = 3;

            Canvas.SetLeft(imgPoele, nouvellePositionX + dx);
            Canvas.SetTop(imgPoele, nouvellePositionY + dy);

            PoeleRotation.Angle = 90;
        }
    }
}