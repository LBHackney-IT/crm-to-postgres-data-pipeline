using System;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CRMToPostgresDataPipeline.lib
{
    public class Handler
    {
        private IServiceProvider _serviceProvider;

        public Handler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Handler()
        {
            var services = new ServiceCollection();
            services.ConfigureServices();

            _serviceProvider = services.BuildServiceProvider();
        }

        public void Execute()
        {
            LambdaLogger.Log("Lambda process started");
            try
            {
                var getRecordsFromCrm = _serviceProvider.GetService<IGetRecordsFromCrm>();
                var mapResponseToObjects = _serviceProvider.GetService<IMapResponseToObject>();
                var loadDataIntoDb = _serviceProvider.GetService<ILoadRecordsIntoDatabase>();

                try
                {
                    LambdaLogger.Log("Calling CRM token generator");
                    var token = getRecordsFromCrm.GetToken().Result;

                    LambdaLogger.Log("Getting records from CRM API");
                    var apiResponse = getRecordsFromCrm.GetRecords(token).Result;

                    LambdaLogger.Log("Mapping APU response to domain objects");
                    var mappedResponse = mapResponseToObjects.MapJsonResponseToRecordValue(apiResponse);
                    var dataToLoad = mapResponseToObjects.CreateResidentContactToLoadIntoDatabase(mappedResponse);

                    LambdaLogger.Log("Loading data into the database");
                    loadDataIntoDb.LoadDataIntoDB(dataToLoad);
                }
                catch (NpgsqlException x)
                {
                    LambdaLogger.Log($"NpgSql Exception has occurred - {x.Message} {x.InnerException} {x.StackTrace}");
                    throw x;
                }

                LambdaLogger.Log("Lambda process ended");
            }
            catch (Exception x)
            {
                LambdaLogger.Log($"Exception has occurred - {x.Message} {x.InnerException} {x.StackTrace}");
                throw x;
            }

        }
    }
}
