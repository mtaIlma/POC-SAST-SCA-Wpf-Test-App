using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using TestWpfApplication.Mappings;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace TestWpfApplication.Data
{
    /// <summary>
    /// Config helper.
    /// </summary>
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    InitializeSessionFactory();
                }
                return _sessionFactory;
            }
        }

        private static void InitializeSessionFactory()
        {
            var configuration = Fluently.Configure()
                .Database(PostgreSQLConfiguration.Standard
                    .ConnectionString(c => c
                        .Host("localhost")
                        .Port(5433)
                        .Database("userdb")
                        .Username("postgres")
                        .Password("secret123"))) // Vuln: hardcoded credentials
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMapping>())
                .ExposeConfiguration(cfg =>
                {
                    cfg.SetProperty("show_sql", "true");
                    cfg.SetProperty("format_sql", "true");
                })
                .BuildConfiguration();

            _sessionFactory = configuration.BuildSessionFactory();
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public static void CreateSchema()
        {
            var configuration = Fluently.Configure()
                .Database(PostgreSQLConfiguration.Standard
                    .ConnectionString(c => c
                        .Host("localhost")
                        .Port(5433)
                        .Database("userdb")
                        .Username("postgres")
                        .Password("secret123"))) // Vuln: hardcoded credentials
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMapping>())
                .BuildConfiguration();

            new SchemaExport(configuration).Create(false, true);
        }
    }
}