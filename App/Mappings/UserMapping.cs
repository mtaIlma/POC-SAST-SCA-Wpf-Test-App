using FluentNHibernate.Mapping;
using TestWpfApplication.Models;

namespace TestWpfApplication.Mappings
{
    /// <summary>
    /// user entity mapper.
    /// </summary>
    public class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Table("users");

            Id(x => x.Id, "id")
                .GeneratedBy.Identity();

            Map(x => x.FirstName, "first_name")
                .Not.Nullable()
                .Length(50);

            Map(x => x.LastName, "last_name")
                .Not.Nullable()
                .Length(50);

            Map(x => x.Email, "email")
                .Not.Nullable()
                .Length(100)
                .Unique();

            Map(x => x.CreatedDate, "created_date")
                .Not.Nullable();

            Map(x => x.IsActive, "is_active")
                .Not.Nullable();
        }
    }
}