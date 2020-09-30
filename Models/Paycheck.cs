using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    public class Paycheck
    {
        public const int PaychecksPerYear = 26;
        public const decimal DefaultPaycheckAmount = 2000;
        public const decimal DefaultAnnualGrossPay = PaychecksPerYear * DefaultPaycheckAmount;
        public int Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Range(2020, 2100)]
        public int Year { get; set; }
        [Range(1, 26)]
        public int Index { get; set; }
        [Required]
        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal GrossAmount { get; set; }
        [Required]
        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal NetAmount { get; set; }
        
        // GrossAmount could go negative on the paycheck if Benefits are too large
        // Ex: family with 50 kids :) or paycheck is too low due to unpaid leave
        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal? BenefitsCost { get; set; }
        public DateTime? BenefitsCostCalculationDate { get; set; }
        public DateTime? SentDate { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public virtual Employee Employee { get; set; }
        public virtual ICollection<PaycheckBenefitCost> PaycheckBenefitsCosts { get; set; }
        
    }
}