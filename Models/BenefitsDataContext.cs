using Microsoft.EntityFrameworkCore;

namespace ServerApp.Models
{
    public class BenefitsDataContext:DbContext
    {
        public BenefitsDataContext(DbContextOptions<BenefitsDataContext> options) : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Paycheck> Paychecks { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Dependent> Dependents { get; set; }
        public DbSet<EmployeeBenefit> EmployeeBenefits { get; set; }
        public DbSet<DependentBenefit> DependentBenefits { get; set; }
        public DbSet<NameStartsWithBenefitDiscount> NameStartsWithBenefitDiscounts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaycheckBenefitCost>()
                .Property(s => s.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}