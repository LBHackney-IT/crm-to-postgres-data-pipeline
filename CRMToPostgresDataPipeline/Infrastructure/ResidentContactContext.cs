using Microsoft.EntityFrameworkCore;

namespace CRMToPostgresDataPipeline.Infrastructure
{
    public class ResidentContactContext : DbContext
    {
        public ResidentContactContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Resident> Residents { get; set; }
        public DbSet<ContactDetail> ContactDetails { get; set; }
        public DbSet<ContactTypeLookup> ContactTypeLookups { get; set; }
        public DbSet<ExternalSystemRecord> ExternalSystemRecords { get; set; }
        public DbSet<ExternalSystemLookup> ExternalSystemLookups { get; set; }
    }
}