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
    /// <summary>
    /// Logique d'interaction pour UCParametre.xaml
    /// </summary>
    public partial class UCParametre : UserControl
    {
        public UCParametre()
        {
            InitializeComponent();
        }

        private void but_retour_parametre(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCDemarrage();
        }

        private void Bouton_volume_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCVolume();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCTemps();

        }
    }
}
