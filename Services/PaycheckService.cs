using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Services
{
    public interface IPaycheckService
    {
        Task ProcessPaychecks(int employeeId, int year);
        Task DeletePaychecks(int employeeId, int year);
        IQueryable<Paycheck> GetEmployeePaychecks(int employeeId, int year);
    }

    public class PaycheckService : BaseDataService, IPaycheckService
    {
        public PaycheckService(BenefitsDataContext db, IBenefitsService benefitsService) : base(db)
        {
            BenefitsService = benefitsService;
        }

        public IBenefitsService BenefitsService { get; set; }

        #region IPaycheckService implementation

        /// <summary>
        ///     This will create missing and recalculate only paychecks that haven't been sent
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task ProcessPaychecks(int employeeId, int year)
        {
            // Getting and processing only paychecks that haven't been sent
            await RebuildPaychecks(employeeId, year);
            var paychecksForTheYear = await GetPaychecksForTheYear(employeeId, year);
            var paychecksToProcess = paychecksForTheYear.Where(p => p.SentDate == null).OrderBy(p => p.Index).ToList();
            var processPaychecksParams = new ProcessPaychecksParams(
                paychecksToProcess, 
                paychecksForTheYear,
                await BenefitsService.GetActiveEmployeeBenefits(), 
                await BenefitsService.GetActiveDependentBenefits());
            await ProcessPaychecks(processPaychecksParams);
        }

        public async Task DeletePaychecks(int employeeId, int year)
        {
            Db.Paychecks.RemoveRange(Db.Paychecks
                .Where(p => p.Year == year && p.EmployeeId == employeeId && p.SentDate == null)
                .Include(p => p.PaycheckBenefitsCosts));
            await Db.SaveChangesAsync();
        }

        public IQueryable<Paycheck> GetEmployeePaychecks(int employeeId, int year)
        {
            return Db.Paychecks.Where(p => p.Year == year && p.EmployeeId == employeeId);
        }

        #endregion

        #region Paycheck processing

        private async Task ProcessPaychecks(ProcessPaychecksParams processPaychecksParams)
        {
            if (!processPaychecksParams.PaychecksToProcess.Any()) return;

            Parallel.ForEach(processPaychecksParams.PaychecksToProcess, paycheck =>
            {
                CreatePaycheckBenefitCosts(paycheck, processPaychecksParams.EmployeeBenefits,
                    processPaychecksParams.DependentBenefits);
                ProcessPaycheck(paycheck);
            });
            AdjustForRoundingIssuesForBenefits(processPaychecksParams.PaychecksForTheYear,
                processPaychecksParams.PaychecksToProcess);
            AdjustForRoundingIssuesForGrossPay(processPaychecksParams.PaychecksForTheYear,
                processPaychecksParams.PaychecksToProcess);

            await Db.SaveChangesAsync();
        }

        private void AdjustForRoundingIssuesForBenefits(List<Paycheck> paychecksForTheYear,
            List<Paycheck> paychecksToProcess)
        {
            if (!paychecksToProcess.Any()) return;
            // Adjusting for rounding issues arising from benefit discount 
            // Adding all residuals form all benefits for the year and adding into last paycheck
            var paycheckBenefitCostsForTheYear =
                paychecksForTheYear.SelectMany(pc => pc.PaycheckBenefitsCosts).ToList();
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
        }

        private void AdjustForRoundingIssuesForGrossPay(List<Paycheck> paychecksForTheYear,
            List<Paycheck> paychecksToProcess)
        {
            if (!paychecksToProcess.Any()) return;
            // Fixing rounding issues with gross pay
            // Adjust last gross for residual because annual gross might not divide exactly by NumberOfChecksPerYear

            var allResidualsForTheYear = paychecksForTheYear.Sum(c => c.ResidualGrossAmount);
            if (allResidualsForTheYear != 0)
            {
                // Add residuals to last check gross
                var lastPaycheck = paychecksToProcess.Last();
                lastPaycheck.GrossAmount += Math.Round(allResidualsForTheYear, 2);
                ProcessPaycheck(lastPaycheck);
            }
        }

        private async Task<List<Paycheck>> GetPaychecksForTheYear(int employeeId, int year)
        {
            if (year <= 0) throw new ArgumentException("Invalid Year");
            if (employeeId <= 0) throw new ArgumentException("Invalid EmployeeId");
            return await Db.Paychecks.Where(pc => pc.Year == year && pc.EmployeeId == employeeId)
                .Include(pc => pc.PaycheckBenefitsCosts)
                .Include(pc => pc.Employee)
                .Include(pc => pc.Employee.Dependents)
                .ToListAsync();
        }

        private async Task RebuildPaychecks(int employeeId, int year)
        {
            var employee = Db.Employees.Find(employeeId);
            if (employee == null) throw new ArgumentOutOfRangeException($"Unable to find employee by id {employeeId}");
            var yearPaychecks = await GetEmployeePaychecks(employeeId, year).Include(p => p.PaycheckBenefitsCosts)
                .ToListAsync();
            var sentPaychecks = yearPaychecks.Where(p => p.SentDate != null).ToList();
            var lastSentIndex = 0;
            if (sentPaychecks.Any())
            {
                lastSentIndex = sentPaychecks.Max(p => p.Index);
                // nothing we can do - last paycheck sent
                if (lastSentIndex == Paycheck.PaychecksPerYear) return;
            }


            var annualPay = employee.AnnualGrossPay ?? Paycheck.DefaultAnnualGrossPay;
            var paycheckAmount = annualPay / Paycheck.PaychecksPerYear;
            var dateFrom = new DateTime(year, 1, 1);
            var daysPerCheck = 365 / Paycheck.PaychecksPerYear;

            foreach (var paycheck in yearPaychecks)
                if (paycheck.SentDate == null)
                    Db.Paychecks.Remove(paycheck);

            var addedPaychecks = new List<Paycheck>();
            for (var i = 1; i <= Paycheck.PaychecksPerYear; i++)
            {
                var dateTo = dateFrom.AddDays(daysPerCheck - 1);
                if (i > lastSentIndex)
                {
                    var paycheck = new Paycheck
                    {
                        GrossAmount = paycheckAmount,
                        StartDate = dateFrom,
                        EndDate = dateTo,
                        Index = i,
                        Year = year,
                        Employee = employee
                    };
                    paycheck.AdjustResidualGrossAmount();
                    addedPaychecks.Add(paycheck);
                    await Db.Paychecks.AddAsync(paycheck);
                }

                dateFrom = dateFrom.AddDays(daysPerCheck);
            }

            await Db.SaveChangesAsync();
        }

        private void ProcessPaycheck(Paycheck paycheck)
        {
            paycheck.NetAmount = paycheck.GrossAmount - paycheck.BenefitsCost.GetValueOrDefault();
        }

        private void CreatePaycheckBenefitCosts(Paycheck paycheck, List<EmployeeBenefit> employeeBenefits,
            List<DependentBenefit> dependentBenefits)
        {
            // Grab available benefits for any employee
            // Create Invoices for Employee for each benefit
            var result = new ConcurrentBag<PaycheckBenefitCost>();
            Parallel.ForEach(employeeBenefits,
                b => { result.Add(new PaycheckBenefitCost(b, paycheck.Employee, paycheck)); });
            // Create Invoices for each of Employee's dependents
            Parallel.ForEach(dependentBenefits,
                b =>
                {
                    Parallel.ForEach(paycheck.Employee.Dependents,
                        d => { result.Add(new PaycheckBenefitCost(b, d, paycheck)); });
                });
            paycheck.PaycheckBenefitsCosts = new List<PaycheckBenefitCost>(result);
            paycheck.BenefitsCost = paycheck.PaycheckBenefitsCosts.Any()
                ? paycheck.PaycheckBenefitsCosts.Sum(c => c.Amount)
                : 0;

            paycheck.BenefitsCostCalculationDate = DateTime.Now;
        }

        #endregion
        private class ProcessPaychecksParams
        {
            public ProcessPaychecksParams(List<Paycheck> paychecksToProcess, List<Paycheck> paychecksForTheYear,
                List<EmployeeBenefit> employeeBenefits, List<DependentBenefit> dependentBenefits)
            {
                PaychecksToProcess = paychecksToProcess;
                PaychecksForTheYear = paychecksForTheYear;
                EmployeeBenefits = employeeBenefits;
                DependentBenefits = dependentBenefits;
            }

            public List<Paycheck> PaychecksToProcess { get; }
            public List<Paycheck> PaychecksForTheYear { get; }
            public List<EmployeeBenefit> EmployeeBenefits { get; }
            public List<DependentBenefit> DependentBenefits { get; }
        }
    }
    

}