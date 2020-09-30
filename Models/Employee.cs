using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models
{
    public class Employee: Person
    {
        [DataType(DataType.Date)]
        [Required]
        public DateTime? StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(32, 2)")]
        public decimal? AnnualGrossPay { get; set; }

        public virtual ICollection<Paycheck> Paychecks { get; set; }
        public virtual ICollection<Dependent> Dependents { get; set; }
    }
}