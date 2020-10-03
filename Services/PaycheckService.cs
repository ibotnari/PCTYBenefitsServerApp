using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ServerApp.Models;

namespace ServerApp.Services
{
    public interface IPaycheckService
    {
        Task ProcessPaychecks(int employeeId, int year);
        Task DeletePaychecks(int employeeId, int year);
    }

    public class PaycheckService: BaseDataService, IPaycheckService
    {

        public IBenefitsService BenefitsService { get; set; }
        public PaycheckService(BenefitsDataContext db, IBenefitsService benefitsService) :base(db)
        {
            BenefitsService = benefitsService;
        }

        public async Task ProcessPaychecks(int employeeId, int year)
        {
            if (year <= 0) throw new ArgumentException("Invalid Year");
            if (employeeId <= 0) throw new ArgumentException("Invalid EmployeeId");
            // Getting and processing only paychecks that haven't been sent
            await RebuildPaychecks(employeeId, year);
            var paychecksForTheYear = await GetPaychecksForTheYear(employeeId, year);
            var paychecksToProcess = paychecksForTheYear.Where(p => p.SentDate == null).OrderBy(p => p.Index).ToList();
            if (!paychecksToProcess.Any()) return;
            // paychecksToProcess.SelectMany(pc => pc.PaycheckBenefitsCosts).ToList()
            var employeeBenefits = await BenefitsService.GetActiveEmployeeBenefits();
            var dependentBenefits = await BenefitsService.GetActiveDependentBenefits();
            Parallel.ForEach(paychecksToProcess, paycheck =>
            {
                CreatePaycheckBenefitCosts(paycheck, employeeBenefits, dependentBenefits);
                ProcessPaycheck(paycheck);
            });
            // Adjusting for rounding issues arising from benefit discount 
            // Adding all residuals form all benefits for the year and adding into last paycheck
            var paycheckBenefitCostsForTheYear = paychecksForTheYear.SelectMany(pc=>pc.PaycheckBenefitsCosts).ToList();
            if (paycheckBenefitCostsForTheYear.Any())
            {
                var allResidualsForTheYear = paycheckBenefitCostsForTheYear.Sum(c => c.ResidualAmount);
                if (allResidualsForTheYear != 0)
                {
                    // Add residuals to last check
                    var lastPaycheck = paychecksToProcess.Last();
                    lastPaycheck.BenefitsCost += Math.Round(allResidualsForTheYear, 2);
                    ProcessPaycheck(lastPaycheck);
                }
            }

            await Db.SaveChangesAsync();
        }

        private async Task<List<Paycheck>> GetPaychecksForTheYear(int employeeId, int year)
        {
            return await Db.Paychecks.Where(pc => pc.Year == year && pc.EmployeeId == employeeId)
                .Include(pc=>pc.PaycheckBenefitsCosts)
                .Include(pc=>pc.Employee)
                .Include(pc=>pc.Employee.Dependents)
                .ToListAsync();
        }

        private async Task<List<Paycheck>> GetOrCreatePaychecksForTheYear(int employeeId, int year)
        {
            var paychecksForTheYear = await GetPaychecksForTheYear(employeeId, year);
            if (!paychecksForTheYear.Any())
            {
                var employee = Db.Employees.Find(employeeId);
                if (employee == null) throw new ArgumentException($"Unable to find employee by id {employeeId}");
                var paychecks = SeedData.BuildPaychecks(employee, year);
                paychecks.ForEach(p => { p.EmployeeId = employeeId;});
                await Db.Paychecks.AddRangeAsync(paychecks);
                await Db.SaveChangesAsync();
            }
            return await GetPaychecksForTheYear(employeeId, year);
        }

        public async Task DeletePaychecks( int employeeId, int year)
        {
            Db.Paychecks.RemoveRange(Db.Paychecks.Where(p => p.Year == year && p.EmployeeId == employeeId && p.SentDate == null).Include(p=>p.PaycheckBenefitsCosts));
            await Db.SaveChangesAsync();
        }

        private async Task RebuildPaychecks(int employeeId, int year)
        {
            var employee = Db.Employees.Find(employeeId);
            if (employee == null) throw new ArgumentOutOfRangeException($"Unable to find employee by id {employeeId}");
            var yearPaychecks = await Db.Paychecks.Where(p=>p.Year == year && p.EmployeeId == employeeId).Include(p=>p.PaycheckBenefitsCosts).ToListAsync();
            var sentPaychecks = yearPaychecks.Where(p => p.SentDate != null).ToList();
            int lastSentIndex = 0;
            if (sentPaychecks.Any())
            {
                lastSentIndex = sentPaychecks.Max(p => p.Index);
                // nothing we can do - last paycheck sent
                if (lastSentIndex == Paycheck.PaychecksPerYear) return;
            }
            
            
            decimal annualPay = employee.AnnualGrossPay ?? Paycheck.DefaultAnnualGrossPay;
            decimal paycheckAmount = annualPay / Paycheck.PaychecksPerYear;
            var dateFrom = new DateTime(year, 1, 1);
            int daysPerCheck = (int)365 / Paycheck.PaychecksPerYear;
            
            foreach (var paycheck in yearPaychecks)
            {
                if (paycheck.SentDate == null)
                {
                    Db.Paychecks.Remove(paycheck);
                }
            }
            for (int i = 1; i <= Paycheck.PaychecksPerYear; i++)
            {
                var dateTo = dateFrom.AddDays(daysPerCheck - 1);
                if (i > lastSentIndex)
                {
                    await Db.Paychecks.AddAsync(new Paycheck()
                    {
                        GrossAmount = paycheckAmount,
                        StartDate = dateFrom,
                        EndDate = dateTo,
                        Index = i,
                        Year = year,
                        Employee = employee
                    });
                }

                dateFrom = dateFrom.AddDays(daysPerCheck);
            }
            await Db.SaveChangesAsync();
        }

        private void ProcessPaycheck(Paycheck paycheck)
        {
            paycheck.NetAmount = paycheck.GrossAmount - paycheck.BenefitsCost.GetValueOrDefault();
        }

        private void CreatePaycheckBenefitCosts(Paycheck paycheck, List<EmployeeBenefit> employeeBenefits, List<DependentBenefit> dependentBenefits)
        {
            // Grab available benefits for any employee
            // Create Invoices for Employee for each benefit
            var result = new ConcurrentBag<PaycheckBenefitCost>();
            Parallel.ForEach(employeeBenefits, b =>
            {
                result.Add( new PaycheckBenefitCost(b, paycheck.Employee, paycheck));
            });
            // Create Invoices for each of Employee's dependents
            Parallel.ForEach(dependentBenefits, b =>
            {
                Parallel.ForEach(paycheck.Employee.Dependents, (d) =>
                {
                    result.Add(new PaycheckBenefitCost(b, d, paycheck));
                });
            });
            paycheck.PaycheckBenefitsCosts = new List<PaycheckBenefitCost>(result);
            paycheck.BenefitsCost = paycheck.PaycheckBenefitsCosts.Any()
                ? paycheck.PaycheckBenefitsCosts.Sum(c => c.Amount)
                : 0;
            
            paycheck.BenefitsCostCalculationDate = DateTime.Now;
            Console.WriteLine($"{paycheck.Index} {paycheck.Year}");
        }

    }
}
