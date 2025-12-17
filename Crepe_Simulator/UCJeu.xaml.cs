using System; // Importation de l'espace de noms System pour les types de base
using System.Collections.Generic; // Importation pour les collections génériques comme List<T>
using System.Diagnostics.Eventing.Reader; // Importation pour la lecture des événements de diagnostic
using System.Linq; // Importation pour LINQ (Language Integrated Query)
using System.Media; // Importation pour la lecture de sons (SoundPlayer)
using System.Numerics; // Importation pour les types numériques avancés
using System.Threading.Tasks; // Importation pour la programmation asynchrone avec Task
using System.Windows; // Importation pour les types de base de WPF
using System.Windows.Controls; // Importation pour les contrôles WPF (Button, Label, etc.)
using System.Windows.Media; // Importation pour les médias WPF (couleurs, brushes)
using System.Windows.Media.Imaging; // Importation pour la gestion des images
using System.Windows.Shapes; // Importation pour les formes géométriques WPF
using System.Windows.Threading; // Importation pour le timer et dispatcher de WPF

namespace Crepe_Simulator // Déclaration de l'espace de noms du projet
{
    public partial class UCJeu : UserControl // Déclaration de la classe UCJeu qui hérite de UserControl
    {
        private DispatcherTimer timer; // Timer principal pour le compte à rebours du jeu
        private TimeSpan tempsRestant; // Temps restant dans la partie
        DispatcherTimer timerPreparation; // Timer pour le temps de cuisson des crêpes
        int tempsRestantPreparation = 10; // Temps restant pour la préparation actuelle (initialisé à 10s)
        private DispatcherTimer timerJauges; // Timer pour mettre à jour les jauges de patience des clients
        private bool traitementMalusEnCours = false; // Flag pour éviter de traiter plusieurs malus simultanément
        public static int Score { get; set; } = 0; // Score actuel du joueur (statique pour y accéder depuis d'autres classes)
        public static readonly int PRIX_CREPE = 5; // Constante : prix de vente d'une crêpe (5€)
        public static readonly int MALUS_CLIENT_PARTI = 2; // Constante : malus quand un client part sans être servi (2€)
        private int tempsCuissonActuel = 10; // Temps de cuisson actuel des crêpes (peut être amélioré)
        public static readonly int COUT_AMELIORATION = 15; // Constante : coût pour améliorer le temps de cuisson (15€)
        public static readonly int REDUCTION_TEMPS = 2; // Constante : réduction du temps de cuisson par amélioration (2s)
        private Random random = new Random(); // Générateur de nombres aléatoires pour les spawns
        private ClientSpawn[] listeSpawns = new ClientSpawn[50]; // Tableau pour stocker les apparitions de clients (max 50)
        private int listeSpawnsCount = 0; // Compteur du nombre d'éléments dans listeSpawns
        private SoundPlayer sonCuisson; // Lecteur audio pour le son de cuisson
        private MediaPlayer musiqueJeu; // Lecteur audio pour la musique de fond
        private SoundPlayer sonVente; // Lecteur audio pour le son de vente
        private (string CrepeType, Image Client, Image Commande)[] clientsEtCommandes = new (string, Image, Image)[3]; // Tableau de tuples associant type de crêpe, image client et image commande

        private class ClientSpawn // Classe interne pour représenter l'apparition d'un client
        {
            public int TempsSpawn { get; set; } // Temps (en secondes) auquel le client apparaît
            public Image ImageClient { get; set; } // Image du client dans l'interface
            public Image ImageCommande { get; set; } // Image de la commande du client
            public Border BorderJauge { get; set; } // Bordure de la jauge de patience
            public Rectangle RectJauge { get; set; } // Rectangle représentant la jauge de patience
            public bool Spawned { get; set; } = false; // Indique si le client est déjà apparu
            public int TempsPatience { get; set; } = 30; // Temps de patience initial du client (30s par défaut)
            public int TempsRestantPatience { get; set; } // Temps de patience restant du client
            public bool MalusApplique { get; set; } = false; // Indique si le malus a déjà été appliqué pour ce client
            public bool ClientServi { get; set; } = false; // Indique si le client a été servi
        }

        public UCJeu() // Constructeur de la classe UCJeu
        {
            InitializeComponent(); // Initialise les composants graphiques définis en XAML

            clientsEtCommandes[0] = ("nutella", imgClient2, imgcmd1); // Association : crêpe nutella avec client 2 et commande 1
            clientsEtCommandes[1] = ("caramele", imgClient3, imgcmd2); // Association : crêpe caramel avec client 3 et commande 2
            clientsEtCommandes[2] = ("chevremiel", imgClient4, imgcmd3); // Association : crêpe chèvre-miel avec client 4 et commande 3

            GenererSpawnsAleatoires(); // Génère les moments d'apparition aléatoires des clients
            InitialiserTimer(); // Initialise le timer principal du jeu
            InitialiserTimerJauges(); // Initialise le timer de mise à jour des jauges

            Score = 0; // Réinitialise le score à 0
            MettreAJourAffichageScore(); // Met à jour l'affichage du score
            MettreAJourBoutonAmelioration(); // Met à jour l'état du bouton d'amélioration

            timerPreparation = new DispatcherTimer(); // Crée un nouveau timer pour la préparation
            timerPreparation.Interval = TimeSpan.FromSeconds(1); // Configure l'intervalle à 1 seconde
            timerPreparation.Tick += Timer_Preparation; // Abonne la méthode Timer_Preparation à l'événement Tick

            InitialiserSonCuisson(); // Initialise le son de cuisson
            InitialiserMusiqueJeu(); // Initialise la musique de fond
            InitialiserSonVente(); // Initialise le son de vente
        }

        private void InitialiserSonCuisson() // Méthode pour initialiser le son de cuisson
        {
            sonCuisson = new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/sons/son_cuisson.wav")).Stream); // Charge le fichier audio depuis les ressources
            sonCuisson.Load(); // Précharge le son en mémoire
        }

        private void InitialiserMusiqueJeu() // Méthode pour initialiser la musique de fond
        {
            musiqueJeu = new MediaPlayer(); // Crée un nouveau MediaPlayer
            musiqueJeu.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "sons/son_jeu.mp3")); // Ouvre le fichier mp3 depuis le répertoire de l'application
            musiqueJeu.MediaEnded += RelancerMusiqueJeu; // Abonne la méthode RelancerMusiqueJeu pour boucler la musique
            musiqueJeu.Volume = 0.3; // Définit le volume à 30%
            musiqueJeu.Play(); // Démarre la lecture de la musique
        }

        private void RelancerMusiqueJeu(object sender, EventArgs e) // Méthode appelée quand la musique se termine
        {
            if (musiqueJeu != null) // Vérifie que le lecteur existe
            {
                musiqueJeu.Position = TimeSpan.Zero; // Remet la position au début
                musiqueJeu.Play(); // Relance la lecture (effet de boucle)
            }
        }

        private void InitialiserSonVente() // Méthode pour initialiser le son de vente
        {
            sonVente = new SoundPlayer(Application.GetResourceStream(new Uri("pack://application:,,,/sons/vente_son.wav")).Stream); // Charge le fichier audio depuis les ressources
            sonVente.Load(); // Précharge le son en mémoire
        }

        private void InitialiserTimerJauges() // Méthode pour initialiser le timer des jauges
        {
            timerJauges = new DispatcherTimer(); // Crée un nouveau timer
            timerJauges.Interval = TimeSpan.FromSeconds(1); // Configure l'intervalle à 1 seconde
            timerJauges.Tick += TimerJauges_Tick; // Abonne la méthode TimerJauges_Tick à l'événement Tick
            timerJauges.Start(); // Démarre le timer
        }

        private void TimerJauges_Tick(object sender, EventArgs e) // Méthode appelée chaque seconde pour mettre à jour les jauges
        {
            for (int i = 0; i < listeSpawnsCount; i++) // Parcourt tous les clients générés
            {
                var spawn = listeSpawns[i]; // Récupère le client actuel
                if (spawn.Spawned && !spawn.MalusApplique && spawn.ImageClient != null && spawn.ImageClient.Visibility == Visibility.Visible) // Vérifie que le client est apparu, visible et n'a pas déjà eu son malus
                {
                    spawn.TempsRestantPatience--; // Décrémente le temps de patience restant
                    double pourcentage = (double)spawn.TempsRestantPatience / spawn.TempsPatience; // Calcule le pourcentage de patience restante

                    if (spawn.RectJauge != null) // Vérifie que la jauge existe
                    {
                        spawn.RectJauge.Height = Math.Max(0, 100 * pourcentage); // Ajuste la hauteur de la jauge proportionnellement
                        SolidColorBrush color; // Déclare la variable pour la couleur
                        if (pourcentage > 0.5) color = new SolidColorBrush(Color.FromRgb(39, 201, 63)); // Vert si > 50%
                        else if (pourcentage > 0.25) color = new SolidColorBrush(Color.FromRgb(255, 187, 5)); // Orange si > 25%
                        else color = new SolidColorBrush(Color.FromRgb(255, 57, 57)); // Rouge si ≤ 25%
                        spawn.RectJauge.Fill = color; // Applique la couleur à la jauge
                    }

                    if (spawn.TempsRestantPatience <= 0) // Si le temps de patience est écoulé
                    {
                        if (spawn.ImageClient != null) spawn.ImageClient.Visibility = Visibility.Hidden; // Cache l'image du client
                        if (spawn.ImageCommande != null) spawn.ImageCommande.Visibility = Visibility.Hidden; // Cache l'image de la commande
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Hidden; // Cache la bordure de la jauge

                        Score -= MALUS_CLIENT_PARTI; // Applique le malus au score
                        MettreAJourAffichageScore(); // Met à jour l'affichage du score
                        MettreAJourBoutonAmelioration(); // Met à jour le bouton d'amélioration
                        spawn.MalusApplique = true; // Marque le malus comme appliqué
                        AfficherMessageMalus(); // Affiche un message de malus
                    }
                }
            }
        }

        private void GenererSpawnsAleatoires() // Méthode pour générer les apparitions aléatoires des clients
        {
            int tempsTotalSecondes = UCTemps.TempsChoisi * 60; // Convertit le temps choisi en secondes
            int nombreClients = Math.Max(5, tempsTotalSecondes / 8); // Calcule le nombre de clients (minimum 5, sinon 1 client toutes les 8 secondes)

            var clientsDisponibles = new (Image, Image, Border, Rectangle)[3] // Tableau des clients disponibles avec leurs éléments UI
            {
                (imgClient2, imgcmd1, borderJauge1, rectJauge1), // Client 2 avec sa commande et sa jauge
                (imgClient3, imgcmd2, borderJauge2, rectJauge2), // Client 3 avec sa commande et sa jauge
                (imgClient4, imgcmd3, borderJauge3, rectJauge3) // Client 4 avec sa commande et sa jauge
            };

            int[] tempsSpawns = new int[50]; // Tableau pour stocker les temps d'apparition
            int tempsCount = 1; // Compteur de temps générés
            tempsSpawns[0] = tempsTotalSecondes - random.Next(3, 6); // Premier client apparaît près de la fin (entre 3 et 5 secondes avant)

            for (int i = 1; i < nombreClients; i++) // Génère les temps pour les autres clients
            {
                int tempsMin = (int)(tempsTotalSecondes * 0.05); // Temps minimum : 5% du temps total
                int tempsMax = tempsTotalSecondes - 3; // Temps maximum : 3 secondes avant la fin
                int temps = random.Next(tempsMin, tempsMax); // Génère un temps aléatoire

                bool tropProche = false; // Flag pour vérifier si le temps est trop proche d'un autre
                for (int j = 0; j < tempsCount; j++) // Parcourt les temps déjà générés
                {
                    if (Math.Abs(tempsSpawns[j] - temps) < 5) // Si l'écart est inférieur à 5 secondes
                    {
                        tropProche = true; // Marque comme trop proche
                        break; // Sort de la boucle
                    }
                }
                while (tropProche) // Tant que le temps est trop proche d'un autre
                {
                    temps = random.Next(tempsMin, tempsMax); // Génère un nouveau temps
                    tropProche = false; // Réinitialise le flag
                    for (int j = 0; j < tempsCount; j++) // Vérifie à nouveau
                    {
                        if (Math.Abs(tempsSpawns[j] - temps) < 5) // Si toujours trop proche
                        {
                            tropProche = true; // Marque comme trop proche
                            break; // Sort de la boucle
                        }
                    }
                }

                tempsSpawns[tempsCount++] = temps; // Ajoute le temps valide au tableau
            }

            Array.Sort(tempsSpawns, 0, tempsCount); // Trie les temps par ordre croissant
            Array.Reverse(tempsSpawns, 0, tempsCount); // Inverse l'ordre (décroissant pour le compte à rebours)

            for (int i = 0; i < tempsCount; i++) // Parcourt tous les temps générés
            {
                int temps = tempsSpawns[i]; // Récupère le temps actuel
                var clientChoisi = clientsDisponibles[random.Next(clientsDisponibles.Length)]; // Choisit un client au hasard
                int tempsPatience = random.Next(20, 41); // Génère un temps de patience aléatoire (entre 20 et 40 secondes)

                listeSpawns[listeSpawnsCount++] = new ClientSpawn // Crée un nouveau ClientSpawn
                {
                    TempsSpawn = temps, // Définit le temps d'apparition
                    ImageClient = clientChoisi.Item1, // Définit l'image du client
                    ImageCommande = clientChoisi.Item2, // Définit l'image de la commande
                    BorderJauge = clientChoisi.Item3, // Définit la bordure de la jauge
                    RectJauge = clientChoisi.Item4, // Définit le rectangle de la jauge
                    TempsPatience = tempsPatience, // Définit le temps de patience total
                    TempsRestantPatience = tempsPatience // Initialise le temps restant égal au temps total
                };
            }
        }

        private void MettreAJourAffichageScore() // Méthode pour mettre à jour l'affichage du score
        {
            label_argent.Text = $"{Score}€"; // Affiche le score avec le symbole €
        }

        private void MettreAJourBoutonAmelioration() // Méthode pour mettre à jour le bouton d'amélioration
        {
            if (bouton_ameliorer == null) // Vérifie que le bouton existe
                return; // Sort de la méthode si le bouton n'existe pas

            int tempsApresAmelioration = tempsCuissonActuel - REDUCTION_TEMPS; // Calcule le temps après amélioration
            var textBlock = new TextBlock // Crée un nouveau TextBlock pour le contenu du bouton
            {
                Text = $"⚡ Améliorer ({COUT_AMELIORATION}€)\nTemps: {tempsApresAmelioration}s", // Texte avec coût et nouveau temps
                TextAlignment = System.Windows.TextAlignment.Center, // Centre le texte
                FontWeight = FontWeights.Bold, // Met le texte en gras
                FontSize = 9, // Définit la taille de police
                Foreground = new SolidColorBrush(Colors.White) // Définit la couleur du texte en blanc
            };

            bouton_ameliorer.Content = textBlock; // Assigne le TextBlock au contenu du bouton
            bouton_ameliorer.IsEnabled = Score >= COUT_AMELIORATION && tempsCuissonActuel > 2; // Active le bouton si le score est suffisant et temps > 2s
            textBlock.Opacity = (Score >= COUT_AMELIORATION && tempsCuissonActuel > 2) ? 1.0 : 0.4; // Opacité 100% si activé, 40% sinon
        }

        private void InitialiserTimer() // Méthode pour initialiser le timer principal
        {
            tempsRestant = TimeSpan.FromMinutes(UCTemps.TempsChoisi); // Initialise le temps restant selon le choix de l'utilisateur
            label_timer.Text = tempsRestant.ToString(@"mm\:ss"); // Affiche le temps au format MM:SS

            timer = new DispatcherTimer(); // Crée un nouveau timer
            timer.Interval = TimeSpan.FromSeconds(1); // Configure l'intervalle à 1 seconde
            timer.Tick += Timer_Tick; // Abonne la méthode Timer_Tick à l'événement Tick
            timer.Start(); // Démarre le timer
        }

        private void Timer_Tick(object sender, EventArgs e) // Méthode appelée chaque seconde par le timer principal
        {
            tempsRestant = tempsRestant.Subtract(TimeSpan.FromSeconds(1)); // Soustrait 1 seconde du temps restant
            label_timer.Text = tempsRestant.ToString(@"mm\:ss"); // Met à jour l'affichage du timer

            int secondesRestantes = (int)tempsRestant.TotalSeconds; // Convertit le temps restant en secondes entières

            for (int i = 0; i < listeSpawnsCount; i++) // Parcourt tous les clients à faire apparaître
            {
                var spawn = listeSpawns[i]; // Récupère le client actuel
                if (!spawn.Spawned && secondesRestantes == spawn.TempsSpawn) // Si le client n'est pas encore apparu et c'est son moment
                {
                    if (spawn.ImageClient != null && spawn.ImageClient.Visibility == Visibility.Hidden) // Vérifie que l'image existe et est cachée
                    {
                        spawn.ImageClient.Visibility = Visibility.Visible; // Affiche l'image du client

                        if (spawn.ImageCommande != null) spawn.ImageCommande.Visibility = Visibility.Visible; // Affiche la commande
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Visible; // Affiche la bordure de la jauge

                        if (spawn.RectJauge != null) // Si la jauge existe
                        {
                            spawn.RectJauge.Height = 100; // Définit la hauteur à 100% (jauge pleine)
                            spawn.RectJauge.Fill = new SolidColorBrush(Color.FromRgb(39, 201, 63)); // Colore la jauge en vert
                        }

                        spawn.Spawned = true; // Marque le client comme apparu
                        spawn.MalusApplique = false; // Réinitialise le flag de malus
                    }
                }
            }

            if (tempsRestant.TotalSeconds <= 0) // Si le temps est écoulé
            {
                timer.Stop(); // Arrête le timer principal
                timerJauges.Stop(); // Arrête le timer des jauges
                label_timer.Text = "00:00"; // Affiche 00:00

                if (musiqueJeu != null) musiqueJeu.Stop(); // Arrête la musique

                var mainWindow = Window.GetWindow(this); // Récupère la fenêtre principale
                if (mainWindow != null && mainWindow.Content is Grid grid) // Si la fenêtre contient une Grid
                {
                    grid.Children.Clear(); // Efface tous les enfants de la Grid
                    grid.Children.Add(new UCScore()); // Ajoute l'écran de score
                }
            }
        }

        private void butQuitter(object sender, RoutedEventArgs e) // Méthode pour quitter le jeu
        {
            if (timer != null) timer.Stop(); // Arrête le timer principal si il existe
            if (timerJauges != null) timerJauges.Stop(); // Arrête le timer des jauges si il existe
            if (musiqueJeu != null) musiqueJeu.Stop(); // Arrête la musique si elle existe
            if (sonVente != null) sonVente.Stop(); // Arrête le son de vente si il existe
            if (sonCuisson != null) sonCuisson.Stop(); // Arrête le son de cuisson si il existe
            Application.Current.Shutdown(); // Ferme complètement l'application
        }

        private async void bouton_preparer_Click(object sender, RoutedEventArgs e) // Méthode appelée quand on clique sur le bouton préparer (asynchrone)
        {
            if (imgCrepe1.Visibility != Visibility.Hidden) // Si une crêpe est déjà en préparation
                return; // Sort de la méthode

            Canvas.SetLeft(imgPoele, 395); // Positionne la poêle horizontalement à 395 pixels
            Canvas.SetTop(imgPoele, 292); // Positionne la poêle verticalement à 292 pixels
            PoeleRotation.Angle = 90; // Fait pivoter la poêle de 90 degrés
            imgCrepe1.Visibility = Visibility.Visible; // Affiche la crêpe en préparation
            tempsRestantPreparation = tempsCuissonActuel; // Initialise le temps de préparation
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s"; // Affiche le temps de préparation
            timerPreparation.Start(); // Démarre le timer de préparation

            if (sonCuisson != null) sonCuisson.PlayLooping(); // Lance le son de cuisson en boucle
        }

        private void Timer_Preparation(object sender, EventArgs e) // Méthode appelée chaque seconde pendant la préparation
        {
            tempsRestantPreparation--; // Décrémente le temps restant
            txtTimer.Text = $"Temps de préparation : {tempsRestantPreparation}s"; // Met à jour l'affichage

            if (tempsRestantPreparation > 0) // Si la préparation n'est pas terminée
                return; // Sort de la méthode

            timerPreparation.Stop(); // Arrête le timer de préparation

            if (imgCrepe2.Visibility == Visibility.Hidden) // Si l'assiette est vide
            {
                imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative)); // Charge l'image de la crêpe cuite
                imgCrepe2.Visibility = Visibility.Visible; // Affiche la crêpe dans l'assiette
                imgCrepe1.Visibility = Visibility.Hidden; // Cache la crêpe en préparation
                Canvas.SetLeft(imgPoele, 312); // Repositionne la poêle horizontalement
                Canvas.SetTop(imgPoele, 276); // Repositionne la poêle verticalement
                PoeleRotation.Angle = 0; // Remet la poêle à 0 degré (position normale)

                if (sonCuisson != null) sonCuisson.Stop(); // Arrête le son de cuisson

                txtTimer.Text = ""; // Efface le texte du timer
            }
            else // Si l'assiette est occupée
            {
                txtTimer.Text = "Assiette occupée ! Vendez d'abord."; // Affiche un message d'erreur
            }
        }

        private async void bouton_ameliorer_Click(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton d'amélioration (méthode asynchrone)
        {
            if (Score < COUT_AMELIORATION || tempsCuissonActuel <= 2) // Vérifie si le joueur a assez d'argent et si le temps peut encore être réduit
                return; // Sort de la méthode si les conditions ne sont pas remplies

            Score -= COUT_AMELIORATION; // Déduit le coût de l'amélioration du score
            tempsCuissonActuel -= REDUCTION_TEMPS; // Réduit le temps de cuisson actuel de 2 secondes
            MettreAJourAffichageScore(); // Met à jour l'affichage du score à l'écran
            MettreAJourBoutonAmelioration(); // Met à jour l'état et le texte du bouton d'amélioration

            if (labelMessageConfirmation != null) // Vérifie que le label de confirmation existe
            {
                labelMessageConfirmation.Content = $"Amélioration achetée ! Temps: {tempsCuissonActuel}s"; // Définit le texte de confirmation avec le nouveau temps
                labelMessageConfirmation.Visibility = Visibility.Visible; // Affiche le message de confirmation
                await Task.Delay(2000); // Attend 2 secondes de manière asynchrone (sans bloquer l'interface)
                labelMessageConfirmation.Visibility = Visibility.Hidden; // Cache le message de confirmation après le délai
            }
        }

        private async void bouton_vendre_Click(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton vendre (méthode asynchrone)
        {
            if (imgCrepe2.Visibility != Visibility.Visible) // Vérifie qu'une crêpe est disponible dans l'assiette
                return; // Sort de la méthode si aucune crêpe n'est prête

            string crepeActuelle = imgCrepe2.Source.ToString(); // Récupère l'URI (chemin) de l'image de la crêpe actuelle
            bool crêpeTrouvée = false; // Flag pour indiquer si un client correspondant a été trouvé

            for (int i = 0; i < clientsEtCommandes.Length; i++) // Parcourt tous les types de clients et leurs commandes
            {
                var (crepeType, client, cmd) = clientsEtCommandes[i]; // Déstructure le tuple en ses trois composants
                if (crepeActuelle.Contains(crepeType) && client.Visibility == Visibility.Visible) // Vérifie si le type de crêpe correspond et si le client est présent
                {
                    ClientSpawn spawn = null; // Variable pour stocker les données du spawn du client
                    for (int j = 0; j < listeSpawnsCount; j++) // Parcourt tous les spawns de clients
                    {
                        if (listeSpawns[j].ImageClient == client && listeSpawns[j].Spawned && !listeSpawns[j].MalusApplique) // Vérifie que c'est le bon client et qu'il n'a pas déjà reçu de malus
                        {
                            spawn = listeSpawns[j]; // Récupère le spawn correspondant
                            break; // Sort de la boucle une fois trouvé
                        }
                    }

                    client.Visibility = Visibility.Hidden; // Cache l'image du client satisfait
                    cmd.Visibility = Visibility.Hidden; // Cache l'image de la commande
                    imgCrepe2.Visibility = Visibility.Hidden; // Cache la crêpe vendue de l'assiette

                    if (spawn != null) // Si les données du spawn ont été trouvées
                    {
                        if (spawn.BorderJauge != null) spawn.BorderJauge.Visibility = Visibility.Hidden; // Cache la bordure de la jauge de patience
                        spawn.MalusApplique = true; // Marque le malus comme appliqué (pour éviter qu'il soit déduit)
                    }

                    crêpeTrouvée = true; // Indique qu'une vente a été effectuée
                    break; // Sort de la boucle principale
                }
            }

            if (!crêpeTrouvée) // Si aucun client ne correspondait à la crêpe préparée
            {
                labelMessageErreurVente.Visibility = Visibility.Visible; // Affiche le message d'erreur de vente
                await Task.Delay(3000); // Attend 3 secondes de manière asynchrone
                labelMessageErreurVente.Visibility = Visibility.Hidden; // Cache le message d'erreur
                return; // Sort de la méthode sans effectuer la vente
            }

            if (sonVente != null) sonVente.Play(); // Joue le son de vente si il existe
            if (sonVente != null) sonVente.Play(); // Joue le son de vente si il existe

            Score += PRIX_CREPE; // Ajoute le prix de la crêpe (5€) au score
            MettreAJourAffichageScore(); // Met à jour l'affichage du score à l'écran
            MettreAJourBoutonAmelioration(); // Met à jour le bouton d'amélioration (disponibilité)
            imgCrepe2.Source = new BitmapImage(new Uri("/Images/crepe_realiste.png", UriKind.Relative)); // Réinitialise l'image à une crêpe nature (sans garniture)

            if (imgCrepe1.Visibility == Visibility.Visible && tempsRestantPreparation <= 0) // Vérifie si une crêpe en attente est prête (cuisson terminée)
            {
                imgCrepe2.Source = imgCrepe1.Source; // Transfère l'image de la crêpe en attente vers l'assiette
                imgCrepe2.Visibility = Visibility.Visible; // Affiche la crêpe dans l'assiette
                imgCrepe1.Visibility = Visibility.Hidden; // Cache la crêpe de la zone de préparation
                Canvas.SetLeft(imgPoele, 312); // Repositionne la poêle à sa position normale horizontalement
                Canvas.SetTop(imgPoele, 276); // Repositionne la poêle à sa position normale verticalement
                PoeleRotation.Angle = 0; // Remet la poêle en position normale (0 degré)

                if (sonCuisson != null) sonCuisson.Stop(); // Arrête le son de cuisson si il est en cours

                txtTimer.Text = ""; // Efface le texte du timer de préparation
            }
        }

        private void GarnirCrepe(string crepePath) // Méthode pour ajouter une garniture à une crêpe
        {
            if (imgCrepe2.Visibility == Visibility.Visible) // Si une crêpe est présente dans l'assiette
            {
                imgCrepe2.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative)); // Change l'image de la crêpe dans l'assiette avec la garniture choisie
            }
            else if (tempsRestantPreparation <= 0 && imgCrepe1.Visibility == Visibility.Visible) // Sinon si une crêpe en préparation est déjà cuite
            {
                imgCrepe1.Source = new BitmapImage(new Uri(crepePath, UriKind.Relative)); // Change l'image de la crêpe en attente avec la garniture choisie
            }
        }

        private void but_nuttela(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton Nutella
        {
            GarnirCrepe("/Images/crepes/crepe_nutella.png"); // Appelle la méthode pour garnir la crêpe avec l'image Nutella
        }

        private void but_caramel(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton Caramel
        {
            GarnirCrepe("/Images/crepes/crepe_caramele.png"); // Appelle la méthode pour garnir la crêpe avec l'image Caramel
        }

        private void but_confutture(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton Confiture
        {
            GarnirCrepe("/Images/crepes/crepe_confitture.png"); // Appelle la méthode pour garnir la crêpe avec l'image Confiture
        }

        private void but_cmiel(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton Chèvre-Miel
        {
            GarnirCrepe("/Images/crepes/crepe_chevremiel.png"); // Appelle la méthode pour garnir la crêpe avec l'image Chèvre-Miel
        }

        private void but_sucre(object sender, RoutedEventArgs e) // Gestionnaire d'événement pour le clic sur le bouton Sucre
        {
            GarnirCrepe("/Images/crepes/crepe_sucre.png"); // Appelle la méthode pour garnir la crêpe avec l'image Sucre
        }

        private async void AfficherMessageMalus() // Méthode pour afficher un message quand un client part sans être servi (méthode asynchrone)
        {
            if (labelMessageErreurVente == null) // Vérifie que le label de message existe
                return; // Sort de la méthode si le label n'existe pas

            labelMessageErreurVente.Content = $"Client parti ! -{MALUS_CLIENT_PARTI}€"; // Définit le contenu du message avec le montant du malus
            labelMessageErreurVente.Visibility = Visibility.Visible; // Affiche le message de malus
            await Task.Delay(2500); // Attend 2,5 secondes de manière asynchrone

            if (labelMessageErreurVente != null) labelMessageErreurVente.Visibility = Visibility.Hidden; // Cache le message si le label existe toujours
        }
    }
}

