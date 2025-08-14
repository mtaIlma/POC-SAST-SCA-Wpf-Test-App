using TestWpfApplication.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace TestWpfApplication
{
    /// <summary>
    /// Entry point.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialiser la session factory au démarrage
                var sessionFactory = NHibernateHelper.SessionFactory;

                // Optionnel : créer le schéma de base si nécessaire
               //  NHibernateHelper.CreateSchema();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation de la base de données : {ex.Message}", // Vuln: error message exposure
                               "Erreur critique", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }

}
