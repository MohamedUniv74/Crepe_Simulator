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
    /// <summary>
    /// Logique d'interaction pour UCJeu.xaml
    /// </summary>
    public partial class UCJeu : UserControl
    {
        private DispatcherTimer timer;
        private TimeSpan tempsRestant;
        DispatcherTimer timerPreparation;
        int tempsRestantPreparation = 10;

        // Timer pour les jauges des clients
        private DispatcherTimer timerJauges;

        // Verrouillage pour éviter les malus multiples simultanés
        private bool traitementMalusEnCours = false;

        // Variable pour le score
        public static int Score { get; set; } = 0;
        private const int PRIX_CREPE = 5; // Prix par crêpe vendue
        private const int MALUS_CLIENT_PARTI = 2; // Malus quand un client part sans être servi

        // Système d'amélioration
        private int tempsCuissonActuel = 10; // Temps de cuisson actuel
        private const int COUT_AMELIORATION = 15; // Coût de l'amélioration
        private const int REDUCTION_TEMPS = 2; // Réduction de temps par amélioration

        // Système de spawn régulier et aléatoire des clients
        private Random random = new Random();
        private List<ClientSpawn> listeSpawns = new List<ClientSpawn>();

        // Sons
        private SoundPlayer sonCuisson;
        private MediaPlayer musiqueJeu;
        private MediaPlayer sonVente;

        // Classe pour stocker les informations de spawn
        private class ClientSpawn
        {
            public int TempsSpawn { get; set; }
            public Image ImageClient { get; set; }
            public Image ImageCommande { get; set; }
            public Border BorderJauge { get; set; }
            public Rectangle RectJauge { get; set; }
            public bool Spawned { get; set; } = false;
            public int TempsPatience { get; set; } = 30; // Temps de patience initial (en secondes)
            public int TempsRestantPatience { get; set; } // Temps restant
            public bool MalusApplique { get; set; } = false; // Pour éviter d'appliquer le malus plusieurs fois
            public bool ClientServi { get; set; } = false; // Pour savoir si le client a été servi
        }

        // Liste des clients et leurs commandes pour simplifier la vente
        private List<(string CrepeType, Image Client, Image Commande)> clientsEtCommandes = new List<(string, Image, Image)>();

        public UCJeu()
        {
            InitializeComponent();

            // Initialiser la liste des clients et commandes
            clientsEtCommandes.Add(("nutella", imgClient2, imgcmd1));
            clientsEtCommandes.Add(("caramele", imgClient3, imgcmd2));
            clientsEtCommandes.Add(("chevremiel", imgClient4, imgcmd3));

            // IMPORTANT : Générer les temps de spawn AVANT d'initialiser le timer
            GenererSpawnsAleatoires();

            InitialiserTimer();

            // Initialiser le timer des jauges
            InitialiserTimerJauges();

            // Initialisation du score à 0
            Score = 0;
            MettreAJourAffichageScore();
            MettreAJourBoutonAmelioration();

            timerPreparation = new DispatcherTimer();
            timerPreparation.Interval = TimeSpan.FromSeconds(1);
            timerPreparation.Tick += Timer_Preparation;

            // Initialiser les sons
            InitialiserSonCuisson();
            InitialiserMusiqueJeu();
            InitialiserSonVente();
        }

        // Méthode pour initialiser le son de cuisson
        private void InitialiserSonCuisson()
        {
            try
            {
                sonCuisson = new SoundPlayer(Application.GetResourceStream(
                    new Uri("pack://application:,,,/sons/son_cuisson.wav")).Stream);
                sonCuisson.Load(); // Précharger le son
            }
            catch (Exception ex)
            {
                // Si le fichier son n'existe pas, on continue sans son
                System.Diagnostics.Debug.WriteLine("Erreur chargement son cuisson: " + ex.Message);
            }
        }

        // Méthode pour initialiser la musique de fond du jeu
        private void InitialiserMusiqueJeu()
        {
            try
            {
                musiqueJeu = new MediaPlayer();
                musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/son_jeu.mp3"));
                musiqueJeu.MediaEnded += RelancerMusiqueJeu;
                musiqueJeu.Volume = 0.3; // Volume modéré pour ne pas couvrir les autres sons
                musiqueJeu.Play();
            }
            catch (Exception ex)
            {
                // Si le fichier musique n'existe pas, on continue sans musique
                System.Diagnostics.Debug.WriteLine("Erreur chargement musique jeu: " + ex.Message);
            }
        }

        // Méthode pour relancer la musique en boucle
        private void RelancerMusiqueJeu(object sender, EventArgs e)
        {
            if (musiqueJeu != null)
            {
                musiqueJeu.Position = TimeSpan.Zero;
                musiqueJeu.Play();
            }
        }

        // Méthode pour initialiser le son de vente
        private void InitialiserSonVente()
        {
            try
            {
                sonVente = new MediaPlayer();
                sonVente.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/vente_son.mp3"));
                sonVente.Volume = 0.5; // Volume modéré
            }
            catch (Exception ex)
            {
                // Si le fichier son n'existe pas, on continue sans son
                System.Diagnostics.Debug.WriteLine("Erreur chargement son vente: " + ex.Message);
            }
        }

        // Méthode pour initialiser le timer des jauges
        private void InitialiserTimerJauges()
        {
            timerJauges = new DispatcherTimer();
            timerJauges.Interval = TimeSpan.FromSeconds(1);
            timerJauges.Tick += TimerJauges_Tick;
            timerJauges.Start();
        }

        // Méthode appelée à chaque seconde pour mettre à jour les jauges
        private void TimerJauges_Tick(object sender, EventArgs e)
        {
            // CRITIQUE : Créer une copie de la liste pour éviter les modifications pendant l'itération
            // Ne traiter que les spawns qui sont actifs ET qui n'ont pas encore eu leur malus appliqué
            var spawnsActifs = listeSpawns.Where(s => s.Spawned && !s.MalusApplique && s.ImageClient != null && s.ImageClient.Visibility == Visibility.Visible).ToList();

            foreach (var spawn in spawnsActifs)
            {
                try
                {
                    // Décrémenter le temps restant
                    spawn.TempsRestantPatience--;

                    // Calculer le pourcentage restant
                    double pourcentage = (double)spawn.TempsRestantPatience / spawn.TempsPatience;

                    // Mettre à jour la hauteur de la jauge
                    if (spawn.RectJauge != null)
                    {
                        spawn.RectJauge.Height = Math.Max(0, 100 * pourcentage);

                        // Changer la couleur selon le temps restant
                        if (pourcentage > 0.5)
                        {
                            spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63)); // Vert
                        }
                        else if (pourcentage > 0.25)
                        {
                            spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(255, 187, 5)); // Orange
                        }
                        else
                        {
                            spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(255, 57, 57)); // Rouge
                        }
                    }

                    // Si le temps est écoulé ET que le malus n'a pas encore été appliqué
                    if (spawn.TempsRestantPatience <= 0)
                    {
                        if (spawn.ImageClient != null)
                            spawn.ImageClient.Visibility = Visibility.Hidden;
                        if (spawn.ImageCommande != null)
                            spawn.ImageCommande.Visibility = Visibility.Hidden;
                        if (spawn.BorderJauge != null)
                            spawn.BorderJauge.Visibility = Visibility.Hidden;

                        // Appliquer le malus UNE SEULE FOIS
                        Score -= MALUS_CLIENT_PARTI;
                        MettreAJourAffichageScore();
                        MettreAJourBoutonAmelioration();

                        // Marquer que le malus a été appliqué
                        spawn.MalusApplique = true;

                        // Afficher un message de pénalité
                        AfficherMessageMalus();
                    }
                }
                catch (Exception ex)
                {
                    // Log l'erreur mais continue le jeu
                    System.Diagnostics.Debug.WriteLine($"Erreur mise à jour jauge: {ex.Message}");
                }
            }
        }

        // Méthode pour générer plusieurs spawns aléatoires
        private void GenererSpawnsAleatoires()
        {
            int tempsTotalSecondes = UCTemps.TempsChoisi * 60;

            // Calculer le nombre de clients en fonction du temps (1 client toutes les 8-10 secondes)
            int nombreClients = Math.Max(5, tempsTotalSecondes / 8);

            // Créer une liste de tous les clients disponibles avec leurs jauges
            List<(Image client, Image commande, Border borderJauge, Rectangle rectJauge)> clientsDisponibles = new List<(Image, Image, Border, Rectangle)>
            {
                (imgClient2, imgcmd1, borderJauge1, rectJauge1),
                (imgClient3, imgcmd2, borderJauge2, rectJauge2),
                (imgClient4, imgcmd3, borderJauge3, rectJauge3)
            };

            // Générer des temps de spawn répartis sur toute la durée
            List<int> tempsSpawns = new List<int>();

            // Premier client apparaît très rapidement (dans les 3-5 premières secondes)
            tempsSpawns.Add(tempsTotalSecondes - random.Next(3, 6));

            // Générer le reste des spawns
            for (int i = 1; i < nombreClients; i++)
            {
                int tempsMin = (int)(tempsTotalSecondes * 0.05);
                int tempsMax = tempsTotalSecondes - 3;
                int temps = random.Next(tempsMin, tempsMax);

                // Minimum 5 secondes d'écart entre chaque spawn
                while (tempsSpawns.Any(t => Math.Abs(t - temps) < 5))
                {
                    temps = random.Next(tempsMin, tempsMax);
                }

                tempsSpawns.Add(temps);
            }

            // Trier les temps de spawn par ordre décroissant
            tempsSpawns.Sort((a, b) => b.CompareTo(a));

            // Créer les spawns avec des clients aléatoires
            foreach (int temps in tempsSpawns)
            {
                var clientChoisi = clientsDisponibles[random.Next(clientsDisponibles.Count)];

                // Temps de patience aléatoire entre 20 et 40 secondes
                int tempsPatience = random.Next(20, 41);

                listeSpawns.Add(new ClientSpawn
                {
                    TempsSpawn = temps,
                    ImageClient = clientChoisi.client,
                    ImageCommande = clientChoisi.commande,
                    BorderJauge = clientChoisi.borderJauge,
                    RectJauge = clientChoisi.rectJauge,
                    TempsPatience = tempsPatience,
                    TempsRestantPatience = tempsPatience,
                    MalusApplique = false,
                    ClientServi = false
                });
            }
        }

        // Méthode pour mettre à jour l'affichage du score
        private void MettreAJourAffichageScore()
        {
            label_argent.Text = $"{Score}€";
        }

        // Méthode pour mettre à jour le bouton d'amélioration
        private void MettreAJourBoutonAmelioration()
        {
            if (bouton_ameliorer != null)
            {
                int tempsApresAmelioration = tempsCuissonActuel - REDUCTION_TEMPS;

                // Créer un TextBlock pour pouvoir contrôler l'opacité du texte
                TextBlock textBlock = new TextBlock
                {
                    Text = $"⚡ Améliorer ({COUT_AMELIORATION}€)\nTemps: {tempsApresAmelioration}s",
                    TextAlignment = System.Windows.TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                bouton_ameliorer.Content = textBlock;

                // Désactiver le bouton si pas assez d'argent ou temps minimum atteint
                bouton_ameliorer.IsEnabled = Score >= COUT_AMELIORATION && tempsCuissonActuel > 2;

                // Changer l'opacité du texte si désactivé
                textBlock.Opacity = (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2) ? 1.0 : 0.4;
            }
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

            // CRITIQUE : Créer une copie pour éviter les problèmes
            var spawnsAVerifier = listeSpawns.Where(s => !s.Spawned).ToList();

            foreach (var spawn in spawnsAVerifier)
            {
                if (secondesRestantes == spawn.TempsSpawn)
                {
                    try
                    {
                        // Vérifier que le client n'est pas déjà visible
                        if (spawn.ImageClient != null && spawn.ImageClient.Visibility == Visibility.Hidden)
                        {
                            spawn.ImageClient.Visibility = Visibility.Visible;

                            if (spawn.ImageCommande != null)
                                spawn.ImageCommande.Visibility = Visibility.Visible;
                            if (spawn.BorderJauge != null)
                                spawn.BorderJauge.Visibility = Visibility.Visible;

                            // Réinitialiser la jauge à 100%
                            if (spawn.RectJauge != null)
                            {
                                spawn.RectJauge.Height = 100;
                                spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63)); // Vert
                            }

                            spawn.Spawned = true;
                            spawn.MalusApplique = false; // Réinitialiser le flag
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur spawn client: {ex.Message}");
                    }
                }
            }

            if (tempsRestant.TotalSeconds <= 0)
            {
                timer.Stop();
                timerJauges.Stop(); // Arrêter le timer des jauges
                label_timer.Text = "00:00";

                // Arrêter la musique de fond quand le jeu se termine
                if (musiqueJeu != null)
                {
                    musiqueJeu.Stop();
                }

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
            if (timer != null)
            {
                timer.Stop();
            }
            if (timerJauges != null)
            {
                timerJauges.Stop();
            }

            // Arrêter la musique en quittant
            if (musiqueJeu != null)
            {
                musiqueJeu.Stop();
            }
            if (sonVente != null)
            {
                sonVente.Stop();
            }

            Application.Current.Shutdown();
        }

        private async void bouton_preparer_Click(object sender, RoutedEventArgs e)
        {
            // On ne peut préparer que si aucune crêpe n'est en train de cuire
            if (imgCrepe1.Visibility == Visibility.Hidden)
            {
                double nouvellePositionX = 395;
                double nouvellePositionY = 292;

                Canvas.SetLeft(imgPoele, nouvellePositionX);
                Canvas.SetTop(imgPoele, nouvellePositionY);

                PoeleRotation.Angle = 90;

                imgCrepe1.Visibility = Visibility.Visible;

                // Utiliser le temps de cuisson actuel
                tempsRestantPreparation = tempsCuissonActuel;
                txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";
                timerPreparation.Start();

                // Jouer le son de cuisson EN BOUCLE
                if (sonCuisson != null)
                {
                    sonCuisson.PlayLooping(); // Joue en boucle pendant la cuisson
                }
            }
        }

        private void Timer_Preparation(object sender, EventArgs e)
        {
            tempsRestantPreparation--;
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s";

            if (tempsRestantPreparation <= 0)
            {
                timerPreparation.Stop();

                // Vérifier si l'assiette est libre avant de mettre la crêpe
                if (imgCrepe2.Visibility == Visibility.Hidden)
                {
                    // L'assiette est libre, on peut y mettre la crêpe
                    // Réinitialiser la crêpe à l'image de base
                    imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative));
                    imgCrepe2.Visibility = Visibility.Visible;
                    imgCrepe1.Visibility = Visibility.Hidden;

                    // Remettre la poêle à sa position normale
                    Canvas.SetLeft(imgPoele, 312);
                    Canvas.SetTop(imgPoele, 276);
                    PoeleRotation.Angle = 0;

                    // Arrêter le son car la poêle quitte la plaque
                    if (sonCuisson != null)
                    {
                        sonCuisson.Stop();
                    }

                    txtTimer.Text = "";
                }
                else
                {
                    // L'assiette est occupée, la crêpe cuite reste dans la poêle
                    // LA POÊLE RESTE INCLINÉE SUR LA PLAQUE
                    // LE SON CONTINUE DE JOUER car la poêle est toujours sur la plaque
                    // On garde imgCrepe1 visible pour pouvoir la garnir
                    txtTimer.Text = "Assiette occupée ! Vendez d'abord.";
                }
            }
        }

        private async void bouton_ameliorer_Click(object sender, RoutedEventArgs e)
        {
            if (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2)
            {
                // Déduire le coût
                Score -= COUT_AMELIORATION;

                // Réduire le temps de cuisson
                tempsCuissonActuel -= REDUCTION_TEMPS;

                // Mettre à jour l'affichage
                MettreAJourAffichageScore();
                MettreAJourBoutonAmelioration();

                // Afficher un message de confirmation
                if (labelMessageConfirmation != null)
                {
                    labelMessageConfirmation.Content = $"Amélioration achetée ! Temps: {tempsCuissonActuel}s";
                    labelMessageConfirmation.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    labelMessageConfirmation.Visibility = Visibility.Hidden;
                }
            }
        }

        // Bouton Vendre avec augmentation du score
        private async void bouton_vendre_Click(object sender, RoutedEventArgs e)
        {
            if (imgCrepe2.Visibility == Visibility.Visible)
            {
                // Récupérer le nom de l'image de la crêpe
                string crepeActuelle = imgCrepe2.Source.ToString();

                bool crêpeTrouvée = false; // Booléen pour savoir si une crêpe correspond

                // Utiliser une boucle pour vérifier quelle crêpe correspond
                foreach (var (crepeType, client, cmd) in clientsEtCommandes)
                {
                    if (crepeActuelle.Contains(crepeType) && client.Visibility == Visibility.Visible)
                    {
                        // IMPORTANT : Trouver le spawn correspondant AVANT de cacher le client
                        var spawn = listeSpawns.FirstOrDefault(s => s.ImageClient == client && s.Spawned && !s.MalusApplique);

                        // Cacher tous les éléments visuels
                        client.Visibility = Visibility.Hidden;
                        cmd.Visibility = Visibility.Hidden;
                        imgCrepe2.Visibility = Visibility.Hidden;

                        // Cacher la jauge et empêcher le malus
                        if (spawn != null)
                        {
                            if (spawn.BorderJauge != null)
                                spawn.BorderJauge.Visibility = Visibility.Hidden;
                            spawn.MalusApplique = true; // CRITIQUE : Empêcher le malus puisque le client a été servi
                        }

                        crêpeTrouvée = true;
                        break; // Sortir de la boucle dès qu'une correspondance est trouvée
                    }
                }

                // Si une crêpe a été vendue, augmenter le score
                if (crêpeTrouvée)
                {
                    Score += PRIX_CREPE;
                    MettreAJourAffichageScore();
                    MettreAJourBoutonAmelioration(); // Mettre à jour le bouton d'amélioration

                    // IMPORTANT : Réinitialiser l'image de la crêpe à la crêpe de base
                    imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative));

                    // NOUVEAU : Vérifier s'il y a une crêpe en attente dans la poêle
                    if (imgCrepe1.Visibility == Visibility.Visible && tempsRestantPreparation <= 0)
                    {
                        // Transférer la crêpe de la poêle vers l'assiette
                        imgCrepe2.Source = imgCrepe1.Source;
                        imgCrepe2.Visibility = Visibility.Visible;
                        imgCrepe1.Visibility = Visibility.Hidden;

                        // NOUVEAU : Remettre la poêle à sa position normale après le transfert
                        Canvas.SetLeft(imgPoele, 312);
                        Canvas.SetTop(imgPoele, 276);
                        PoeleRotation.Angle = 0;

                        // Arrêter le son car la poêle quitte la plaque
                        if (sonCuisson != null)
                        {
                            sonCuisson.Stop();
                        }

                        // Effacer le message d'assiette occupée
                        txtTimer.Text = "";
                    }
                }
                else
                {
                    // Si aucune crêpe ne correspond, afficher le message
                    labelMessageErreurVente.Visibility = Visibility.Visible;
                    await Task.Delay(3000);
                    labelMessageErreurVente.Visibility = Visibility.Hidden;
                }
            }
        }

        // Méthode commune pour garnir une crêpe
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

        // Méthode pour afficher un message de malus
        private async void AfficherMessageMalus()
        {
            try
            {
                if (labelMessageErreurVente != null)
                {
                    labelMessageErreurVente.Content = $"Client parti ! -{MALUS_CLIENT_PARTI}€";
                    labelMessageErreurVente.Visibility = Visibility.Visible;
                    await Task.Delay(2500);

                    // Vérifier que le label existe toujours avant de le cacher
                    if (labelMessageErreurVente != null)
                    {
                        labelMessageErreurVente.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur affichage message malus: {ex.Message}");
            }
        }
    }
}