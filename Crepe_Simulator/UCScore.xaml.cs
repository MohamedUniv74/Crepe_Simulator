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
        public UCScore()
        {
            InitializeComponent();

            // Ajouter l'événement Click au bouton
            bouton_rejouer.Click += Bouton_rejouer_Click;
        }

        private void Bouton_rejouer_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
