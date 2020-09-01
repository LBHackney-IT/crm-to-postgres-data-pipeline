using System;
using System.Collections.Generic;
using System.Linq;
using CRMToPostgresDataPipeline.lib;
using CRMToPostgresDataPipeline.lib.Domain;
using FluentAssertions;
using NUnit.Framework;
using DBResident = CRMToPostgresDataPipeline.Infrastructure.Resident;
using Resident = CRMToPostgresDataPipeline.lib.Domain.Resident;

namespace CRMToPostgresDataPipeline.Tests.lib
{
    public class LoadRecordsIntoDatabaseTests : DatabaseTests
    {
        private LoadRecordsIntoDatabase _classUnderTest;
        private ResidentContact _recordOne;
        private ResidentContact _recordTwo;
        private List<ResidentContact> _mappedRecords;

        [SetUp]
        public void Setup()
        {
            _recordOne = new ResidentContact
            {
                Resident = new Resident
                {
                    Firstname = "test-firstname",
                    Lastname = "test-lastname",
                    DateOfBirth = new DateTime(2020, 12, 12),
                    Gender = 'M'
                },
                CommunicationDetails = new CommunicationDetails
                {
                    telephone = new List<string>(new string[] { "0000011112222" }),
                    email = new List<string>(new string[] { "Hi.hello@hackney.gov.uk" }),
                    mobile = new List<string>(new string[] { "11111111111-new", "444444444" }),
                    Default = new DefaultContacts
                    {
                        telephone = "0000011112222",
                        email = "Hi.hello@hackney.gov.uk",
                        mobile = "11111111111-new"
                    }
                },
                DetailsFromExternalRecords = new DetailsFromExternalRecords
                {
                    HouseRef = "test-houseRef",
                    PersonNo = "test-person-no",
                    ContactId = "test-contact-id"
                }
            };

            _recordTwo = new ResidentContact
            {
                Resident = new Resident
                {
                    Firstname = "one name",
                    Lastname = "another name",
                    DateOfBirth = new DateTime(2020, 12, 12),
                    Gender = 'F'
                },
                CommunicationDetails = new CommunicationDetails
                {
                    telephone = new List<string>(new string[] { }),
                    email = new List<string>(new string[] { "I-am-an-email@address.com" }),
                    mobile = new List<string>(new string[] { }),
                    Default = new DefaultContacts
                    {
                        telephone = null,
                        email = "I-am-an-email@address.com",
                        mobile = null
                    }
                },
                DetailsFromExternalRecords = new DetailsFromExternalRecords
                {
                    HouseRef = "testRef",
                    PersonNo = "testNo",
                    ContactId = "testId"
                }
            };

            _mappedRecords = new List<ResidentContact>
            {
                _recordOne,
                _recordTwo
            };

            _classUnderTest = new LoadRecordsIntoDatabase(ResidentContactContext);
        }

        [Test]
        public void ClassImplementsBoundaryInterface()
        {
            Assert.NotNull(_classUnderTest is ILoadRecordsIntoDatabase);
        }

        [Test]
        public void LoadsDataFromAllReturnedRecordsIntoTheRelevantDatabaseTables()
        {
            ResidentContactContext.Residents.Count().Should().Be(0);
            ResidentContactContext.ContactDetails.Count().Should().Be(0);
            ResidentContactContext.ContactTypeLookups.Count().Should().Be(0);
            ResidentContactContext.ExternalSystemLookups.Count().Should().Be(0);
            ResidentContactContext.ExternalSystemRecords.Count().Should().Be(0);

            _classUnderTest.LoadDataIntoDB(_mappedRecords);

            ResidentContactContext.Residents.Count().Should().Be(2);
            ResidentContactContext.ContactDetails.Count().Should().Be(5);
            ResidentContactContext.ContactTypeLookups.Count().Should().Be(3);
            ResidentContactContext.ExternalSystemLookups.Count().Should().Be(2);
            ResidentContactContext.ExternalSystemRecords.Count().Should().Be(6);
        }

        [Test]
        public void LoadsDetailsFromEachRecordIntoTheRelevantDatabaseTables()
        {
            var residentRecords = ResidentContactContext.Residents.OrderBy(r => r.Id);
            var contactDetailRecords = ResidentContactContext.ContactDetails.OrderBy(cd => cd.Id);
            var contactTypeLookups = ResidentContactContext.ContactTypeLookups.OrderBy(ct => ct.Id);
            var sourceSystemsLookups = ResidentContactContext.ExternalSystemLookups.OrderBy(s => s.Id);
            var externalSystemRecords = ResidentContactContext.ExternalSystemRecords.OrderBy(ex => ex.Id);

            _classUnderTest.LoadDataIntoDB(_mappedRecords);

            // ------------------ Check Resident

            var dbResidentOne = residentRecords.FirstOrDefault();

            var recordOneResidentDetails = _recordOne.Resident;
            var recordTwoResidentDetails = _recordTwo.Resident;

            dbResidentOne.FirstName.Should().BeEquivalentTo(recordOneResidentDetails.Firstname);
            dbResidentOne.LastName.Should().BeEquivalentTo(recordOneResidentDetails.Lastname);
            dbResidentOne.DateOfBirth.Should().Be(recordOneResidentDetails.DateOfBirth);
            dbResidentOne.Gender.Should().Be(recordOneResidentDetails.Gender);

            var dbResidentTwo = residentRecords.Last();

            dbResidentTwo.FirstName.Should().BeEquivalentTo(recordTwoResidentDetails.Firstname);
            dbResidentTwo.LastName.Should().BeEquivalentTo(recordTwoResidentDetails.Lastname);
            dbResidentTwo.DateOfBirth.Should().Be(recordTwoResidentDetails.DateOfBirth);
            dbResidentTwo.Gender.Should().Be(recordTwoResidentDetails.Gender);
            // ---------------- Check Contact Lookup
            var telephoneLookupRecords = contactTypeLookups.Where(x => x.Name == "Telephone");
            var mobileLookupRecords = contactTypeLookups.Where(x => x.Name == "Mobile");
            var emailLookupRecords = contactTypeLookups.Where(x => x.Name == "Email");

            telephoneLookupRecords.Count().Should().Be(1);
            mobileLookupRecords.Count().Should().Be(1);
            emailLookupRecords.Count().Should().Be(1);

            //Check that default isn't added as a contact type
            contactTypeLookups.Count(x => x.Name == "Default").Should().Be(0);

            // ----------------- Check Contact Details

            var telephoneLookupId = telephoneLookupRecords.First().Id;
            var mobileLookupId = mobileLookupRecords.First().Id;
            var emailLookupId = emailLookupRecords.First().Id;

            var expectedContactDetailsForResidentOne =
                contactDetailRecords.Where(cd => cd.ResidentId.Equals(dbResidentOne.Id));

            var expectedContactDetailsForResidentTwo =
                contactDetailRecords.Where(cd => cd.ResidentId.Equals(dbResidentTwo.Id));

            expectedContactDetailsForResidentOne.Count().Should().Be(4);
            expectedContactDetailsForResidentTwo.Count().Should().Be(1);

            foreach (var record in contactDetailRecords)
            {
                record.DateAdded.Should().BeSameDateAs(DateTime.UtcNow);
                record.DateLastModified.Should().BeSameDateAs(DateTime.UtcNow);
            }


            CheckContactValueIsDefault("0000011112222", telephoneLookupId, dbResidentOne.Id).Should().BeTrue();


            CheckContactValueIsDefault("11111111111-new", mobileLookupId, dbResidentOne.Id).Should().BeTrue();

            CheckContactValueIsDefault("444444444", mobileLookupId, dbResidentOne.Id).Should().BeFalse();


            CheckContactValueIsDefault("Hi.hello@hackney.gov.uk", emailLookupId, dbResidentOne.Id).Should().BeTrue();

            CheckContactValueIsDefault("I-am-an-email@address.com", emailLookupId, dbResidentTwo.Id).Should().BeTrue();

            //----------- Check Source Systems

            var uHLookUps = sourceSystemsLookups.Where(x => x.Name == "UH");
            var crmLookUps = sourceSystemsLookups.Where(x => x.Name == "CRM");

            uHLookUps.Count().Should().Be(1);
            crmLookUps.Count().Should().Be(1);

            // --------------- Check External Systems

            var expectedExternalDetailsForResidentOne =
                externalSystemRecords.Where(cd => cd.ResidentId.Equals(dbResidentOne.Id));

            var expectedExternalDetailsForResidentTwo =
                externalSystemRecords.Where(cd => cd.ResidentId.Equals(dbResidentTwo.Id));

            expectedExternalDetailsForResidentOne.Count().Should().Be(3);
            expectedExternalDetailsForResidentTwo.Count().Should().Be(3);


            var uhLookUpId = uHLookUps.FirstOrDefault(u => u.Name.Equals("UH")).Id;
            var crmLookupId = crmLookUps.FirstOrDefault(c => c.Name.Equals("CRM")).Id;

            // Check records retrieved using source system ids have expected field names and values
            CheckValueSavedInExternalSystemRecord(uhLookUpId, dbResidentOne.Id, "HouseRef").Should()
                .BeEquivalentTo("test-houseRef");

            CheckValueSavedInExternalSystemRecord(uhLookUpId, dbResidentOne.Id, "PersonNo").Should()
                .BeEquivalentTo("test-person-no");

            CheckValueSavedInExternalSystemRecord(crmLookupId, dbResidentOne.Id, "ContactId").Should()
                .BeEquivalentTo("test-contact-id");



            CheckValueSavedInExternalSystemRecord(uhLookUpId, dbResidentTwo.Id, "HouseRef").Should()
                .BeEquivalentTo("testRef");

            CheckValueSavedInExternalSystemRecord(uhLookUpId, dbResidentTwo.Id, "PersonNo").Should()
                .BeEquivalentTo("testNo");

            CheckValueSavedInExternalSystemRecord(crmLookupId, dbResidentTwo.Id, "ContactId").Should()
                .BeEquivalentTo("testId");
        }

        private string CheckValueSavedInExternalSystemRecord(int externalSysLookupId, int residentId, string expectedFieldName)
        {
            var record = ResidentContactContext.ExternalSystemRecords
                .FirstOrDefault(ex => ex.ExternalSystemLookupId.Equals(externalSysLookupId) && ex.ResidentId.Equals(residentId) && ex.Name.Equals(expectedFieldName));

            return record.Value;
        }

        private bool CheckContactValueIsDefault(string contactValue, int lookupId, int residentId)
        {
            var contactDetail = ResidentContactContext.ContactDetails
                .FirstOrDefault(l => l.ContactValue.Equals(contactValue) && l.ContactTypeLookupId.Equals(lookupId) && l.ResidentId.Equals(residentId));
            return contactDetail != null && contactDetail.IsDefault;
        }
    }
}