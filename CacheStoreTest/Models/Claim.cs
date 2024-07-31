using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CacheStoreTest.Models
{
    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }

        [Required]
        public int PatientID { get; set; }

        [Required]
        public int HealthcareProviderID { get; set; }

        [Required]
        public int InsuranceCompanyID { get; set; }

        [Required]
        public int Amount { get; set; }
    }
}
