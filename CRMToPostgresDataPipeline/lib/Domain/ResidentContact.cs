namespace CRMToPostgresDataPipeline.lib.Domain
{
    public class ResidentContact
    {
        public Resident Resident { get; set; }
        public CommunicationDetails CommunicationDetails { get; set; }
        public DetailsFromExternalRecords DetailsFromExternalRecords { get; set; }
    }
}