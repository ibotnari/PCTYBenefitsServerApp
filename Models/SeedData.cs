using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ServerApp.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var db = new BenefitsDataContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<BenefitsDataContext>>()))
            {
                Initialize(db);
            }
        }

        public static void Initialize(BenefitsDataContext db)
        {
            if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                db.Database.Migrate();
            }
            
            if (db.Employees.Any())
            {
                return;
            }

            var employeeBenefit = new EmployeeBenefit()
            {
                AnnualCost = 1000,IsEnabled = true, Description = "Medical benefit for household",
            };
            employeeBenefit.BenefitDiscounts = new List<BenefitDiscount>(){new NameStartsWithBenefitDiscount()
            {
                NameStartsWith = "A",
                Percent = 0.01M,
            }};
            db.EmployeeBenefits.AddRange(employeeBenefit);

            var dependentBenefit = new DependentBenefit()
            {
                AnnualCost = 500,IsEnabled = true,Description = "Medical benefit for dependents"
            };
            dependentBenefit.BenefitDiscounts = new List<BenefitDiscount>()
            {
                new NameStartsWithBenefitDiscount()
                {
                    NameStartsWith = "A",
                    Percent = 0.1M,
                }
            };
            db.DependentBenefits.AddRange(dependentBenefit);

            var employee = new Employee()
            {
                FirstName = "John",
                LastName = "Doe",
                StartDate = new DateTime(2020, 1, 1),
                    
                Dependents = new List<Dependent>()
                {
                    new Dependent()
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        DependentRelationshipToEmployee = DependentRelationshipToEmployee.Spouse,
                    },
                    new Dependent()
                    {
                        FirstName = "Kevin",
                        LastName = "Doe",
                        DependentRelationshipToEmployee = DependentRelationshipToEmployee.Child,
                    },
                    new Dependent()
                    {
                        FirstName = "Alex",
                        LastName = "Doe",
                        DependentRelationshipToEmployee = DependentRelationshipToEmployee.Child,
                    }
                }
            };

            employee.Paychecks = BuildPaychecks(employee, 2020);
            db.Employees.AddRange(
                employee
            );
            db.SaveChanges();
        }

        public static List<Paycheck> BuildPaychecks(Employee e, int year)
        {
            decimal annual = e.AnnualGrossPay ?? Paycheck.DefaultAnnualGrossPay;
            decimal paycheckAmount = 1.0M * annual / Paycheck.PaychecksPerYear;
            List<Paycheck> result = new List<Paycheck>(Paycheck.PaychecksPerYear);
            var dateFrom = new DateTime(year, 1, 1);
            int daysPerCheck = (int)365 / Paycheck.PaychecksPerYear;
            DateTime dateTo;
            for (int i = 0; i < Paycheck.PaychecksPerYear; i++)
            {
                dateTo = dateFrom.AddDays(daysPerCheck-1);
                var paycheck = new Paycheck()
                {
                    GrossAmount = paycheckAmount,
                    StartDate = dateFrom,
                    EndDate = dateTo,
                    Index = i+1,
                    Year = year,
                    Employee = e
                };
                result.Add(paycheck);
                paycheck.AdjustResidualGrossAmount();
                dateFrom = dateFrom.AddDays(daysPerCheck);
            }

            return result;
        }

    }
}
