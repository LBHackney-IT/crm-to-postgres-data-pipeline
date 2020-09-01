using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMToPostgresDataPipeline.Infrastructure
{
    [Table("external_system_ids")]

    public class ExternalSystemRecord
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("resident_id")]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident Resident { get; set; }

        [Column("external_system_lookup_id")]
        public int ExternalSystemLookupId { get; set; }

        [ForeignKey("ExternalSystemLookupId")]
        public ExternalSystemLookup ExternalSystemLookup { get; set; }

        [Column("external_id_name")]
        public string Name { get; set; }

        [Column("external_id_value")]
        public string Value { get; set; }
    }
}