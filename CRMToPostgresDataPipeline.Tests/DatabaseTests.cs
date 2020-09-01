using CRMToPostgresDataPipeline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;

namespace CRMToPostgresDataPipeline.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        protected IDbContextTransaction _transaction;

        protected DbContextOptionsBuilder _builder;

        protected ResidentContactContext ResidentContactContext { get; set; }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            _builder = new DbContextOptionsBuilder();
            _builder.UseNpgsql(ConnectionString.TestDatabase());
        }

        [SetUp]
        public void SetUp()
        {
            ResidentContactContext = new ResidentContactContext(_builder.Options);
            ResidentContactContext.Database.EnsureCreated();

            ResidentContactContext.Residents.RemoveRange(ResidentContactContext.Residents);
            ResidentContactContext.ContactDetails.RemoveRange(ResidentContactContext.ContactDetails);
            ResidentContactContext.ContactTypeLookups.RemoveRange(ResidentContactContext.ContactTypeLookups);
            ResidentContactContext.ExternalSystemRecords.RemoveRange(ResidentContactContext.ExternalSystemRecords);
            ResidentContactContext.ExternalSystemLookups.RemoveRange(ResidentContactContext.ExternalSystemLookups);

            _transaction = ResidentContactContext.Database.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            _transaction.Rollback();
            _transaction.Dispose();

            ResidentContactContext.Database.EnsureDeleted();
        }
    }
}