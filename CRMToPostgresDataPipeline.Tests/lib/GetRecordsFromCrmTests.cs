using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bogus;
using CRMToPostgresDataPipeline.lib;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CRMToPostgresDataPipeline.Tests.lib
{
    [TestFixture]
    public class GetRecordsFromCrmTests
    {
        private GetRecordsFromCrm _classUnderTest;
        private Faker _faker;
        private Mock<HttpMessageHandler> _messageHandler;
        private Uri _uri;
        private HttpClient _httpClient;
        private string _password;
        private string _testTokenUri;
        private string _crmUri;

        [SetUp]
        public void Setup()
        {
            _faker = new Faker();
            _password = "password-for-getting-token";
            _testTokenUri = "http://test-token-generator.com/";
            _crmUri = "http://test-crm.com/";

            Environment.SetEnvironmentVariable("TOKEN_GEN_PASSWORD", _password);
            Environment.SetEnvironmentVariable("TOKEN_GEN_URL", _testTokenUri);
            Environment.SetEnvironmentVariable("CRM_URL", _crmUri);

            _messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);


            _httpClient = new HttpClient(_messageHandler.Object)
            {
                BaseAddress = _uri
            };

            _classUnderTest = new GetRecordsFromCrm(_httpClient);
        }

        [Test]
        public void ClassImplementsBoundaryInterface()
        {
            Assert.NotNull(_classUnderTest is IGetRecordsFromCrm);
        }

        [Test]
        public async Task GetTokenMakesAnApiCallToTokenGeneratorWithCorrectPassword()
        {
            _uri = new Uri(Environment.GetEnvironmentVariable("TOKEN_GEN_URL"));

            var expectedToken = _faker.Internet.Password();
            SetUpMessageHandlerToReturnValidResponse(_messageHandler, expectedToken, _testTokenUri, _password);

            var response = await _classUnderTest.GetToken().ConfigureAwait(true);


            response.Should().BeEquivalentTo(expectedToken);
            _messageHandler.Verify();

        }

        [Test]
        public async Task GetRecordMakesAnApiCallToCrmUsingPreviouslyRetrievedToken()
        {
            _uri = new Uri(Environment.GetEnvironmentVariable("CRM_URL"));

            const string authToken = "testAuthToken1";
            const string expectedResponse = @"{value: []}";
            const string expectedEndpoint = "api/data/v8.2/contacts";
            const string expectedQueryString =
                "$select=hackney_communicationdetails,firstname,lastname,birthdate,hackney_gender,hackney_houseref,hackney_personno&$filter=hackney_communicationdetails ne null";

            SetUpMessageHandlerToReturnValidResponse(_messageHandler, expectedResponse, _crmUri, authToken, expectedEndpoint, expectedQueryString);

            var response = await _classUnderTest.GetRecords(authToken).ConfigureAwait(true);


            response.Should().BeEquivalentTo(expectedResponse);
            _messageHandler.Verify();

        }

        private static void SetUpMessageHandlerToReturnValidResponse(Mock<HttpMessageHandler> messageHandler, string expectedJsonString, string targetUri, string authorisationValue, string endpoint = null, string query = null)
        {
            var stubbedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedJsonString)
            };

            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => CheckUrls(req, targetUri, authorisationValue, endpoint, query)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(stubbedResponse)
                .Verifiable();
        }

        private static bool CheckRequestForAuthorisation(HttpRequestMessage receivedRequest, string targetUri, string authorisationValue)
        {
            var requestUri = receivedRequest.RequestUri.ToString();
            var authorizationParameter = receivedRequest.Headers.Authorization.Parameter;
            return HttpUtility.UrlDecode(requestUri) == targetUri && authorizationParameter == authorisationValue;
        }

        private static bool CheckUrls(HttpRequestMessage receivedRequest, string targetUri, string authorisationValue, string endpoint = null, string query = null)
        {
            var requestUri = receivedRequest.RequestUri.ToString();
            var authorizationParameter = receivedRequest.Headers.Authorization.Parameter ?? receivedRequest.Headers.Authorization.ToString();


            if (endpoint == null && query == null)
            {
                return HttpUtility.UrlDecode(requestUri) == targetUri && authorizationParameter == authorisationValue;
            }

            var expectedUrl = $"{targetUri}{endpoint}?{query}";

            return HttpUtility.UrlDecode(requestUri) == expectedUrl && authorizationParameter == authorisationValue;
        }
    }
}