using TestWpfApplication.Models;
using TestWpfApplication.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TestWpfApplication.Models;
using TestWpfApplication.Services;

namespace TestWpfApplication
{
    /// <summary>
    /// Sql search window.
    /// </summary>
    public partial class SqlSearchWindow : Window
    {
        private readonly UserService _userService;

        public SqlSearchWindow()
        {
            InitializeComponent();
            _userService = new UserService();
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            string sqlQuery = SqlQueryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                MessageBox.Show("Veuillez saisir une requête SQL.", "Validation",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isValid = _userService.ValidateSqlQuery(sqlQuery);

            if (isValid)
            {
                StatusTextBlock.Text = "✓ Requête SQL valide";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                ExecuteButton.IsEnabled = true;
            }
            else
            {
                StatusTextBlock.Text = "✗ Requête SQL invalide ou non autorisée";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                ExecuteButton.IsEnabled = false;

                MessageBox.Show("Requête SQL invalide ou non autorisée.\n\n" +
                               "Seules les requêtes SELECT sont autorisées.\n" +
                               "Les requêtes de modification (INSERT, UPDATE, DELETE, etc.) sont interdites.",
                               "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string sqlQuery = SqlQueryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                MessageBox.Show("Veuillez saisir une requête SQL.", "Erreur",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_userService.ValidateSqlQuery(sqlQuery))
            {
                MessageBox.Show("Requête SQL invalide ou non autorisée.", "Erreur",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                StatusTextBlock.Text = "Exécution de la requête...";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Blue;

                if (UserViewRadio.IsChecked == true)
                {
                    ExecuteUserQuery(sqlQuery);
                }
                else
                {
                    ExecuteRawQuery(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution de la requête :\n{ex.Message}",
                               "Erreur SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Erreur d'exécution";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ExecuteUserQuery(string sqlQuery)
        {
            try
            {
                List<User> users = _userService.ExecuteCustomSqlQuery(sqlQuery);
                UsersResultDataGrid.ItemsSource = users;

                ResultCountTextBlock.Text = $"{users.Count} utilisateur(s) trouvé(s)";
                StatusTextBlock.Text = "Requête exécutée avec succès";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                // Si la requête ne retourne pas des utilisateurs valides, basculer vers la vue brute
                MessageBox.Show("La requête ne retourne pas des données d'utilisateurs valides.\n" +
                               "Basculement vers la vue brute.", "Information",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                RawViewRadio.IsChecked = true;
                ExecuteRawQuery(sqlQuery);
            }
        }

        private void ExecuteRawQuery(string sqlQuery)
        {
            RawResultDataGrid.ItemsSource = null;
            DataTable results = _userService.ExecuteRawSqlQuery(sqlQuery);
            RawResultDataGrid.ItemsSource = results.DefaultView;

            ResultCountTextBlock.Text = $"{results.Rows.Count} ligne(s) trouvée(s)";
            StatusTextBlock.Text = "Requête exécutée avec succès";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void ClearSqlButton_Click(object sender, RoutedEventArgs e)
        {
            SqlQueryTextBox.Clear();
            UsersResultDataGrid.ItemsSource = null;
            RawResultDataGrid.ItemsSource = null;
            ResultCountTextBlock.Text = "0 résultat(s)";
            StatusTextBlock.Text = "Prêt";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Black;
            ExecuteButton.IsEnabled = true;
        }

        private void ViewMode_Changed(object sender, RoutedEventArgs e)
        {
            if (UserViewRadio == null || RawViewRadio == null) return;

            if (UserViewRadio.IsChecked == true)
            {
                UsersResultDataGrid.Visibility = Visibility.Visible;
                RawResultDataGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                UsersResultDataGrid.Visibility = Visibility.Collapsed;
                RawResultDataGrid.Visibility = Visibility.Visible;
            }
        }
    }
}