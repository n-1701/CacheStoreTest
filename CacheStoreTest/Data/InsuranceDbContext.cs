using Bogus;
using CacheStoreTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace CacheStoreTest.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<InsuranceCompany> Providers { get; set; }
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<Claim> Claims { get; set; }
       // public Faker<Patient> PatientFaker { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Patient>()
            //.HasMany(e => e.Claims)
            //.WithOne(e => e.Patient)
            //.HasForeignKey(e => e.PatientID)
            //.IsRequired();
            // Debugger.Launch();
            GenerateData(modelBuilder);
        }
       
        private static void GenerateData(ModelBuilder modelBuilder)
        {
            //List<Patient> patients = GeneratePatients();
            //modelBuilder.Entity<Patient>().HasData(patients);

            //List<HealthcareProvider> healthcareProviders = GenerateHealthcareProviders();
            //modelBuilder.Entity<HealthcareProvider>().HasData(healthcareProviders);

            //List<InsuranceCompany> insuranceCompanies = GenerateInsuranceCompanies();
            //modelBuilder.Entity<InsuranceCompany>().HasData(insuranceCompanies);

            List<Claim> claims = GenerateClaims(null, null, null);
            modelBuilder.Entity<Claim>()
                .HasData(claims);
        }

        private static List<Claim> GenerateClaims(List<Patient> patients, List<HealthcareProvider> healthcareProviders, List<InsuranceCompany> insuranceCompanies)
        {
            int claimsIdStart = 10;
            var claimsFaker = new Faker<Claim>()
                .UseSeed(3000)
                .RuleFor(c => c.ClaimID, f => claimsIdStart++)
               // .RuleFor(c => c.Patient, f => PatientFaker)
                //.RuleFor(c => c.InsuranceCompany, f => f.PickRandom(insuranceCompanies))
                //.RuleFor(c => c.HealthcareProvider, f => f.PickRandom(healthcareProviders))
                .RuleFor(c => c.PatientID, f => f.PickRandom(Enumerable.Range(9000, 1000).ToList()))
                //.RuleFor(c => c.PatientID, (f, p) => p.Patient.PatientId)
                .RuleFor(c => c.InsuranceCompanyID, f => f.PickRandom(Enumerable.Range(5010, 5).ToList()))
                .RuleFor(c => c.HealthcareProviderID, f => f.PickRandom(Enumerable.Range(2801, 50).ToList()))
                .RuleFor(c => c.Amount, f => f.Random.Int(1000, 1000000));
            List<Claim> claims = claimsFaker.Generate(2500);
            return claims;
        }

        private static List<InsuranceCompany> GenerateInsuranceCompanies()
        {
            // Seed companies
            int insuranceCompaniesIdStart = 5010;
            var insuranceCompaniesFaker = new Faker<InsuranceCompany>()
                .UseSeed(2000)
                .RuleFor(h => h.InsuranceCompanyId, h => insuranceCompaniesIdStart++)
                .RuleFor(h => h.Name, h => h.Company.CompanyName())
                .RuleFor(h => h.Phone, h => h.Phone.PhoneNumber());

            List<InsuranceCompany> insuranceCompanies = insuranceCompaniesFaker.Generate(0);
            return insuranceCompanies;
        }

        private static List<HealthcareProvider> GenerateHealthcareProviders()
        {
            // Seed providers
            int healthcareProviderIdStart = 2801;
            var healthcareProviderFaker = new Faker<HealthcareProvider>()
                .UseSeed(1000)
                .RuleFor(h => h.HealthcareProviderId, h => healthcareProviderIdStart++)
                .RuleFor(h => h.Name, h => h.Company.CompanyName())
                .RuleFor(h => h.Phone, h => h.Phone.PhoneNumber());

            List<HealthcareProvider> healthcareProviders = healthcareProviderFaker.Generate(0);
            return healthcareProviders;
        }

        private static List<Patient> GeneratePatients()
        {
            // Seed patients
            int patientIdStart = 9000;
            var patientFaker = new Faker<Patient>()
            .UseSeed(42)
            .RuleFor(p => p.PatientId, f => patientIdStart++)
            .RuleFor(p => p.Email, f => f.Internet.Email())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber());
            //PatientFaker = patientFaker;
            List<Patient> patients = patientFaker.Generate(0);
            return patients;
        }
    }
}
