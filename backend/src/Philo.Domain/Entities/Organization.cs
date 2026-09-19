namespace Philo.Domain.Entities
{
    public class Organization: BaseEntity   
    {
        public string Name { get; set; }
        public ICollection<OrganizationUser> OrganizationUsers { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
