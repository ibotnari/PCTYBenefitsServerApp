namespace ServerApp.Models
{
    public class Dependent:Person
    {
        public DependentRelationshipToEmployee DependentRelationshipToEmployee { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee {get; set; }
        
        //public virtual ICollection<EmployeeBenefit> Benefits { get; set; }
    }
}