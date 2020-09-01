using System.Collections.Generic;
using CRMToPostgresDataPipeline.lib.Domain;

namespace CRMToPostgresDataPipeline.lib
{
    public interface ILoadRecordsIntoDatabase
    {
        public void LoadDataIntoDB(IEnumerable<ResidentContact> recordsToLoadIntoDB);
    }
}