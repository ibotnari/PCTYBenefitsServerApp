using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models
{
    public class Person
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; set; }
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; set; }
    }
}