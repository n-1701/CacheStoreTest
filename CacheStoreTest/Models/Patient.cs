using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CacheStoreTest.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }


    }

}
