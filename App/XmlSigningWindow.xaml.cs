using Microsoft.Win32;
using TestWpfApplication.Models;
using TestWpfApplication.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TestWpfApplication.Models;
using TestWpfApplication.Services;

namespace TestWpfApplication
{
    /// <summary>
    /// Xml signing window.
    /// </summary>
    public partial class XmlSigningWindow : Window
    {
        private XmlSigningService _signingService;
        private string _lastSignedFilePath;

        public XmlSigningWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            LogMessage("Application de signature XML initialisée.");
            LogMessage("Configurez Azure Key Vault et sélectionnez un fichier XML à signer.");
            UpdateStatus("Prêt", Colors.Gray);
            UpdateTimestamp();
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatus("Test de connexion...", Colors.Orange);
                LogMessage("=== Test de connexion Azure Key Vault ===");

                var config = GetKeyVaultConfig();
                if (!config.IsValid())
                {
                    LogMessage("❌ Configuration invalide. Vérifiez tous les champs requis.");
                    UpdateStatus("Configuration invalide", Colors.Red);
                    return;
                }

                _signingService = new XmlSigningService(config);

                LogMessage("Test de connexion en cours...");
                bool connectionSuccess = await _signingService.TestConnectionAsync();

                if (connectionSuccess)
                {
                    LogMessage("✅ Connexion réussie ! Certificat accessible.");
                    UpdateStatus("Connexion OK", Colors.Green);
                    MessageBox.Show("✅ Connexion à Azure Key Vault réussie !\nLe certificat est accessible.",
                                   "Test réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogMessage("❌ Échec de la connexion. Vérifiez la configuration.");
                    UpdateStatus("Échec connexion", Colors.Red);
                    MessageBox.Show("❌ Impossible de se connecter à Azure Key Vault.\nVérifiez votre configuration.",
                                   "Test échoué", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur lors du test : {ex.Message}");
                UpdateStatus("Erreur test", Colors.Red);
                MessageBox.Show($"❌ Erreur lors du test de connexion :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseXmlButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionner le fichier XML à signer"
            };

            if (openDialog.ShowDialog() == true)
            {
                XmlFilePathTextBox.Text = openDialog.FileName;

                // Proposer un nom de fichier de sortie par défaut
                string directory = Path.GetDirectoryName(openDialog.FileName);
                string fileName = Path.GetFileNameWithoutExtension(openDialog.FileName);
                string extension = Path.GetExtension(openDialog.FileName);
                string suggestedOutput = Path.Combine(directory, $"{fileName}_signed{extension}");

                OutputFilePathTextBox.Text = suggestedOutput;

                LogMessage($"📁 Fichier sélectionné : {openDialog.FileName}");
                LogMessage($"📄 Sortie proposée : {suggestedOutput}");
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                Title = "Choisir l'emplacement du fichier signé",
                FileName = Path.GetFileName(OutputFilePathTextBox.Text)
            };

            if (saveDialog.ShowDialog() == true)
            {
                OutputFilePathTextBox.Text = saveDialog.FileName;
                LogMessage($"📄 Destination définie : {saveDialog.FileName}");
            }
        }

        private async void SignXmlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation des entrées
                if (string.IsNullOrWhiteSpace(XmlFilePathTextBox.Text))
                {
                    MessageBox.Show("Veuillez sélectionner un fichier XML à signer.", "Erreur",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(XmlFilePathTextBox.Text))
                {
                    MessageBox.Show("Le fichier XML spécifié n'existe pas.", "Erreur",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Créer le service si nécessaire
                if (_signingService == null)
                {
                    var config = GetKeyVaultConfig();
                    if (!config.IsValid())
                    {
                        MessageBox.Show("Configuration Azure Key Vault invalide. Testez d'abord la connexion.",
                                       "Configuration invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    _signingService = new XmlSigningService(config);
                }

                UpdateStatus("Signature en cours...", Colors.Orange);
                LogMessage("\n=== DÉBUT DE LA SIGNATURE ===");
                LogMessage($"⏰ {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                LogMessage($"📄 Fichier source : {XmlFilePathTextBox.Text}");
                LogMessage($"📄 Fichier destination : {OutputFilePathTextBox.Text}");

                // Effectuer la signature
                var result = await _signingService.SignXmlFileAsync(
                    XmlFilePathTextBox.Text,
                    OutputFilePathTextBox.Text);

                if (result.Success)
                {
                    LogMessage("✅ Signature réussie !");
                    LogMessage($"🔐 Empreinte certificat : {result.CertificateThumbprint}");
                    LogMessage($"💾 Fichier signé : {result.SignedFilePath}");
                    LogMessage($"⏰ Temps de signature : {result.SigningTime:dd/MM/yyyy HH:mm:ss}");

                    _lastSignedFilePath = result.SignedFilePath;
                    DownloadSignedButton.IsEnabled = true;

                    UpdateStatus("Signature réussie", Colors.Green);

                    MessageBox.Show($"✅ Document XML signé avec succès !\n\nFichier signé : {result.SignedFilePath}",
                                   "Signature réussie", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogMessage($"❌ Échec de la signature : {result.Message}");
                    if (result.Exception != null)
                    {
                        LogMessage($"🔍 Détails de l'erreur : {result.Exception.Message}");
                    }

                    UpdateStatus("Échec signature", Colors.Red);

                    MessageBox.Show($"❌ Échec de la signature :\n{result.Message}",
                                   "Erreur de signature", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                LogMessage("=== FIN DE LA SIGNATURE ===\n");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Exception lors de la signature : {ex.Message}");
                UpdateStatus("Erreur signature", Colors.Red);
                MessageBox.Show($"❌ Erreur inattendue :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerifySignatureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileToVerify = _lastSignedFilePath ?? OutputFilePathTextBox.Text;

                if (string.IsNullOrWhiteSpace(fileToVerify) || !File.Exists(fileToVerify))
                {
                    var openDialog = new OpenFileDialog
                    {
                        Filter = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                        Title = "Sélectionner le fichier XML signé à vérifier"
                    };

                    if (openDialog.ShowDialog() != true)
                        return;

                    fileToVerify = openDialog.FileName;
                }

                LogMessage($"\n=== VÉRIFICATION DE SIGNATURE ===");
                LogMessage($"📄 Fichier : {fileToVerify}");

                if (_signingService == null)
                {
                    var config = GetKeyVaultConfig();
                    _signingService = new XmlSigningService(config);
                }

                bool isValid = _signingService.VerifyXmlSignature(fileToVerify);

                if (isValid)
                {
                    LogMessage("✅ Signature valide et vérifiée !");
                    UpdateStatus("Signature valide", Colors.Green);
                    MessageBox.Show("✅ La signature du document XML est valide et vérifiée.",
                                   "Signature valide", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogMessage("❌ Signature invalide ou corrompue !");
                    UpdateStatus("Signature invalide", Colors.Red);
                    MessageBox.Show("❌ La signature du document XML est invalide ou le document a été modifié.",
                                   "Signature invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                LogMessage("=== FIN DE VÉRIFICATION ===\n");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur lors de la vérification : {ex.Message}");
                MessageBox.Show($"❌ Erreur lors de la vérification :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadSignedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_lastSignedFilePath) || !File.Exists(_lastSignedFilePath))
                {
                    MessageBox.Show("Aucun fichier signé disponible au téléchargement.", "Information",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers XML (*.xml)|*.xml|Tous les fichiers (*.*)|*.*",
                    Title = "Télécharger le fichier signé",
                    FileName = Path.GetFileName(_lastSignedFilePath)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.Copy(_lastSignedFilePath, saveDialog.FileName, true);
                    LogMessage($"💾 Fichier téléchargé vers : {saveDialog.FileName}");

                    MessageBox.Show($"✅ Fichier téléchargé avec succès :\n{saveDialog.FileName}",
                                   "Téléchargement réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Erreur lors du téléchargement : {ex.Message}");
                MessageBox.Show($"❌ Erreur lors du téléchargement :\n{ex.Message}",
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private AzureKeyVaultConfig GetKeyVaultConfig()
        {
            return new AzureKeyVaultConfig
            {
                KeyVaultUrl = KeyVaultUrlTextBox.Text.Trim(),
                CertificateName = CertificateNameTextBox.Text.Trim(),
                TenantId = TenantIdTextBox.Text.Trim(),
                ClientId = ClientIdTextBox.Text.Trim(),
                ClientSecret = ClientSecretPasswordBox.Password,
                AuthMethod = (AuthenticationMethod)AuthMethodComboBox.SelectedIndex
            };
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                LogTextBox.AppendText($"[{timestamp}] {message}\n");
                LogTextBox.ScrollToEnd();
                UpdateTimestamp();
            });
        }

        private void UpdateStatus(string message, Color color)
        {
            Dispatcher.Invoke(() =>
            {
                StatusTextBlock.Text = message;
                StatusIndicator.Fill = new SolidColorBrush(color);
            });
        }

        private void UpdateTimestamp()
        {
            TimestampTextBlock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}