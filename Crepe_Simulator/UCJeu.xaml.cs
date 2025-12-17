using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crepe_Simulator
{
    public partial class UCJeu : UserControl
    {
        private DispatcherTimer timer;
        private TimeSpan tempsRestant;
        DispatcherTimer timerPreparation;
        int tempsRestantPreparation = 10;
        private DispatcherTimer timerJauges;
        private bool traitementMalusEnCours = false;
        public static int Score { get; set; } = 0;
        public static readonly int PRIX_CREPE = 5;
        public static readonly int MALUS_CLIENT_PARTI = 2;
        private int tempsCuissonActuel = 10;
        public static readonly int COUT_AMELIORATION = 15;
        public static readonly int REDUCTION_TEMPS = 2;
        private Random random = new Random();
        private ClientSpawn[] listeSpawns = new ClientSpawn[50];
        private int listeSpawnsCount = 0;
        private SoundPlayer sonCuisson;
        private MediaPlayer musiqueJeu;
        private SoundPlayer sonVente;
        private (string CrepeType, Image Client, Image Commande)[] clientsEtCommandes = new (string, Image, Image)[3];

        private class ClientSpawn
        {
            public int TempsSpawn { get; set; }
            public Image ImageClient { get; set; }
            public Image ImageCommande { get; set; }
            public Border BorderJauge { get; set; }
            public Rectangle RectJauge { get; set; }
            public bool Spawned { get; set; } = false;
            public int TempsPatience { get; set; } = 30;
            public int TempsRestantPatience { get; set; }
            public bool MalusApplique { get; set; } = false;
            public bool ClientServi { get; set; } = false;
        }

        public UCJeu()
        {
            InitializeComponent();

            clientsEtCommandes[0] = ("nutella", imgClient2, imgcmd1);
            clientsEtCommandes[1] = ("caramele", imgClient3, imgcmd2);
            clientsEtCommandes[2] = ("chevremiel", imgClient4, imgcmd3);

            GenererSpawnsAleatoires();
            InitialiserTimer();
            InitialiserTimerJauges();

            Score = 0;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();

            timerPreparation = new DispatcherTimer();
            timerPreparation.Interval = TimeSpan.FromSeconds(1);
            timerPreparation.Tick += Timer_Preparation;

            InitialiserSonCuisson();
            InitialiserMusiqueJeu();
            InitialiserSonVente();
        }

        private void InitialiserSonCuisson()
        {
            sonCuisson = new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/sons/son_cuisson.wav")).Stream);
            sonCuisson.Load();
        }

        private void InitialiserMusiqueJeu()
        {
            musiqueJeu = new MediaPlayer();
            musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/son_jeu.mp3"));
            musiqueJeu.MediaEnded += RelancerMusiqueJeu;
            musiqueJeu.Volume = 0.3;
            musiqueJeu.Play();
        }

        private void RelancerMusiqueJeu(object sender, EventArgs e)
        {
            if (musiqueJeu != null)
            {
                musiqueJeu.Position = TimeSpan.Zero;
                musiqueJeu.Play();
            }
        }

        private void InitialiserSonVente()
        {
            sonVente = new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/sons/vente_son.wav")).Stream);
            sonVente.Load();
        }

        private void InitialiserTimerJauges()
        {
            timerJauges = new DispatcherTimer();
            timerJauges.Interval = TimeSpan.FromSeconds(1);
            timerJauges.Tick += TimerJauges_Tick;
            timerJauges.Start();
        }

        private void TimerJauges_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < listeSpawnsCount; i++)
            {
                var spawn = listeSpawns[i];
                if (spawn.Spawned && !spawn.MalusApplique && spawn.ImageClient != null && spawn.ImageClient.Visibility == Visibility.Visible)
                {
                    spawn.TempsRestantPatience--;
                    double pourcentage = (double)spawn.TempsRestantPatience / spawn.TempsPatience;

                    if (spawn.RectJauge != null)
                    {
                        spawn.RectJauge.Height = Math.Max(0, 100 * pourcentage);
                        SolidColorBrush color;
                        if (pourcentage > 0.5) color = new SolidColorBrush(Color.FromRgb(39, 201, 63));
                        else if (pourcentage > 0.25) color = new SolidColorBrush(Color.FromRgb(255, 187, 5));
                        else color = new SolidColorBrush(Color.FromRgb(255, 57, 57));
                        spawn.RectJauge.Fill = color;
                    }

                    if (spawn.TempsRestantPatience <= 0)
                    {
                        if (spawn.ImageClient != null) spawn.ImageClient.Visibility = Visibility.Hidden;
                        if (spawn.ImageCommande != null) spawn.ImageCommande.Visibility = Visibility.Hidden;
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Hidden;

                        Score -= MALUS_CLIENT_PARTI;
                        MettreAJourAffichageScore();
                        MettreAJourBoutonAmelioration();
                        spawn.MalusApplique = true;
                        AfficherMessageMalus();
                    }
                }
            }
        }

        private void GenererSpawnsAleatoires()
        {
            int tempsTotalSecondes = UCTemps.TempsChoisi * 60;
            int nombreClients = Math.Max(5, tempsTotalSecondes / 8);

            var clientsDisponibles = new (Image, Image, Border, Rectangle)[3]
            {
                (imgClient2, imgcmd1, borderJauge1, rectJauge1),
                (imgClient3, imgcmd2, borderJauge2, rectJauge2),
                (imgClient4, imgcmd3, borderJauge3, rectJauge3)
            };

            int[] tempsSpawns = new int[50];
            int tempsCount = 1;
            tempsSpawns[0] = tempsTotalSecondes - random.Next(3, 6);

            for (int i = 1; i < nombreClients; i++)
            {
                int tempsMin = (int)(tempsTotalSecondes * 0.05);
                int tempsMax = tempsTotalSecondes - 3;
                int temps = random.Next(tempsMin, tempsMax);

                bool tropProche = false;
                for (int j = 0; j < tempsCount; j++)
                {
                    if (Math.Abs(tempsSpawns[j] - temps) < 5)
                    {
                        tropProche = true;
                        break;
                    }
                }
                while (tropProche)
                {
                    temps = random.Next(tempsMin, tempsMax);
                    tropProche = false;
                    for (int j = 0; j < tempsCount; j++)
                    {
                        if (Math.Abs(tempsSpawns[j] - temps) < 5)
                        {
                            tropProche = true;
                            break;
                        }
                    }
                }

                tempsSpawns[tempsCount++] = temps;
            }

            Array.Sort(tempsSpawns, 0, tempsCount);
            Array.Reverse(tempsSpawns, 0, tempsCount);

            for (int i = 0; i < tempsCount; i++)
            {
                int temps = tempsSpawns[i];
                var clientChoisi = clientsDisponibles[random.Next(clientsDisponibles.Length)];
                int tempsPatience = random.Next(20, 41);

                listeSpawns[listeSpawnsCount++] = new ClientSpawn
                {
                    TempsSpawn = temps,
                    ImageClient = clientChoisi.Item1,
                    ImageCommande = clientChoisi.Item2,
                    BorderJauge = clientChoisi.Item3,
                    RectJauge = clientChoisi.Item4,
                    TempsPatience = tempsPatience,
                    TempsRestantPatience = tempsPatience
                };
            }
        }

        private void MettreAJourAffichageScore()
        {
            label_argent.Text = $"{Score}€";
        }

        private void MettreAJourBoutonAmelioration()
        {
            if (bouton_ameliorer == null)
                return;

            int tempsApresAmelioration = tempsCuissonActuel - REDUCTION_TEMPS;
            var textBlock = new TextBlock
            {
                Text = $"⚡ Améliorer ({COUT_AMELIORATION}€)\nTemps: {tempsApresAmelioration}s",
                TextAlignment = System.Windows.TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 9,
                Foreground = new SolidColorBrush(Colors.White)
            };

            bouton_ameliorer.Content = textBlock;
            bouton_ameliorer.IsEnabled = Score >= COUT_AMELIORATION && tempsCuissonActuel > 2;
            textBlock.Opacity = (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2) ? 1.0 : 0.4;
        }

        private void InitialiserTimer()
        {
            tempsRestant = TimeSpan.FromMinutes(UCTemps.TempsChoisi);
            label_timer.Text = tempsRestant.ToString(@"mm\:ss");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tempsRestant = tempsRestant.Subtract(TimeSpan.FromSeconds(1));
            label_timer.Text = tempsRestant.ToString(@"mm\:ss");

            int secondesRestantes = (int)tempsRestant.TotalSeconds;

            for (int i = 0; i < listeSpawnsCount; i++)
            {
                var spawn = listeSpawns[i];
                if (!spawn.Spawned && secondesRestantes == spawn.TempsSpawn)
                {
                    if (spawn.ImageClient != null && spawn.ImageClient.Visibility == Visibility.Hidden)
                    {
                        spawn.ImageClient.Visibility = Visibility.Visible;

                        if (spawn.ImageCommande != null) spawn.ImageCommande.Visibility = Visibility.Visible;
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Visible;

                        if (spawn.RectJauge != null)
                        {
                            spawn.RectJauge.Height = 100;
                            spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63));
                        }

                        spawn.Spawned = true;
                        spawn.MalusApplique = false;
                    }
                }
            }

            if (tempsRestant.TotalSeconds <= 0)
            {
                timer.Stop();
                timerJauges.Stop();
                label_timer.Text = "00:00";

                if (musiqueJeu != null) musiqueJeu.Stop();

                var mainWindow = Window.GetWindow(this);
                if (mainWindow != null && mainWindow.Content is Grid grid)
                {
                    grid.Children.Clear();
                    grid.Children.Add(new UCScore());
                }
            }
        }

        private void butQuitter(object sender, RoutedEventArgs e)
        {
            if (timer != null) timer.Stop();
            if (timerJauges != null) timerJauges.Stop();
            if (musiqueJeu != null) musiqueJeu.Stop();
            if (sonVente != null) sonVente.Stop();
            if (sonCuisson != null) sonCuisson.Stop();
            Application.Current.Shutdown();
        }

        private async void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe1.Visibility != Visibility.Hidden)
                return;

            Canvas.SetLeft(imgPoele, 395);
            Canvas.SetTop(imgPoele, 292);
            PoeleRotation.Angle = 90;
            imgCrepe1.Visibility = Visibility.Visible;
            tempsRestantPreparation = tempsCuissonActuel;
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";
            timerPreparation.Start();

            if (sonCuisson != null) sonCuisson.PlayLooping();
        }

        private void Timer_Preparation(object sender, EventArgs e)
        {
            tempsRestantPreparation--;
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";

            if (tempsRestantPreparation > 0)
                return;

            timerPreparation.Stop();

            if (imgCrepe2.Visibility == Visibility.Hidden)
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative));
                imgCrepe2.Visibility = Visibility.Visible;
                imgCrepe1.Visibility = Visibility.Hidden;
                Canvas.SetLeft(imgPoele, 312);
                Canvas.SetTop(imgPoele, 276);
                PoeleRotation.Angle = 0;

                if (sonCuisson != null) sonCuisson.Stop();

                txtTimer.Text = "";
            }
            else
            {
                txtTimer.Text = "Assiette occupée ! Vendez d'abord.";
            }
        }

        private async void bouton_ameliorer_Click(object sender, RoutedEventArgs e)
        {
            if (Score < COUT_AMELIORATION || tempsCuissonActuel <= 2)
                return;

            Score -= COUT_AMELIORATION;
            tempsCuissonActuel -= REDUCTION_TEMPS;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();

            if (labelMessageConfirmation != null)
            {
                labelMessageConfirmation.Content = $"Amélioration achetée ! Temps: {tempsCuissonActuel}s";
                labelMessageConfirmation.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                labelMessageConfirmation.Visibility = Visibility.Hidden;
            }
        }

        private async void bouton_vendre_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe2.Visibility != Visibility.Visible)
                return;

            string crepeActuelle = imgCrepe2.Source.ToString();
            bool crêpeTrouvée = false;

            for (int i = 0; i < clientsEtCommandes.Length; i++)
            {
                var (crepeType, client, cmd) = clientsEtCommandes[i];
                if (crepeActuelle.Contains(crepeType) && client.Visibility == Visibility.Visible)
                {
                    ClientSpawn spawn = null;
                    for (int j = 0; j < listeSpawnsCount; j++)
                    {
                        if (listeSpawns[j].ImageClient == client && listeSpawns[j].Spawned && !listeSpawns[j].MalusApplique)
                        {
                            spawn = listeSpawns[j];
                            break;
                        }
                    }

                    client.Visibility = Visibility.Hidden;
                    cmd.Visibility = Visibility.Hidden;
                    imgCrepe2.Visibility = Visibility.Hidden;

                    if (spawn != null)
                    {
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Hidden;
                        spawn.MalusApplique = true;
                    }

                    crêpeTrouvée = true;
                    break;
                }
            }

            if (!crêpeTrouvée)
            {
                labelMessageErreurVente.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                labelMessageErreurVente.Visibility = Visibility.Hidden;
                return;
            }

            if (sonVente != null) sonVente.Play();

            Score += PRIX_CREPE;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();
            imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative));

            if (imgCrepe1.Visibility == Visibility.Visible && tempsRestantPreparation <= 0)
            {
                imgCrepe2.Source = imgCrepe1.Source;
                imgCrepe2.Visibility = Visibility.Visible;
                imgCrepe1.Visibility = Visibility.Hidden;
                Canvas.SetLeft(imgPoele, 312);
                Canvas.SetTop(imgPoele, 276);
                PoeleRotation.Angle = 0;

                if (sonCuisson != null) sonCuisson.Stop();

                txtTimer.Text = "";
            }
        }

        private void GarnirCrepe(string crepePath)
        {
            if (imgCrepe2.Visibility == Visibility.Visible)
            {
                imgCrepe2.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative));
            }
            else if (tempsRestantPreparation <= 0 && imgCrepe1.Visibility == Visibility.Visible)
            {
                imgCrepe1.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative));
            }
        }

        private void but_nuttela(object sender, RoutedEventArgs e)
        {
            GarnirCrepe("/Images/crepes/crepe_nutella.png");
        }

        private void but_caramel(object sender, RoutedEventArgs e)
        {
            GarnirCrepe("/Images/crepes/crepe_caramele.png");
        }

        private void but_confutture(object sender, RoutedEventArgs e)
        {
            GarnirCrepe("/Images/crepes/crepe_confitture.png");
        }

        private void but_cmiel(object sender, RoutedEventArgs e)
        {
            GarnirCrepe("/Images/crepes/crepe_chevremiel.png");
        }

        private void but_sucre(object sender, RoutedEventArgs e)
        {
            GarnirCrepe("/Images/crepes/crepe_sucre.png");
        }

        private async void AfficherMessageMalus()
        {
            if (labelMessageErreurVente == null)
                return;

            labelMessageErreurVente.Content = $"Client parti ! -{MALUS_CLIENT_PARTI}€";
            labelMessageErreurVente.Visibility = Visibility.Visible;
            await Task.Delay(2500);

            if (labelMessageErreurVente != null) labelMessageErreurVente.Visibility = Visibility.Hidden;
        }
    }
}
