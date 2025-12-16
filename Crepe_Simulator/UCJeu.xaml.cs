using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
        // Timers
        private DispatcherTimer timer;
        private DispatcherTimer timerPreparation;
        private DispatcherTimer timerJauges;
        private TimeSpan tempsRestant;
        private int tempsRestantPreparation = 10;

        // Score et constantes
        public static int Score { get; set; } = 0;
        private const int PRIX_CREPE = 5;  // Fixé à 5€ tout le temps
        private const int MALUS_CLIENT_PARTI = 2;
        private const int COUT_AMELIORATION = 15;
        private const int REDUCTION_TEMPS = 2;
        private int tempsCuissonActuel = 10;

        // Système de clients
        private Random random = new Random();
        private List<ClientSpawn> listeSpawns = new List<ClientSpawn>();
        private List<(string type, Image client, Image cmd)> clientsEtCommandes = new List<(string, Image, Image)>();

        // Sons
        private SoundPlayer sonCuisson;
        private SoundPlayer sonVente;
        private MediaPlayer musiqueJeu;

        private class ClientSpawn
        {
            public int TempsSpawn;
            public Image ImageClient;
            public Image ImageCommande;
            public Border BorderJauge;
            public Rectangle RectJauge;
            public bool Spawned = false;
            public int TempsPatience = 30;
            public int TempsRestantPatience;
            public bool MalusApplique = false;
        }

        public UCJeu()
        {
            InitializeComponent();

            // Initialiser clients
            clientsEtCommandes.Add(("nutella", imgClient2, imgcmd1));
            clientsEtCommandes.Add(("caramele", imgClient3, imgcmd2));
            clientsEtCommandes.Add(("chevremiel", imgClient4, imgcmd3));

            GenererSpawnsAleatoires();
            InitialiserTimer();
            InitialiserTimerJauges();

            Score = 0;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();

            timerPreparation = new DispatcherTimer();
            timerPreparation.Interval = TimeSpan.FromSeconds(1);
            timerPreparation.Tick += Timer_Preparation;

            InitialiserSons();
        }

        private void InitialiserSons()
        {
            try
            {
                sonCuisson = new SoundPlayer(Application.GetResourceStream(
                    new Uri("pack://application:,,,/sons/son_cuisson.wav")).Stream);
                sonCuisson.Load();

                sonVente = new SoundPlayer(Application.GetResourceStream(
                    new Uri("pack://application:,,,/sons/vente_son.wav")).Stream);
                sonVente.Load();

                musiqueJeu = new MediaPlayer();
                musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/son_jeu.mp3"));
                musiqueJeu.MediaEnded += (s, e) => { musiqueJeu.Position = TimeSpan.Zero; musiqueJeu.Play(); };
                musiqueJeu.Volume = 0.3;
                musiqueJeu.Play();
            }
            catch
            {
                // Si les sons n'existent pas, on continue sans
            }
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
            for (int i = 0; i < listeSpawns.Count; i++)
            {
                ClientSpawn spawn = listeSpawns[i];

                if (!spawn.Spawned || spawn.MalusApplique || spawn.ImageClient.Visibility != Visibility.Visible)
                    continue;

                spawn.TempsRestantPatience--;
                double pourcentage = (double)spawn.TempsRestantPatience / spawn.TempsPatience;
                spawn.RectJauge.Height = Math.Max(0, 100 * pourcentage);

                if (pourcentage > 0.5)
                    spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63));
                else if (pourcentage > 0.25)
                    spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(255, 187, 5));
                else
                    spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(255, 57, 57));

                if (spawn.TempsRestantPatience <= 0)
                {
                    spawn.ImageClient.Visibility = Visibility.Hidden;
                    spawn.ImageCommande.Visibility = Visibility.Hidden;
                    spawn.BorderJauge.Visibility = Visibility.Hidden;
                    spawn.MalusApplique = true;

                    Score -= MALUS_CLIENT_PARTI;
                    MettreAJourAffichageScore();
                    MettreAJourBoutonAmelioration();
                    AfficherMessageMalus();
                }
            }
        }

        private void GenererSpawnsAleatoires()
        {
            int tempsTotalSecondes = UCTemps.TempsChoisi * 60;
            int nombreClients = Math.Max(5, tempsTotalSecondes / 8);

            var clientsDisponibles = new List<(Image, Image, Border, Rectangle)>
            {
                (imgClient2, imgcmd1, borderJauge1, rectJauge1),
                (imgClient3, imgcmd2, borderJauge2, rectJauge2),
                (imgClient4, imgcmd3, borderJauge3, rectJauge3)
            };

            List<int> tempsSpawns = new List<int>();
            tempsSpawns.Add(tempsTotalSecondes - random.Next(3, 6));

            for (int i = 1; i < nombreClients; i++)
            {
                int temps = random.Next((int)(tempsTotalSecondes * 0.05), tempsTotalSecondes - 3);

                while (tempsSpawns.Any(t => Math.Abs(t - temps) < 5))
                    temps = random.Next((int)(tempsTotalSecondes * 0.05), tempsTotalSecondes - 3);

                tempsSpawns.Add(temps);
            }

            tempsSpawns.Sort((a, b) => b.CompareTo(a));

            for (int i = 0; i < tempsSpawns.Count; i++)
            {
                var client = clientsDisponibles[random.Next(clientsDisponibles.Count)];
                int patience = random.Next(20, 41);

                listeSpawns.Add(new ClientSpawn
                {
                    TempsSpawn = tempsSpawns[i],
                    ImageClient = client.Item1,
                    ImageCommande = client.Item2,
                    BorderJauge = client.Item3,
                    RectJauge = client.Item4,
                    TempsPatience = patience,
                    TempsRestantPatience = patience
                });
            }
        }

        private void MettreAJourAffichageScore()
        {
            label_argent.Text = $"{Score}€";
        }

        private void MettreAJourBoutonAmelioration()
        {
            if (bouton_ameliorer == null) return;

            int tempsApres = tempsCuissonActuel - REDUCTION_TEMPS;
            TextBlock textBlock = new TextBlock
            {
                Text = $"⚡ Améliorer ({COUT_AMELIORATION}€)\nTemps: {tempsApres}s",
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 9,
                Foreground = new SolidColorBrush(Colors.White)
            };

            bouton_ameliorer.Content = textBlock;
            bouton_ameliorer.IsEnabled = Score >= COUT_AMELIORATION && tempsCuissonActuel > 2;
            textBlock.Opacity = bouton_ameliorer.IsEnabled ? 1.0 : 0.4;
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

            for (int i = 0; i < listeSpawns.Count; i++)
            {
                ClientSpawn spawn = listeSpawns[i];

                if (spawn.Spawned || secondesRestantes != spawn.TempsSpawn) continue;
                if (spawn.ImageClient.Visibility == Visibility.Visible) continue;

                spawn.ImageClient.Visibility = Visibility.Visible;
                spawn.ImageCommande.Visibility = Visibility.Visible;
                spawn.BorderJauge.Visibility = Visibility.Visible;
                spawn.RectJauge.Height = 100;
                spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63));
                spawn.Spawned = true;
                spawn.MalusApplique = false;
            }

            if (tempsRestant.TotalSeconds <= 0)
            {
                timer.Stop();
                timerJauges.Stop();
                if (musiqueJeu != null) musiqueJeu.Stop();

                Window mainWindow = Window.GetWindow(this);
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
            if (sonCuisson != null) sonCuisson.Stop();
            Application.Current.Shutdown();
        }

        private void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe1.Visibility != Visibility.Hidden) return;

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

            if (tempsRestantPreparation > 0) return;

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
            if (Score < COUT_AMELIORATION || tempsCuissonActuel <= 2) return;

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
            if (imgCrepe2.Visibility != Visibility.Visible) return;

            string crepeActuelle = imgCrepe2.Source.ToString();
            bool trouvee = false;

            for (int i = 0; i < clientsEtCommandes.Count; i++)
            {
                var (type, client, cmd) = clientsEtCommandes[i];

                if (!crepeActuelle.Contains(type) || client.Visibility != Visibility.Visible)
                    continue;

                for (int j = 0; j < listeSpawns.Count; j++)
                {
                    if (listeSpawns[j].ImageClient == client && listeSpawns[j].Spawned)
                    {
                        listeSpawns[j].BorderJauge.Visibility = Visibility.Hidden;
                        listeSpawns[j].MalusApplique = true;
                    }
                }

                client.Visibility = Visibility.Hidden;
                cmd.Visibility = Visibility.Hidden;
                imgCrepe2.Visibility = Visibility.Hidden;
                trouvee = true;
                break;
            }

            if (trouvee)
            {
                if (sonVente != null) sonVente.Play();
                Score += PRIX_CREPE;
                MettreAJourAffichageScore();
                MettreAJourBoutonAmelioration();
                AfficherMessageGain();  // Nouveau : Affiche le gain pour confirmer que c'est toujours 5€
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
            else
            {
                labelMessageErreurVente.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                labelMessageErreurVente.Visibility = Visibility.Hidden;
            }
        }

        private void GarnirCrepe(string crepePath)
        {
            if (imgCrepe2.Visibility == Visibility.Visible)
                imgCrepe2.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative));
            else if (tempsRestantPreparation <= 0 && imgCrepe1.Visibility == Visibility.Visible)
                imgCrepe1.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative));
        }

        private void but_nuttela(object sender, RoutedEventArgs e) => GarnirCrepe("/Images/crepes/crepe_nutella.png");
        private void but_caramel(object sender, RoutedEventArgs e) => GarnirCrepe("/Images/crepes/crepe_caramele.png");
        private void but_confutture(object sender, RoutedEventArgs e) => GarnirCrepe("/Images/crepes/crepe_confitture.png");
        private void but_cmiel(object sender, RoutedEventArgs e) => GarnirCrepe("/Images/crepes/crepe_chevremiel.png");
        private void but_sucre(object sender, RoutedEventArgs e) => GarnirCrepe("/Images/crepes/crepe_sucre.png");

        // Nouveau : Méthode pour afficher le gain (toujours +5€)
        private async void AfficherMessageGain()
        {
            if (labelMessageErreurVente == null) return;

            labelMessageErreurVente.Content = $"Crêpe vendue ! +{PRIX_CREPE}€";
            labelMessageErreurVente.Visibility = Visibility.Visible;
            await Task.Delay(2500);
            if (labelMessageErreurVente != null)
                labelMessageErreurVente.Visibility = Visibility.Hidden;
        }

        private async void AfficherMessageMalus()
        {
            if (labelMessageErreurVente == null) return;

            labelMessageErreurVente.Content = $"Client parti ! -{MALUS_CLIENT_PARTI}€";
            labelMessageErreurVente.Visibility = Visibility.Visible;
            await Task.Delay(2500);
            if (labelMessageErreurVente != null)
                labelMessageErreurVente.Visibility = Visibility.Hidden;
        }
    }
}
