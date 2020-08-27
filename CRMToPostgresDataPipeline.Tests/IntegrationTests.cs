using System;
using NUnit.Framework;
using WireMock.Server;

namespace CRMToPostgresDataPipeline.Tests
{
    [TestFixture]
    public class IntegrationTests : DatabaseTests
    {
        protected WireMockServer MockTokenAPI { get; private set; }
        protected WireMockServer MockCrmAPI { get; private set; }

        [SetUp]
        public void BaseSetup()
        {
            ConfigureMockApis();
        }

        [TearDown]
        public void BaseTearDown()
        {
            MockTokenAPI.Stop();
            MockTokenAPI.Dispose();

            MockCrmAPI.Stop();
            MockCrmAPI.Dispose();
        }

        private void ConfigureMockApis()
        {
            MockTokenAPI = WireMockServer.Start();
            MockCrmAPI = WireMockServer.Start();

            Environment.SetEnvironmentVariable("TOKEN_GEN_URL", $"http://localhost:{MockTokenAPI.Ports[0]}/");
            Environment.SetEnvironmentVariable("CRM_URL", $"http://localhost:{MockCrmAPI.Ports[0]}/");
        }
    }
}