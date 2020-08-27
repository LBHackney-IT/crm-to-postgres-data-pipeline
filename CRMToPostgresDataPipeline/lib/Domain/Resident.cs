using System;

namespace CRMToPostgresDataPipeline.lib.Domain
{
    public class Resident
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public char? Gender { get; set; }
    }
}