using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CRMToPostgresDataPipeline.lib
{
    public class GetRecordsFromCrm : IGetRecordsFromCrm
    {
        private readonly HttpClient _client;
        private readonly string _tokenGenPassword;
        private readonly string _tokenUrl;
        private readonly string _crmUrl;

        public GetRecordsFromCrm(HttpClient httpClient)
        {
            _client = httpClient;
            _tokenGenPassword = Environment.GetEnvironmentVariable("TOKEN_GEN_PASSWORD"); ;
            _tokenUrl = Environment.GetEnvironmentVariable("TOKEN_GEN_URL");
            _crmUrl = Environment.GetEnvironmentVariable("CRM_URL");
        }

        public async Task<string> GetToken()
        {
            var builder = new UriBuilder(_tokenUrl);

            var request = new HttpRequestMessage
            {
                RequestUri = builder.Uri,
                Method = HttpMethod.Get
            };

            _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _tokenGenPassword);

            var response = await _client.SendAsync(request, CancellationToken.None).ConfigureAwait(true);
            var token = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            return token;
        }

        public async Task<string> GetRecords(string token)
        {
            const string contactsEndpoint = "api/data/v8.2/contacts";
            const string selectQuery = "hackney_communicationdetails,firstname,lastname,birthdate,hackney_gender,hackney_houseref,hackney_personno";
            const string filterQuery = "hackney_communicationdetails ne null";
            var builder = new UriBuilder(_crmUrl);

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["$select"] = selectQuery;
            query["$filter"] = filterQuery;
            builder.Query = query.ToString();
            builder.Path = contactsEndpoint;

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(builder.ToString()),
                Method = HttpMethod.Get,
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            var response = await _client.SendAsync(request, CancellationToken.None).ConfigureAwait(true);
            var records = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            return records;
        }
    }
}
