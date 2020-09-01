using System.Threading.Tasks;

namespace CRMToPostgresDataPipeline.lib
{
    public interface IGetRecordsFromCrm
    {
        public Task<string> GetToken();
        public Task<string> GetRecords(string token);
    }
}