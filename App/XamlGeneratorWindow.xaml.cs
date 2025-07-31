using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace TestWpfApplication
{
    /// <summary>
    /// Xaml generator window.
    /// </summary>
    public partial class XamlGeneratorWindow : Window
    {
        private FrameworkElement _generatedContent;
        private List<Control> _inputControls;

        public XamlGeneratorWindow()
        {
            InitializeComponent();
            _inputControls = new List<Control>();
        }

        private void ValidateXamlButton_Click(object sender, RoutedEventArgs e)
        {
            string xamlContent = XamlTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(xamlContent))
            {
                ShowMessage("Veuillez saisir du XAML.", "Validation", MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Tentative de parsing du XAML
                var xamlDocument = new XmlDocument();
                xamlDocument.LoadXml(xamlContent);

                ShowMessage("✅ XAML valide !", "Validation réussie", MessageBoxImage.Information);
                UpdateStatus("XAML validé avec succès", true);
            }
            catch (XmlException ex)
            {
                ShowMessage($"❌ Erreur XML :\n{ex.Message}", "Erreur de validation", MessageBoxImage.Error);
                UpdateStatus("Erreur de validation XML", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur de validation :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
                UpdateStatus("Erreur de validation", false);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string xamlContent = XamlTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(xamlContent))
            {
                ShowMessage("Veuillez saisir du XAML.", "Génération", MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Parser et créer l'interface
                var element = (FrameworkElement)XamlReader.Parse(xamlContent);

                _generatedContent = element;
                DynamicContentPresenter.Content = element;

                // Collecter tous les contrôles de saisie
                CollectInputControls(element);

                UpdateStatus($"Interface générée avec succès ! {_inputControls.Count} contrôle(s) trouvé(s)", true);
                ShowMessage($"✅ Interface générée avec succès !\n{_inputControls.Count} contrôle(s) de saisie détecté(s).",
                           "Génération réussie", MessageBoxImage.Information);
            }
            catch (XamlParseException ex)
            {
                ShowMessage($"❌ Erreur de parsing XAML :\n{ex.Message}\n\nLigne {ex.LineNumber}, Position {ex.LinePosition}",
                           "Erreur XAML", MessageBoxImage.Error);
                UpdateStatus("Erreur de parsing XAML", false);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors de la génération :\n{ex.Message}",
                           "Erreur", MessageBoxImage.Error);
                UpdateStatus("Erreur de génération", false);
            }
        }

        private void CollectInputControls(DependencyObject parent)
        {
            _inputControls.Clear();

            if (parent == null) return;

            // Ajouter le contrôle actuel s'il est un contrôle de saisie
            if (parent is TextBox || parent is PasswordBox || parent is ComboBox ||
                parent is CheckBox || parent is RadioButton || parent is DatePicker ||
                parent is Slider || parent is ListBox)
            {
                _inputControls.Add((Control)parent);
            }

            // Parcourir récursivement les enfants
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                CollectInputControls(child);
            }
        }

        private void ExtractDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (_generatedContent == null)
            {
                ShowMessage("Veuillez d'abord générer une interface.", "Extraction", MessageBoxImage.Warning);
                return;
            }

            try
            {
                var extractedData = new Dictionary<string, object>();

                foreach (var control in _inputControls)
                {
                    string controlName = control.Name ?? $"{control.GetType().Name}_{control.GetHashCode()}";
                    object value = ExtractValueFromControl(control);
                    extractedData[controlName] = value;
                }

                // Affichage des résultats
                var result = "=== DONNÉES EXTRAITES ===\n";
                result += $"Date/Heure: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n";
                result += $"Contrôles trouvés: {_inputControls.Count}\n\n";

                foreach (var item in extractedData)
                {
                    result += $"🔹 {item.Key}: {item.Value ?? "(vide)"}\n";
                }

                result += "\n=== FORMAT JSON ===\n";
                var jsonData = JsonSerializer.Serialize(extractedData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                result += jsonData;

                ResultTextBox.Text = result;
                UpdateStatus($"Données extraites de {extractedData.Count} contrôle(s)", true);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors de l'extraction :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
                UpdateStatus("Erreur d'extraction", false);
            }
        }

        private object ExtractValueFromControl(Control control)
        {
            return control switch
            {
                TextBox textBox => textBox.Text,
                PasswordBox passwordBox => passwordBox.Password,
                ComboBox comboBox => comboBox.SelectedItem?.ToString() ?? comboBox.Text,
                CheckBox checkBox => checkBox.IsChecked,
                RadioButton radioButton => radioButton.IsChecked,
                DatePicker datePicker => datePicker.SelectedDate,
                Slider slider => slider.Value,
                ListBox listBox => listBox.SelectedItem?.ToString(),
                _ => control.ToString()
            };
        }

        private void FillSampleDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (_generatedContent == null)
            {
                ShowMessage("Veuillez d'abord générer une interface.", "Remplissage", MessageBoxImage.Warning);
                return;
            }

            try
            {
                var random = new Random();
                var sampleNames = new[] { "Jean Dupont", "Marie Martin", "Pierre Durand", "Sophie Leclerc" };
                var sampleEmails = new[] { "jean@email.com", "marie@test.fr", "pierre@demo.com", "sophie@exemple.org" };

                foreach (var control in _inputControls)
                {
                    switch (control)
                    {
                        case TextBox textBox:
                            if (textBox.Name?.ToLower().Contains("nom") == true ||
                                textBox.Name?.ToLower().Contains("name") == true)
                            {
                                textBox.Text = sampleNames[random.Next(sampleNames.Length)];
                            }
                            else if (textBox.Name?.ToLower().Contains("email") == true ||
                                     textBox.Name?.ToLower().Contains("mail") == true)
                            {
                                textBox.Text = sampleEmails[random.Next(sampleEmails.Length)];
                            }
                            else
                            {
                                textBox.Text = $"Exemple {random.Next(1, 100)}";
                            }
                            break;

                        case PasswordBox passwordBox:
                            passwordBox.Password = "Demo123!";
                            break;

                        case CheckBox checkBox:
                            checkBox.IsChecked = random.Next(2) == 1;
                            break;

                        case RadioButton radioButton:
                            radioButton.IsChecked = random.Next(3) == 1;
                            break;

                        case DatePicker datePicker:
                            datePicker.SelectedDate = DateTime.Today.AddDays(-random.Next(365));
                            break;

                        case Slider slider:
                            slider.Value = random.Next((int)slider.Minimum, (int)slider.Maximum + 1);
                            break;

                        case ComboBox comboBox:
                            if (comboBox.Items.Count > 0)
                            {
                                comboBox.SelectedIndex = random.Next(comboBox.Items.Count);
                            }
                            break;
                    }
                }

                UpdateStatus("Données d'exemple remplies", true);
                ShowMessage($"✅ Données d'exemple ajoutées dans {_inputControls.Count} contrôle(s) !",
                           "Remplissage réussi", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors du remplissage :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
                UpdateStatus("Erreur de remplissage", false);
            }
        }

        private void ResetValuesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_generatedContent == null)
            {
                ShowMessage("Veuillez d'abord générer une interface.", "Reset", MessageBoxImage.Warning);
                return;
            }

            try
            {
                foreach (var control in _inputControls)
                {
                    switch (control)
                    {
                        case TextBox textBox:
                            textBox.Clear();
                            break;
                        case PasswordBox passwordBox:
                            passwordBox.Clear();
                            break;
                        case CheckBox checkBox:
                            checkBox.IsChecked = false;
                            break;
                        case RadioButton radioButton:
                            radioButton.IsChecked = false;
                            break;
                        case ComboBox comboBox:
                            comboBox.SelectedIndex = -1;
                            break;
                        case DatePicker datePicker:
                            datePicker.SelectedDate = null;
                            break;
                        case Slider slider:
                            slider.Value = slider.Minimum;
                            break;
                        case ListBox listBox:
                            listBox.SelectedIndex = -1;
                            break;
                    }
                }

                ResultTextBox.Clear();
                UpdateStatus("Valeurs réinitialisées", true);
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors de la réinitialisation :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
            }
        }

        private void ClearPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            DynamicContentPresenter.Content = null;
            _generatedContent = null;
            _inputControls.Clear();
            ResultTextBox.Clear();
            UpdateStatus("Aperçu effacé", true);
        }

        #region Templates XAML

        private void LoadSimpleForm_Click(object sender, RoutedEventArgs e)
        {
            XamlTextBox.Text = @"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <TextBlock Text=""Formulaire Simple"" FontSize=""18"" FontWeight=""Bold"" Margin=""0,0,0,15""/>
    
    <TextBlock Text=""Nom :"" Margin=""0,0,0,5""/>
    <TextBox Name=""NomTextBox"" Height=""25"" Margin=""0,0,0,10""/>
    
    <TextBlock Text=""Email :"" Margin=""0,0,0,5""/>
    <TextBox Name=""EmailTextBox"" Height=""25"" Margin=""0,0,0,10""/>
    
    <CheckBox Name=""AccepterCheckBox"" Content=""J'accepte les conditions"" Margin=""0,0,0,10""/>
    
    <Button Content=""Valider"" Width=""100"" Height=""30"" HorizontalAlignment=""Left""/>
</StackPanel>";
        }

        private void LoadRegistrationForm_Click(object sender, RoutedEventArgs e)
        {
            XamlTextBox.Text = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
      Margin=""10"">

    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
    </Grid.RowDefinitions>

    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""120""/>
        <ColumnDefinition Width=""*""/>
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Row=""0"" Grid.ColumnSpan=""2"" Text=""Inscription Utilisateur"" 
               FontSize=""20"" FontWeight=""Bold"" HorizontalAlignment=""Center"" Margin=""0,0,0,20""/>

    <TextBlock Grid.Row=""1"" Grid.Column=""0"" Text=""Prénom :"" VerticalAlignment=""Center""/>
    <TextBox Grid.Row=""1"" Grid.Column=""1"" Name=""PrenomTextBox"" Height=""25"" Margin=""5""/>

    <TextBlock Grid.Row=""2"" Grid.Column=""0"" Text=""Nom :"" VerticalAlignment=""Center""/>
    <TextBox Grid.Row=""2"" Grid.Column=""1"" Name=""NomTextBox"" Height=""25"" Margin=""5""/>

    <TextBlock Grid.Row=""3"" Grid.Column=""0"" Text=""Email :"" VerticalAlignment=""Center""/>
    <TextBox Grid.Row=""3"" Grid.Column=""1"" Name=""EmailTextBox"" Height=""25"" Margin=""5""/>

    <TextBlock Grid.Row=""4"" Grid.Column=""0"" Text=""Mot de passe :"" VerticalAlignment=""Center""/>
    <PasswordBox Grid.Row=""4"" Grid.Column=""1"" Name=""PasswordBox"" Height=""25"" Margin=""5""/>

    <TextBlock Grid.Row=""5"" Grid.Column=""0"" Text=""Date naissance :"" VerticalAlignment=""Center""/>
    <DatePicker Grid.Row=""5"" Grid.Column=""1"" Name=""DateNaissancePicker"" Height=""25"" Margin=""5""/>

    <TextBlock Grid.Row=""6"" Grid.Column=""0"" Text=""Pays :"" VerticalAlignment=""Center""/>
    <ComboBox Grid.Row=""6"" Grid.Column=""1"" Name=""PaysComboBox"" Height=""25"" Margin=""5"">
        <ComboBoxItem Content=""France""/>
        <ComboBoxItem Content=""Belgique""/>
        <ComboBoxItem Content=""Suisse""/>
        <ComboBoxItem Content=""Canada""/>
    </ComboBox>

    <StackPanel Grid.Row=""7"" Grid.ColumnSpan=""2"" Orientation=""Horizontal"" 
                HorizontalAlignment=""Center"" Margin=""0,20,0,0"">
        <Button Content=""S'inscrire"" Width=""100"" Height=""30"" Margin=""5""/>
        <Button Content=""Annuler"" Width=""100"" Height=""30"" Margin=""5""/>
    </StackPanel>
</Grid>
";
        }

        private void LoadSurveyForm_Click(object sender, RoutedEventArgs e)
        {
            XamlTextBox.Text = @"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" Margin=""10"">
    <TextBlock Text=""Questionnaire de Satisfaction"" FontSize=""18"" FontWeight=""Bold"" 
               HorizontalAlignment=""Center"" Margin=""0,0,0,20""/>

    <TextBlock Text=""1. Comment évaluez-vous notre service ?"" FontWeight=""Bold"" Margin=""0,0,0,5""/>
    <StackPanel Orientation=""Horizontal"" Margin=""0,0,0,15"">
        <RadioButton Name=""Excellent"" Content=""Excellent"" Margin=""0,0,15,0""/>
        <RadioButton Name=""Bon"" Content=""Bon"" Margin=""0,0,15,0""/>
        <RadioButton Name=""Moyen"" Content=""Moyen"" Margin=""0,0,15,0""/>
        <RadioButton Name=""Mauvais"" Content=""Mauvais""/>
    </StackPanel>

    <TextBlock Text=""2. Recommanderiez-vous nos services ?"" FontWeight=""Bold"" Margin=""0,0,0,5""/>
    <CheckBox Name=""RecommanderCheckBox"" Content=""Oui, je recommande"" Margin=""0,0,0,15""/>

    <TextBlock Text=""3. Niveau de satisfaction (1-10) :"" FontWeight=""Bold"" Margin=""0,0,0,5""/>
    <Slider Name=""SatisfactionSlider"" Minimum=""1"" Maximum=""10"" Value=""5"" 
            TickPlacement=""BottomRight"" TickFrequency=""1"" Margin=""0,0,0,15""/>

    <TextBlock Text=""4. Commentaires supplémentaires :"" FontWeight=""Bold"" Margin=""0,0,0,5""/>
    <TextBox Name=""CommentairesTextBox"" Height=""80"" AcceptsReturn=""True"" 
             TextWrapping=""Wrap"" VerticalScrollBarVisibility=""Auto"" Margin=""0,0,0,15""/>

    <Button Content=""Envoyer le questionnaire"" Width=""180"" Height=""30"" 
            HorizontalAlignment=""Center""/>
</StackPanel>";
        }

        private void ResetXaml_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Êtes-vous sûr de vouloir réinitialiser le XAML ?",
                                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                XamlTextBox.Text = @"<StackPanel Margin=""10"">
    <TextBlock Text=""Votre interface ici..."" FontSize=""16"" FontWeight=""Bold""/>
</StackPanel>";
                ClearPreviewButton_Click(sender, e);
            }
        }

        private void SaveXaml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Fichiers XAML (*.xaml)|*.xaml|Tous les fichiers (*.*)|*.*",
                    DefaultExt = "xaml",
                    FileName = $"interface_dynamique_{DateTime.Now:yyyyMMdd_HHmmss}.xaml"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, XamlTextBox.Text);
                    ShowMessage($"✅ XAML sauvegardé :\n{saveDialog.FileName}", "Sauvegarde réussie", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors de la sauvegarde :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
            }
        }

        private void LoadXaml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Fichiers XAML (*.xaml)|*.xaml|Tous les fichiers (*.*)|*.*",
                    DefaultExt = "xaml"
                };

                if (openDialog.ShowDialog() == true)
                {
                    XamlTextBox.Text = File.ReadAllText(openDialog.FileName);
                    ShowMessage($"✅ XAML chargé :\n{openDialog.FileName}", "Chargement réussi", MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Erreur lors du chargement :\n{ex.Message}", "Erreur", MessageBoxImage.Error);
            }
        }

        #endregion

        #region Méthodes utilitaires

        private void ShowMessage(string message, string title, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        private void UpdateStatus(string message, bool isSuccess)
        {
            Title = $"Générateur XAML Dynamique - {message}";
        }

        #endregion
    }
}