using System;
using System.Windows;
using System.Windows.Controls;

namespace Crepe_Simulator
{
    public partial class UCTemps : UserControl
    {
        // Variable statique pour stocker le temps
        public static int TempsChoisi = 3;

        public UCTemps()
        {
            InitializeComponent();
            sliderTemps.Value = TempsChoisi;
            MettreAJourAffichage();
        }

        private void sliderTemps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MettreAJourAffichage();
        }

        private void MettreAJourAffichage()
        {
            if (txtTempsAffiche != null && sliderTemps != null)
            {
                int minutes = (int)sliderTemps.Value;
                txtTempsAffiche.Text = minutes == 1 ? "1 minute" : $"{minutes} minutes";
            }
        }

        private void bouton_valider_Click(object sender, RoutedEventArgs e)
        {
            TempsChoisi = (int)sliderTemps.Value;
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCParametre();
        }

        private void bouton_annuler_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).ZoneJeu.Content = new UCParametre();
        }
    }
}