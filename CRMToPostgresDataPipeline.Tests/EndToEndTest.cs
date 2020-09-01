using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CRMToPostgresDataPipeline.lib;
using FluentAssertions;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CRMToPostgresDataPipeline.Tests
{
    public class EndToEndTest : IntegrationTests
    {
        private Handler _handler;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("TOKEN_GEN_PASSWORD", "token");

            Environment.SetEnvironmentVariable("CONNECTION_STRING", ConnectionString.TestDatabase());

            _handler = new Handler();
        }

        [Test]
        public void WhenTheScriptIsTriggeredUploadsCRMDataFromApiIntoPostgresDatabase()
        {
            var expectedJsonString = File.ReadAllText(@"./../../../TestFixtures/ExampleCrmResponse.json");
            var expectedAuthorisationToken = "You-may-pass";

            SetUpMessageHandlerToReturnValidResponse(MockTokenAPI, expectedAuthorisationToken);

            SetUpMessageHandlerToReturnValidResponse(MockCrmAPI, expectedJsonString);

            _handler.Execute();

            ResidentContactContext.Residents.Count().Should().Be(2);
            ResidentContactContext.ContactDetails.Count().Should().Be(7);

            ResidentContactContext.ContactTypeLookups.Count().Should().Be(3);
            ResidentContactContext.ExternalSystemLookups.Count().Should().Be(2);

            ResidentContactContext.ExternalSystemRecords.Count().Should().Be(6);

            var residentRecords = ResidentContactContext.Residents.OrderBy(r => r.Id);

            var dbResidentOne = residentRecords.FirstOrDefault();

            var recordOneDateOfBirth = new DateTime(2000, 12, 01);

            dbResidentOne.FirstName.Should().BeEquivalentTo("Hello");
            dbResidentOne.LastName.Should().BeEquivalentTo("Goodbye");
            dbResidentOne.DateOfBirth.Should().Be(recordOneDateOfBirth);
            dbResidentOne.Gender.Should().Be(null);

            var dbResidentTwo = residentRecords.Last();

            dbResidentTwo.FirstName.Should().BeEquivalentTo("Golden");
            dbResidentTwo.LastName.Should().BeEquivalentTo("Rose");
            dbResidentTwo.DateOfBirth.Should().Be(null);
            dbResidentTwo.Gender.Should().Be('M');
        }

        private static void SetUpMessageHandlerToReturnValidResponse(WireMockServer mockApi, string expectedJsonString)
        {
            mockApi.Given(Request.Create().UsingGet())
                .RespondWith(Response.Create().WithBody(expectedJsonString, encoding: Encoding.UTF8)
                    .WithStatusCode(HttpStatusCode.OK));
        }
    }
}