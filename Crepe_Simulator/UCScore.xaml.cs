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
using System.Media;

namespace Crepe_Simulator
{
    public partial class UCScore : UserControl
    {
        private SoundPlayer sonFin;

        public UCScore()
        {
            InitializeComponent();

            // Initialiser et jouer le son
            JouerSonFin();

            // Afficher le score obtenu durant la partie
            AfficherScore();

            // Ajouter les événements Click aux boutons
            bouton_rejouer.Click += Bouton_rejouer_Click;
            bouton_menu.Click += Bouton_menu_Click;
        }

        private void JouerSonFin()
        {
            try
            {
                // Créer une instance de SoundPlayer avec le fichier WAV
                sonFin = new SoundPlayer(Application.GetResourceStream(
                    new Uri("pack://application:,,,/sons/son-fin.wav")).Stream);

                // Précharger le son
                sonFin.Load();

                // Jouer le son
                sonFin.Play();
            }
            catch (Exception ex)
            {
                // Gérer les erreurs (fichier non trouvé, etc.)
                MessageBox.Show($"Erreur lors de la lecture du son : {ex.Message}");
            }
        }

        private void AfficherScore()
        {
            // Récupérer le score depuis UCJeu
            int scoreObtenu = UCJeu.Score;

            if (scoreObtenu < 1)
            {
                perdu.Visibility = Visibility.Visible;
            }
            else
            {
                trophée.Visibility = Visibility.Visible;
            }

            // Afficher le score avec le symbole €
            label_score.Text = $"{scoreObtenu}€";
        }

        private void Bouton_rejouer_Click(object sender, RoutedEventArgs e)
        {
            // Arrêter le son si nécessaire
            if (sonFin != null)
            {
                sonFin.Stop();
                sonFin.Dispose();
            }

            // Récupérer la fenêtre principale
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow != null && mainWindow.Content is Grid grid)
            {
                // Vider le contenu actuel
                grid.Children.Clear();

                // Créer une nouvelle instance de UCJeu (avec un timer qui redémarre)
                grid.Children.Add(new UCJeu());
            }
        }

        private void Bouton_menu_Click(object sender, RoutedEventArgs e)
        {
            // Arrêter le son si nécessaire
            if (sonFin != null)
            {
                sonFin.Stop();
                sonFin.Dispose();
            }

            // Récupérer la fenêtre principale
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow != null && mainWindow.Content is Grid grid)
            {
                // Vider le contenu actuel
                grid.Children.Clear();

                // Retourner au menu principal (adaptez selon votre page de menu)
                // Supposons que vous avez une page UCMenu
                grid.Children.Add(new UCDemarrage());
            }
        }
    }
}