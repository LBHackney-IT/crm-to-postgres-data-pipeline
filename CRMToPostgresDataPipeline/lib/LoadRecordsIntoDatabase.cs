using System;
using System.Collections.Generic;
using System.Linq;
using CRMToPostgresDataPipeline.Infrastructure;
using CRMToPostgresDataPipeline.lib.Domain;
using DbContactTypeLookup = CRMToPostgresDataPipeline.Infrastructure.ContactTypeLookup;
using DbResident = CRMToPostgresDataPipeline.Infrastructure.Resident;

namespace CRMToPostgresDataPipeline.lib
{
    public class LoadRecordsIntoDatabase : ILoadRecordsIntoDatabase
    {
        private readonly ResidentContactContext _residentContactContext;

        public LoadRecordsIntoDatabase(ResidentContactContext residentContactContext)
        {
            _residentContactContext = residentContactContext;
        }

        public void LoadDataIntoDB(IEnumerable<ResidentContact> recordsToLoadIntoDB)
        {
            CheckContactLookUpTablesArePresent();
            CheckSourceSystemLookUpTablesArePresent();

            foreach (var residentContact in recordsToLoadIntoDB)
            {
                var residentDetail = residentContact.Resident;

                var databaseRecord = new DbResident
                {
                    FirstName = residentDetail.Firstname,
                    LastName = residentDetail.Lastname,
                    DateOfBirth = residentDetail.DateOfBirth,
                    Gender = residentDetail.Gender
                };

                _residentContactContext.Residents.Add(databaseRecord);
                _residentContactContext.SaveChanges();

                LoadContactDetailsAndTypesDataForResident(residentContact, databaseRecord.Id);
                LoadExternalAndSourceSystemsDataForResident(residentContact, databaseRecord.Id);
            }
        }

        private void CheckContactLookUpTablesArePresent()
        {
            var listOfContactTypes = new List<string> { "Telephone", "Email", "Mobile" };

            foreach (var contactType in listOfContactTypes)
            {
                AddContactTypeIfAbsent(contactType);
            }
        }

        private void AddContactTypeIfAbsent(string contactType)
        {
            if (_residentContactContext.ContactTypeLookups.Any(ct => ct.Name.Equals(contactType))) return;
            var databaseContactLookUp = new DbContactTypeLookup
            {
                Name = contactType
            };

            _residentContactContext.ContactTypeLookups.Add(databaseContactLookUp);
            _residentContactContext.SaveChanges();
        }

        private void CheckSourceSystemLookUpTablesArePresent()
        {
            var listOfSourceSystemTypes = new List<string> { "UH", "CRM" };

            foreach (var sourceSystemType in listOfSourceSystemTypes)
            {
                AddSourceSystemTypeIfAbsent(sourceSystemType);
            }
        }

        private void AddSourceSystemTypeIfAbsent(string sourceSystemType)
        {
            if (_residentContactContext.ExternalSystemLookups.Any(s => s.Name.Equals(sourceSystemType))) return;
            var dbSourceSystemRecord = new ExternalSystemLookup
            {
                Name = sourceSystemType
            };

            _residentContactContext.ExternalSystemLookups.Add(dbSourceSystemRecord);
            _residentContactContext.SaveChanges();
        }

        private void LoadContactDetailsAndTypesDataForResident(ResidentContact residentContact, int residentId)
        {
            var contactDetails = residentContact.CommunicationDetails;

            if (residentContact.CommunicationDetails.telephone != null)
            {
                foreach (var contactDetail in from number in contactDetails.telephone
                                              let telephoneLookupId = _residentContactContext.ContactTypeLookups.FirstOrDefault(x => x.Name.Equals("Telephone")).Id
                                              select new ContactDetail
                                              {
                                                  ContactTypeLookupId = telephoneLookupId,
                                                  ContactValue = number,
                                                  IsDefault = string.Equals(number, contactDetails.Default.telephone),
                                                  IsActive = string.Equals(number, contactDetails.Default.telephone),
                                                  DateAdded = DateTime.UtcNow,
                                                  DateLastModified = DateTime.UtcNow, //Making this the same as assumption is this would be run on an empty database
                                                  ResidentId = residentId
                                              })
                {
                    _residentContactContext.ContactDetails.Add(contactDetail);
                    _residentContactContext.SaveChanges();
                }
            }

            if (residentContact.CommunicationDetails.mobile != null)
            {
                foreach (var contactDetail in from number in contactDetails.mobile
                                              let mobileLookupId = _residentContactContext.ContactTypeLookups.FirstOrDefault(x => x.Name.Equals("Mobile")).Id
                                              select new ContactDetail
                                              {
                                                  ContactTypeLookupId = mobileLookupId,
                                                  ContactValue = number,
                                                  IsDefault = number.Equals(contactDetails.Default.mobile),
                                                  IsActive = number.Equals(contactDetails.Default.mobile),
                                                  DateAdded = DateTime.UtcNow,
                                                  DateLastModified = DateTime.UtcNow, //Making this the same as assumption is this would be run on an empty database
                                                  ResidentId = residentId
                                              })
                {
                    _residentContactContext.ContactDetails.Add(contactDetail);
                    _residentContactContext.SaveChanges();
                }
            }

            if (residentContact.CommunicationDetails.email != null)
            {
                foreach (var contactDetail in from number in contactDetails.email
                                              let emailLookupId = _residentContactContext.ContactTypeLookups.FirstOrDefault(x => x.Name.Equals("Email")).Id
                                              select new ContactDetail
                                              {
                                                  ContactTypeLookupId = emailLookupId,
                                                  ContactValue = number,
                                                  IsDefault = number.Equals(contactDetails.Default.email),
                                                  IsActive = number.Equals(contactDetails.Default.email),
                                                  DateAdded = DateTime.UtcNow,
                                                  DateLastModified = DateTime.UtcNow, //Making this the same as assumption is this would be run on an empty database
                                                  ResidentId = residentId
                                              })
                {
                    _residentContactContext.ContactDetails.Add(contactDetail);
                    _residentContactContext.SaveChanges();
                }
            }
        }

        private void LoadExternalAndSourceSystemsDataForResident(ResidentContact residentContact, int residentId)
        {
            var externalDetails = residentContact.DetailsFromExternalRecords;

            var uhExternalSystemLookUpId =
                _residentContactContext.ExternalSystemLookups.FirstOrDefault(x => x.Name.Equals("UH")).Id;

            var crmExternalSystemLookUpId =
                _residentContactContext.ExternalSystemLookups.FirstOrDefault(x => x.Name.Equals("CRM")).Id;

            if (externalDetails.HouseRef != null)
            {
                var dbExternalSystemsRecord = new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = uhExternalSystemLookUpId,
                    Name = "HouseRef",
                    Value = externalDetails.HouseRef
                };

                _residentContactContext.ExternalSystemRecords.Add(dbExternalSystemsRecord);
                _residentContactContext.SaveChanges();
            }

            if (externalDetails.PersonNo != null)
            {
                var dbExternalSystemsRecord = new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = uhExternalSystemLookUpId,
                    Name = "PersonNo",
                    Value = externalDetails.PersonNo
                };

                _residentContactContext.ExternalSystemRecords.Add(dbExternalSystemsRecord);
                _residentContactContext.SaveChanges();
            }

            if (externalDetails.ContactId != null)
            {
                var dbExternalSystemsRecord = new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = crmExternalSystemLookUpId,
                    Name = "ContactId",
                    Value = externalDetails.ContactId
                };

                _residentContactContext.ExternalSystemRecords.Add(dbExternalSystemsRecord);
                _residentContactContext.SaveChanges();
            }
        }
    }
}
