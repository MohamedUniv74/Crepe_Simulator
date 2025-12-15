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

            // Afficher le score obtenu durant la partie
            AfficherScore();

            // Ajouter les événements Click aux boutons
            bouton_rejouer.Click += Bouton_rejouer_Click;
            bouton_menu.Click += Bouton_menu_Click;
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