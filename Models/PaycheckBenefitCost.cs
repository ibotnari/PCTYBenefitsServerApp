using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    public sealed class PaycheckBenefitCost    
    {
        internal PaycheckBenefitCost()
        {
            
        }

        public PaycheckBenefitCost(Benefit benefit, Person benefitReceiver, Paycheck paycheck)
        {
            BenefitReceiver = benefitReceiver;
            Paycheck = paycheck;
            Benefit = benefit;
            CalculateAmount();
        }
        public int Id { get; set; }
        // stamped for change resilience
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public Person BenefitReceiver { get; internal set; }
        [Required]
        [Range(0.01, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Amount { get; internal set; }
        [Required]
        [Range(0.01, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal AmountBeforeDiscounts { get; internal set; }
        [Required]
        public Paycheck Paycheck { get; internal set; }
        public Benefit Benefit { get; internal set; }
        [Column(TypeName = "decimal(32,24)")]
        public decimal ResidualAmount { get; internal set; }

        internal void CalculateAmount()
        {
            AmountBeforeDiscounts = Benefit.AnnualCost / Paycheck.PaychecksPerYear;
            Amount = AmountBeforeDiscounts;
            foreach (var discount in Benefit.BenefitDiscounts)
            {
                ApplyDiscount(discount);
            }
            ResidualAmount = Amount;
            Amount = Math.Round(Amount, 2);
            ResidualAmount -= Amount;
        }

        private void ApplyDiscount(BenefitDiscount discount)
        {
            if (discount.DoesApply(this))
            {
                Amount *= (1 - discount.Percent);
            }
        }

        //TODO : Assumption that Name doesn't change otherwise uncomment
        // [Required]
        // public string BenefitReceiverFirstName { get; set; }
        // // stamped for change resilience
        // [Required]
        // public string BenefitReceiverLastName { get; set; }
    }
}