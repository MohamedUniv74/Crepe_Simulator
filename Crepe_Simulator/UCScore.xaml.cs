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

namespace Crepe_Simulator
{
    public partial class UCScore : UserControl
    {
        private MediaPlayer mediaPlayer;

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
                // Créer une instance de MediaPlayer
                mediaPlayer = new MediaPlayer();

                // Charger le fichier audio
                string cheminSon = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sons/son-fin.mp3");

                // Vérifier si le fichier existe
                if (!System.IO.File.Exists(cheminSon))
                {
                    MessageBox.Show($"Fichier non trouvé : {cheminSon}");
                    return;
                }

                // S'abonner à l'événement MediaOpened pour jouer quand c'est prêt
                mediaPlayer.MediaOpened += (s, e) =>
                {
                    mediaPlayer.Play();
                };

                // Gérer les erreurs de média
                mediaPlayer.MediaFailed += (s, e) =>
                {
                    MessageBox.Show($"Erreur média : {e.ErrorException.Message}");
                };

                // Ouvrir le fichier
                mediaPlayer.Open(new Uri(cheminSon, UriKind.Absolute));

                // Définir le volume (optionnel, 0.0 à 1.0)
                mediaPlayer.Volume = 0.5;
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

            // Afficher le score avec le symbole €
            label_score.Text = $"{scoreObtenu}€";
        }

        private void Bouton_rejouer_Click(object sender, RoutedEventArgs e)
        {
            // Arrêter le son si nécessaire
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
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
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
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