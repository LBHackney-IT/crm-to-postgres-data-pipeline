using System;
using System.Collections.Generic;

namespace CRMToPostgresDataPipeline.lib.Domain
{
    public class CrmResponse
    {
        public List<RecordValue> value { get; set; }
    }

    public class RecordValue
    {
        public string hackney_communicationdetails { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime? birthdate { get; set; }
        public string hackney_gender { get; set; }
        public string hackney_houseref { get; set; }
        public string hackney_personno { get; set; }
        public string contactid { get; set; }
    }
}