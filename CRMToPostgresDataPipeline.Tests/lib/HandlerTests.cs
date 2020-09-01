using System;
using System.Collections.Generic;
using CRMToPostgresDataPipeline.lib;
using CRMToPostgresDataPipeline.lib.Domain;
using Moq;
using NUnit.Framework;

namespace CRMToPostgresDataPipeline.Tests.lib
{
    public class HandlerTests
    {
        private Mock<IGetRecordsFromCrm> _mockApiCaller;
        private Mock<IMapResponseToObject> _mockResponseConverter;
        private Mock<ILoadRecordsIntoDatabase> _mockRecordToDbLoader;
        private Handler _classUnderTest;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Setup()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();

            _mockApiCaller = new Mock<IGetRecordsFromCrm>();
            _mockResponseConverter = new Mock<IMapResponseToObject>();
            _mockRecordToDbLoader = new Mock<ILoadRecordsIntoDatabase>();

            _mockServiceProvider.Setup(x => x.GetService(typeof(IGetRecordsFromCrm))).Returns(_mockApiCaller.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IMapResponseToObject))).Returns(_mockResponseConverter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoadRecordsIntoDatabase))).Returns(_mockRecordToDbLoader.Object);

            _classUnderTest = new Handler(_mockServiceProvider.Object);
        }

        [Test]
        public void ExecuteTriggersTheProcessOfGettingResponsesFromCrmIntoTheDatabase()
        {
            const string testToken = "test";

            const string testResponse = @"{value: []}";

            var testMappedResponse = new List<RecordValue>();
            var testTableRecord = new List<ResidentContact>();

            _mockApiCaller.Setup(m => m.GetToken()).ReturnsAsync(testToken);
            _mockApiCaller.Setup(m => m.GetRecords(testToken)).ReturnsAsync(testResponse);

            _mockResponseConverter.Setup(m => m.MapJsonResponseToRecordValue(testResponse)).Returns(testMappedResponse);
            _mockResponseConverter.Setup(m => m.CreateResidentContactToLoadIntoDatabase(testMappedResponse)).Returns(testTableRecord);

            _mockRecordToDbLoader.Setup(m => m.LoadDataIntoDB(testTableRecord));

            _classUnderTest.Execute();

            _mockApiCaller.Verify(m => m.GetToken(), Times.Once);
            _mockApiCaller.Verify(m => m.GetRecords(testToken), Times.Once);

            _mockResponseConverter.Verify(m => m.MapJsonResponseToRecordValue(testResponse), Times.Once);
            _mockResponseConverter.Verify(m => m.CreateResidentContactToLoadIntoDatabase(testMappedResponse), Times.Once);

            _mockRecordToDbLoader.Verify(m => m.LoadDataIntoDB(testTableRecord), Times.Once);
        }
    }
}