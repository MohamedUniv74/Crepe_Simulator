using System.Text;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //AfficheDemarrage(); // Afficher l'écran d'accueil au démarrage

        }

        //private void AfficheDemarrage()
        //{
        //    UCDemarrage uc = new UCDemarrage();
        //    ZoneJeu.Content = uc;
        //    uc.butDemarrer.Click += AfficherJeu; 
        //}

        //private void AfficherJeu(object sender, RoutedEventArgs e)
        //{
        //    UCJeu ucJeu = new UCJeu(); 
        //    ZoneJeu.Content = ucJeu; 
        //}
        private void butRegles(object sender, RoutedEventArgs e)
        {

        }
    }
}