using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CacheStoreTest.Models
{
    public class HealthcareProvider
    {
        [Key]
        public int HealthcareProviderId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        
    }
}
