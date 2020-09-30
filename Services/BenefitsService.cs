using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Services
{
    public interface IBenefitsService
    {
        Task<List<DependentBenefit>> GetActiveDependentBenefits();
        Task<List<EmployeeBenefit>> GetActiveEmployeeBenefits();
    }

    public class BenefitsService: BaseDataService, IBenefitsService
    {
        public BenefitsService(BenefitsDataContext db) : base(db)
        {
            
        }
        public Task<List<DependentBenefit>> GetActiveDependentBenefits()
        {
            return Db.DependentBenefits.Where(b=>b.IsEnabled).Include(b=>b.BenefitDiscounts).ToListAsync();
        }
        public Task<List<EmployeeBenefit>> GetActiveEmployeeBenefits()
        {
            return Db.EmployeeBenefits.Where(b => b.IsEnabled).Include(b => b.BenefitDiscounts).ToListAsync();
        }
    }
}
