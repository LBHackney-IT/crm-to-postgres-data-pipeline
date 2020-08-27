using System.Collections.Generic;

namespace CRMToPostgresDataPipeline.lib.Domain
{
    public class CommunicationDetails
    {
        public List<string> telephone { get; set; }
        public List<string> mobile { get; set; }
        public List<string> email { get; set; }
        public DefaultContacts Default { get; set; }
    }

    public class DefaultContacts
    {
        public string telephone;
        public string mobile;
        public string email;
    }
}