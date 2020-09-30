using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Services
{
    public interface IEmployeeService
    {
        Task<Employee> DeleteEmployee(int id);
    }

    public class EmployeeService: BaseDataService, IEmployeeService
    {
        public EmployeeService(BenefitsDataContext db) : base(db)
        {
        }

        public async Task<Employee> DeleteEmployee(int id)
        {
            var employee = await Db.Employees.Where(e=>e.Id == id)
                .Include(e=>e.Dependents)
                .Include(e=>e.Paychecks).ThenInclude(p=>p.PaycheckBenefitsCosts)
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                throw new ArgumentException("Employee not found");
            }
            
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (Paycheck employeePaycheck in employee.Paychecks)
                {
                    employeePaycheck.PaycheckBenefitsCosts.Clear();
                }
                Db.SaveChanges();
                employee.Paychecks.Clear();
                Db.Dependents.RemoveRange(employee.Dependents);
                employee.Dependents.Clear();
                Db.SaveChanges();
                Db.Employees.Remove(employee);
                Db.SaveChanges();
                scope.Complete();
            }
            

            return employee;
        }
    }
}
