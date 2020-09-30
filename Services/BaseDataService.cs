using ServerApp.Models;

namespace ServerApp.Services
{
    public class BaseDataService    
    {
        public BenefitsDataContext Db { get; }
        public BaseDataService(BenefitsDataContext db)
        {
            Db = db;
        }
    }
}
