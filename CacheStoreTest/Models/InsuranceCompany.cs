using System.ComponentModel.DataAnnotations;

namespace CacheStoreTest.Models
{
    public class InsuranceCompany
    {
        [Key]
        public int InsuranceCompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

    }
}
