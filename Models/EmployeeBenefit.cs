using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    /// <summary>
    /// Assumption : once the Benefit is created and used it is not editable
    /// </summary>
    public abstract class Benefit
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        [Required]
        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal AnnualCost { get; set; }
        [Required]
        [MaxLength(100)]
        public string Description { get; set; }
        public virtual ICollection<BenefitDiscount> BenefitDiscounts { get; set; }
    }

    public abstract class BenefitDiscount
    {
        public int Id { get; set; }
        [Required]
        [Range(0, 1)]
        [Column(TypeName = "decimal(5, 4)")]
        public decimal Percent { get; set; }

        public abstract bool DoesApply(PaycheckBenefitCost benefitCost);
    }
    public class NameStartsWithBenefitDiscount: BenefitDiscount
    {
        [Required]
        [MinLength(1)]
        [MaxLength(100)]
        public string NameStartsWith { get; set; }
        public override bool DoesApply(PaycheckBenefitCost benefitCost)
        {
            if (benefitCost.BenefitReceiver == null || string.IsNullOrEmpty(benefitCost.BenefitReceiver.FirstName)) return false;
            return benefitCost.BenefitReceiver.FirstName.ToLower().StartsWith(NameStartsWith.ToLower());
        }
    }
    public class EmployeeBenefit:Benefit
    {
        
    }
    public class DependentBenefit : Benefit
    {
        
    }
}
