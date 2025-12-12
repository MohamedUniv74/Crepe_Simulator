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
    /// Logique d'interaction pour UCReglesJeu.xaml
    /// </summary>
    public partial class UCReglesJeu : UserControl
    {
        public UCReglesJeu()
        {
            InitializeComponent();
        }

        private void but_retour_regles(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCDemarrage();
        }
    }
}
