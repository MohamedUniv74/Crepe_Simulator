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
            AfficheDemarrage();

        }

        private void AfficheDemarrage()
        {
            UCDemarrage uc = new UCDemarrage();  
            ZoneJeu.Content = uc;  
            uc.bouton_jouer.Click +=  AfficherJeu;
            uc.bouton_reglesjeu.Click += AfficherRegles;
            uc.bouton_parametre.Click += AfficherParametres;
            uc.bouton_quitter.Click += (s, e) => Application.Current.Shutdown();
        }

        private void AfficherParametres(object sender, RoutedEventArgs e)
        {
            UCParametre uc = new UCParametre();
            ZoneJeu.Content = uc;
        }

        private void AfficherRegles(object sender, RoutedEventArgs e)
        {
            UCReglesJeu uc = new UCReglesJeu();
            ZoneJeu.Content = uc;
        }

        private void AfficherJeu(object sender, RoutedEventArgs e)
        {
            UCJeu uc = new UCJeu();
            ZoneJeu.Content = uc;
        }









    }
}