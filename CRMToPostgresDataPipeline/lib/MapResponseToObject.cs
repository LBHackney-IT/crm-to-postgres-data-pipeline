using System;
using System.Collections.Generic;
using System.Linq;
using CRMToPostgresDataPipeline.lib.Domain;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CRMToPostgresDataPipeline.lib
{
    public class MapResponseToObject : IMapResponseToObject
    {
        public List<RecordValue> MapJsonResponseToRecordValue(string responseFromCrm)
        {
            var response = JsonSerializer.Deserialize<CrmResponse>(responseFromCrm);
            return response.value;
        }

        public List<ResidentContact> CreateResidentContactToLoadIntoDatabase(IEnumerable<RecordValue> mappedResponse)
        {
            return mappedResponse.Select(record => new ResidentContact
            {
                Resident = CreateResidentObject(record),
                CommunicationDetails = CreateCommunicationDetailsObject(record),
                DetailsFromExternalRecords = CreateDetailsFromExternalRecord(record)
            }).ToList();
        }

        public static CommunicationDetails CreateCommunicationDetailsObject(RecordValue mappedResponse)
        {
            var details = mappedResponse.hackney_communicationdetails;
            var response = JsonConvert.DeserializeObject<CommunicationDetails>(details);
            return response;
        }

        public static Resident CreateResidentObject(RecordValue singleRecord)
        {
            return new Resident
            {
                Firstname = singleRecord.firstname,
                Lastname = singleRecord.lastname,
                DateOfBirth = singleRecord.birthdate,
                Gender = singleRecord.hackney_gender != null ? Convert.ToChar(singleRecord.hackney_gender) : (char?)null
            };
        }

        public static DetailsFromExternalRecords CreateDetailsFromExternalRecord(RecordValue singleRecord)
        {
            return new DetailsFromExternalRecords
            {
                HouseRef = singleRecord.hackney_houseref,
                PersonNo = singleRecord.hackney_personno,
                ContactId = singleRecord.contactid
            };
        }
    }
}