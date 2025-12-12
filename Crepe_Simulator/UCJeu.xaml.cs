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
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        public UCJeu()
        {
            InitializeComponent();
        }

        private void butQuitter(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            double nouvellePositionX = 475;
            double nouvellePositionY = 272;

            double dx = 5;
            double dy = 3;

            Canvas.SetLeft(imgPoele, nouvellePositionX + dx);
            Canvas.SetTop(imgPoele, nouvellePositionY + dy);

            PoeleRotation.Angle = 90;
        }
    }
}
