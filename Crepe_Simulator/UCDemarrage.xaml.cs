using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;

namespace Crepe_Simulator
{
    public partial class UCDemarrage : UserControl
    {
        public UCDemarrage()
        {
            InitializeComponent();
        }

        // Bouton JOUER → remplace ZoneJeu par UCJeu
        private void butJouer(object sender, RoutedEventArgs e)
        {
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
