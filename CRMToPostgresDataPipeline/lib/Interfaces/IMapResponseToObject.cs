using System.Collections.Generic;
using CRMToPostgresDataPipeline.lib.Domain;

namespace CRMToPostgresDataPipeline.lib
{
    public interface IMapResponseToObject
    {
        List<RecordValue> MapJsonResponseToRecordValue(string responseFromCrm);

        List<ResidentContact> CreateResidentContactToLoadIntoDatabase(IEnumerable<RecordValue> mappedResponse);
    }
}