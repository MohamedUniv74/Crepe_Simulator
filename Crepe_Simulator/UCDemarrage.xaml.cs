using System;
using System.Media;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crepe_Simulator
{
    public partial class UCDemarrage : UserControl
    {
        // Déclarer la variable globale pour la musique
        private static MediaPlayer musique;

        public UCDemarrage()
        {
            InitializeComponent();

            // Initialiser et lancer la musique
            InitMusique();
        }

        // Méthode pour initialiser la musique
        private void InitMusique()
        {
            // Ne créer la musique qu'une seule fois
            if (musique == null)
            {
                musique = new MediaPlayer();

                // Charger la musique (assurez-vous que le fichier existe dans le dossier sons/)
                musique.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/matin-insouciant.mp3"));

                // Relancer la musique automatiquement quand elle se termine
                musique.MediaEnded += RelanceMusique;

                // Ajuster le volume (0.0 à 1.0)
                musique.Volume = 0.5;

                // Démarrer la musique
                musique.Play();
            }
        }

        // Méthode pour relancer la musique en boucle
        private void RelanceMusique(object? sender, EventArgs e)
        {
            musique.Position = TimeSpan.Zero;
            musique.Play();
        }

        // Méthode publique pour arrêter la musique (accessible depuis d'autres pages)
        public static void ArreterMusique()
        {
            if (musique != null)
            {
                musique.Stop();
            }
        }

        // Bouton JOUER → remplace ZoneJeu par UCJeu et arrête la musique
        private void butJouer(object sender, RoutedEventArgs e)
        {
            // Arrêter la musique du menu avant de lancer le jeu
            ArreterMusique();

            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCJeu();
        }

        // Bouton RÈGLES → remplace ZoneJeu par UCReglesJeu
        private void butRegles(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCReglesJeu();
        }

        // Bouton PARAMÈTRES → remplace ZoneJeu par UCParametres
        private void butParam(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCParametre();
        }

        // Bouton QUITTER → ferme l'application
        private void butQuitter(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}