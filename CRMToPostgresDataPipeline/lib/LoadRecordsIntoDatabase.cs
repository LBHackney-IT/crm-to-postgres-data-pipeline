using System;
using System.Collections.Generic;
using System.Linq;
using CRMToPostgresDataPipeline.Infrastructure;
using CRMToPostgresDataPipeline.lib.Domain;
using Newtonsoft.Json;
using DbContactTypeLookup = CRMToPostgresDataPipeline.Infrastructure.ContactTypeLookup;
using DbResident = CRMToPostgresDataPipeline.Infrastructure.Resident;

namespace CRMToPostgresDataPipeline.lib
{
    public class LoadRecordsIntoDatabase : ILoadRecordsIntoDatabase
    {
        private readonly ResidentContactContext _residentContactContext;
        private int _telephoneId;
        private int _mobileId;
        private int _emailId;
        private int _crmExternalSystemLookUpId;
        private int _uhExternalSystemLookUpId;

        public LoadRecordsIntoDatabase(ResidentContactContext residentContactContext)
        {
            _residentContactContext = residentContactContext;
        }

        public void LoadDataIntoDB(IEnumerable<ResidentContact> recordsToLoadIntoDB)
        {
            CheckContactLookUpTablesArePresent();
            CheckSourceSystemLookUpTablesArePresent();
            GetLookupIds();

            var residentsToAdd = new List<DbResident>();
            var residents = recordsToLoadIntoDB.Select(residentContact =>
            {
                var residentDetail = residentContact.Resident;

                var databaseRecord = new DbResident
                {
                    FirstName = residentDetail.Firstname,
                    LastName = residentDetail.Lastname,
                    DateOfBirth = residentDetail.DateOfBirth,
                    Gender = residentDetail.Gender
                };
                residentsToAdd.Add(databaseRecord);
                return new ResidentDetails
                {
                    DatabaseResident = databaseRecord,
                    DomainResident = residentContact
                };
            }).ToList();

            _residentContactContext.Residents.AddRange(residentsToAdd);
            _residentContactContext.SaveChanges();

            var contactDetailRecords = new List<ContactDetail>();
            residents
                .ForEach(resident =>
                    contactDetailRecords.AddRange(LoadContactDetailsAndTypesDataForResident(resident.DomainResident,
                        resident.DatabaseResident.Id)));

            var externalIdRecords = new List<ExternalSystemRecord>();
            residents
                .ForEach(resident =>
                    externalIdRecords.AddRange(LoadExternalAndSourceSystemsDataForResident(resident.DomainResident, resident.DatabaseResident.Id)));

            _residentContactContext.ContactDetails.AddRange(contactDetailRecords);
            _residentContactContext.ExternalSystemRecords.AddRange(externalIdRecords);
            _residentContactContext.SaveChanges();
        }

        private void CheckContactLookUpTablesArePresent()
        {
            new List<string> { "Telephone", "Email", "Mobile" }.ForEach(AddContactTypeIfAbsent);
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
            new List<string> { "UH", "CRM" }.ForEach(AddSourceSystemTypeIfAbsent);
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

        private List<ContactDetail> LoadContactDetailsAndTypesDataForResident(ResidentContact residentContact, int residentId)
        {
            var contactDetailsForResident = residentContact.CommunicationDetails;
            var contactRecordsToSave = new List<ContactDetail>();

            contactRecordsToSave.AddRange(residentContact.CommunicationDetails.telephone.Where(t => t != null)
                .Select(t => new ContactDetail
                {
                    ContactTypeLookupId = _telephoneId,
                    ContactValue = t,
                    IsDefault = string.Equals(t, contactDetailsForResident.Default?.telephone),
                    IsActive = string.Equals(t, contactDetailsForResident.Default?.telephone),
                    DateAdded = DateTime.UtcNow,
                    //Making this the same as assumption is this would be run on an empty database
                    DateLastModified = DateTime.UtcNow,
                    ResidentId = residentId
                }));

            contactRecordsToSave.AddRange(residentContact.CommunicationDetails.mobile.Where(m => m != null)
                .Select(m => new ContactDetail
                {
                    ContactTypeLookupId = _mobileId,
                    ContactValue = m,
                    IsDefault = m.Equals(contactDetailsForResident.Default?.mobile),
                    IsActive = m.Equals(contactDetailsForResident.Default?.mobile),
                    DateAdded = DateTime.UtcNow,
                    //Making this the same as assumption is this would be run on an empty database
                    DateLastModified = DateTime.UtcNow,
                    ResidentId = residentId
                }));

            contactRecordsToSave.AddRange(residentContact.CommunicationDetails.email.Where(e => e != null)
                .Select(e => new ContactDetail
                {
                    ContactTypeLookupId = _emailId,
                    ContactValue = e,
                    IsDefault = e.Equals(contactDetailsForResident.Default?.email),
                    IsActive = e.Equals(contactDetailsForResident.Default?.email),
                    DateAdded = DateTime.UtcNow,
                    //Making this the same as assumption is this would be run on an empty database
                    DateLastModified = DateTime.UtcNow,
                    ResidentId = residentId
                }));

            return contactRecordsToSave;
        }

        private List<ExternalSystemRecord> LoadExternalAndSourceSystemsDataForResident(ResidentContact residentContact, int residentId)
        {
            var externalDetails = residentContact.DetailsFromExternalRecords;
            var systemRecordsToAdd = new List<ExternalSystemRecord>();

            if (externalDetails.HouseRef != null)
            {
                systemRecordsToAdd.Add(new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = _uhExternalSystemLookUpId,
                    Name = "HouseRef",
                    Value = externalDetails.HouseRef
                });
            }

            if (externalDetails.PersonNo != null)
            {
                systemRecordsToAdd.Add(new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = _uhExternalSystemLookUpId,
                    Name = "PersonNo",
                    Value = externalDetails.PersonNo
                });
            }

            if (externalDetails.ContactId != null)
            {
                systemRecordsToAdd.Add(new ExternalSystemRecord
                {
                    ResidentId = residentId,
                    ExternalSystemLookupId = _crmExternalSystemLookUpId,
                    Name = "ContactId",
                    Value = externalDetails.ContactId
                });
            }
            return systemRecordsToAdd;
        }
        private void GetLookupIds()
        {
            _telephoneId = _residentContactContext.ContactTypeLookups
                .First(x => x.Name.Equals("Telephone")).Id;
            _mobileId = _residentContactContext.ContactTypeLookups
                .First(x => x.Name.Equals("Mobile")).Id;
            _emailId = _residentContactContext.ContactTypeLookups
                .First(x => x.Name.Equals("Email")).Id;
            _uhExternalSystemLookUpId =
                _residentContactContext.ExternalSystemLookups.First(x => x.Name.Equals("UH")).Id;
            _crmExternalSystemLookUpId =
                _residentContactContext.ExternalSystemLookups.First(x => x.Name.Equals("CRM")).Id;
        }
    }

    public class ResidentDetails
    {
        public ResidentContact DomainResident { get; set; }

        public DbResident DatabaseResident { get; set; }
    }
}
