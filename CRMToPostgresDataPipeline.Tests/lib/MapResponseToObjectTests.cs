using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CRMToPostgresDataPipeline.lib;
using CRMToPostgresDataPipeline.lib.Domain;
using FluentAssertions;
using NUnit.Framework;
using Resident = CRMToPostgresDataPipeline.lib.Domain.Resident;

namespace CRMToPostgresDataPipeline.Tests.lib
{
    public class MapResponseToObjectTests
    {
        private string _responseFromCrm;
        private MapResponseToObject _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _responseFromCrm = File.ReadAllText(@"./../../../TestFixtures/ExampleCrmResponse.json");
            _classUnderTest = new MapResponseToObject();
        }

        [Test]
        public void ClassImplementsBoundaryInterface()
        {
            Assert.NotNull(_classUnderTest is IMapResponseToObject);
        }

        [Test]
        public void CreatesMappedResponseObjectFromJSONResponse()
        {
            var result = _classUnderTest.MapJsonResponseToRecordValue(_responseFromCrm);
            var expectedRecordOne = result.ElementAt(0);
            var expectedRecordTwo = result.ElementAt(1);

            var datetimeForRecordOne = new DateTime(2000, 12, 01);

            expectedRecordOne.hackney_communicationdetails.Should().NotBeNull();
            expectedRecordOne.firstname.Should().BeEquivalentTo("Hello");
            expectedRecordOne.lastname.Should().BeEquivalentTo("Goodbye");
            expectedRecordOne.birthdate.Should().Be(datetimeForRecordOne);
            expectedRecordOne.hackney_gender.Should().BeEquivalentTo(null);
            expectedRecordOne.hackney_houseref.Should().BeEquivalentTo("000000");
            expectedRecordOne.hackney_personno.Should().BeEquivalentTo("1");
            expectedRecordOne.contactid.Should().BeEquivalentTo("aaaaaa-1111");

            expectedRecordTwo.hackney_communicationdetails.Should().NotBeNull();
            expectedRecordTwo.firstname.Should().BeEquivalentTo("Golden");
            expectedRecordTwo.lastname.Should().BeEquivalentTo("Rose");
            expectedRecordTwo.birthdate.Should().Be(null);
            expectedRecordTwo.hackney_gender.Should().BeEquivalentTo("M");
            expectedRecordTwo.hackney_houseref.Should().BeEquivalentTo("111111");
            expectedRecordTwo.hackney_personno.Should().BeEquivalentTo("6");
            expectedRecordTwo.contactid.Should().BeEquivalentTo("bbbb-33333");
        }

        [Test]
        public void CreatesTableRecordsObjectFromResponse()
        {
            var mappedResponse = _classUnderTest.MapJsonResponseToRecordValue(_responseFromCrm);
            var recordsToLoadIntoDB = _classUnderTest.CreateResidentContactToLoadIntoDatabase(mappedResponse);

            var expectedResultForRecordOne = new ResidentContact
            {
                Resident = new Resident
                {
                    Firstname = "Hello",
                    Lastname = "Goodbye",
                    DateOfBirth = new DateTime(2000, 12, 01),
                    Gender = null
                },
                CommunicationDetails = new CommunicationDetails
                {
                    telephone = new List<string>(new string[] { "2222222222" }),
                    email = new List<string>(new string[] { "test.hello@hackney.gov.uk", "testtest.default@hackney.gov.uk" }),
                    mobile = new List<string>(new string[] { "11111111111", "00000000000" }),
                    Default = new DefaultContacts
                    {
                        telephone = "2222222222",
                        email = "testtest.default@hackney.gov.uk",
                        mobile = "11111111111"
                    }
                },
                DetailsFromExternalRecords = new DetailsFromExternalRecords
                {
                    HouseRef = "000000",
                    PersonNo = "1",
                    ContactId = "aaaaaa-1111"
                }
            };

            var expectedResultForRecordTwo = new ResidentContact()
            {
                Resident = new Resident
                {
                    Firstname = "Golden",
                    Lastname = "Rose",
                    DateOfBirth = null,
                    Gender = 'M'
                },
                CommunicationDetails = new CommunicationDetails
                {
                    telephone = new List<string>(new string[] { }),
                    email = new List<string>(new string[] { }),
                    mobile = new List<string>(new string[] { "0700033867- new", "01111111111" }),
                    Default = new DefaultContacts
                    {
                        telephone = null,
                        email = null,
                        mobile = "01111111111"
                    }
                },
                DetailsFromExternalRecords = new DetailsFromExternalRecords
                {
                    HouseRef = "111111",
                    PersonNo = "6",
                    ContactId = "bbbb-33333"
                }
            };

            recordsToLoadIntoDB.ElementAt(0).Should().BeEquivalentTo(expectedResultForRecordOne);
            recordsToLoadIntoDB.ElementAt(1).Should().BeEquivalentTo(expectedResultForRecordTwo);
        }

        [Test]
        public void MapsHackneyCommunicationDetailsStringToCommunicationDetailsObject()
        {
            var mappedResponse = _classUnderTest.MapJsonResponseToRecordValue(_responseFromCrm);
            var expectedTelephoneArray = new List<string>(new string[] { "2222222222" });
            var expectedEmailArray = new List<string>(new string[] { "test.hello@hackney.gov.uk", "testtest.default@hackney.gov.uk" });
            var expectedMobileArray = new List<string>(new string[] { "11111111111", "00000000000" });

            var expectedDefaultContacts = new DefaultContacts
            {
                telephone = "2222222222",
                email = "testtest.default@hackney.gov.uk",
                mobile = "11111111111"
            };


            var result = MapResponseToObject.CreateCommunicationDetailsObject(mappedResponse.FirstOrDefault());

            result.telephone.Should().BeEquivalentTo(expectedTelephoneArray);
            result.mobile.Should().BeEquivalentTo(expectedMobileArray);
            result.email.Should().BeEquivalentTo(expectedEmailArray);
            result.Default.telephone.Should().BeEquivalentTo(expectedDefaultContacts.telephone);
            result.Default.email.Should().BeEquivalentTo(expectedDefaultContacts.email);
            result.Default.mobile.Should().BeEquivalentTo(expectedDefaultContacts.mobile);
        }

        [Test]
        public void CreatesResidentObjectFromResponse()
        {
            var recordValues = _classUnderTest.MapJsonResponseToRecordValue(_responseFromCrm);

            var expectedResidentDetails = new Resident
            {
                Firstname = "Hello",
                Lastname = "Goodbye",
                DateOfBirth = new DateTime(2000, 12, 01),
                Gender = null
            };

            var result = MapResponseToObject.CreateResidentObject(recordValues.FirstOrDefault());
            result.Firstname.Should().BeEquivalentTo(expectedResidentDetails.Firstname);
            result.Lastname.Should().BeEquivalentTo(expectedResidentDetails.Lastname);
            result.DateOfBirth.Should().Be(expectedResidentDetails.DateOfBirth);
            result.Gender.Should().BeEquivalentTo(expectedResidentDetails.Gender);
        }

        [Test]
        public void CreatesDetailsFromExternalRecordsObjectFromResponse()
        {
            var recordValues = _classUnderTest.MapJsonResponseToRecordValue(_responseFromCrm);

            var expectedExternalRecord = new DetailsFromExternalRecords
            {
                HouseRef = "000000",
                PersonNo = "1",
                ContactId = "aaaaaa-1111"
            };

            var result = MapResponseToObject.CreateDetailsFromExternalRecord(recordValues.FirstOrDefault());
            result.HouseRef.Should().BeEquivalentTo(expectedExternalRecord.HouseRef);
            result.PersonNo.Should().BeEquivalentTo(expectedExternalRecord.PersonNo);
            result.ContactId.Should().BeEquivalentTo(expectedExternalRecord.ContactId);
        }
    }
}