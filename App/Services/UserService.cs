using TestWpfApplication.Data;
using TestWpfApplication.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TestWpfApplication.Data;
using TestWpfApplication.Models;

namespace TestWpfApplication.Services
{
    /// <summary>
    /// user service.
    /// </summary>
    public class UserService
    {
        public List<User> SearchUsers(string searchTerm, bool activeOnly = false)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var criteria = session.CreateCriteria<User>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var disjunction = Restrictions.Disjunction();
                    disjunction.Add(Restrictions.Like("FirstName", $"%{searchTerm}%", MatchMode.Anywhere));
                    disjunction.Add(Restrictions.Like("LastName", $"%{searchTerm}%", MatchMode.Anywhere));
                    disjunction.Add(Restrictions.Like("Email", $"%{searchTerm}%", MatchMode.Anywhere));

                    criteria.Add(disjunction);
                }

                if (activeOnly)
                {
                    criteria.Add(Restrictions.Eq("IsActive", true));
                }

                criteria.AddOrder(Order.Asc("FirstName"));
                criteria.AddOrder(Order.Asc("LastName"));

                return criteria.List<User>().ToList();
            }
        }

        public User GetUserById(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.Get<User>(id);
            }
        }

        public void SaveUser(User user)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    session.SaveOrUpdate(user);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void DeleteUser(int id)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var user = session.Get<User>(id);
                    if (user != null)
                    {
                        session.Delete(user);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<User> GetAllUsers()
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.CreateCriteria<User>()
                    .AddOrder(Order.Asc("FirstName"))
                    .List<User>()
                    .ToList();
            }
        }

        public List<User> ExecuteCustomSqlQuery(string sqlQuery)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var query = session.CreateSQLQuery(sqlQuery)
                    .AddEntity(typeof(User));

                return query.List<User>().ToList();
            }
        }

        public DataTable ExecuteRawSqlQuery(string sqlQuery)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var connection = session.Connection;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.CommandType = CommandType.Text;

                    using (var adapter = new Npgsql.NpgsqlDataAdapter((Npgsql.NpgsqlCommand)command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public bool ValidateSqlQuery(string sqlQuery)
        {
            try
            {                
                var upperQuery = sqlQuery.ToUpper().Trim();
                // Doit commencer par SELECT
                if (!upperQuery.StartsWith("SELECT"))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}