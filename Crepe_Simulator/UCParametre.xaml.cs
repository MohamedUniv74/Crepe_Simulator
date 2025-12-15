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
            UpdateFullscreenButtonText();
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

        private void Bouton_fullscreen_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                if (mainWindow.WindowState == WindowState.Maximized && mainWindow.WindowStyle == WindowStyle.None)
                {
                    // Désactiver le fullscreen
                    mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                    mainWindow.ResizeMode = ResizeMode.CanResize;
                    mainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    // Activer le fullscreen - ORDRE IMPORTANT!
                    mainWindow.WindowStyle = WindowStyle.None;
                    mainWindow.ResizeMode = ResizeMode.NoResize;
                    // Forcer le rafraîchissement avant de maximiser
                    mainWindow.UpdateLayout();
                    mainWindow.WindowState = WindowState.Maximized;
                }
                UpdateFullscreenButtonText();
            }
        }

        private void UpdateFullscreenButtonText()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null && Bouton_fullscreen != null)
            {
                if (mainWindow.WindowState == WindowState.Maximized && mainWindow.WindowStyle == WindowStyle.None)
                {
                    Bouton_fullscreen.Content = "Mode Fenêtre";
                }
                else
                {
                    Bouton_fullscreen.Content = "Fullscreen";
                }
            }
        }
    }
}