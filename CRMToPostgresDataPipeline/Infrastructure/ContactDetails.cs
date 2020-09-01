using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMToPostgresDataPipeline.Infrastructure
{
    [Table("contact_details")]
    public class ContactDetail
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("contact_type_lookup_id")]
        public int ContactTypeLookupId { get; set; }

        [ForeignKey("ContactTypeLookupId")]
        public ContactTypeLookup ContactTypeLookup { get; set; }

        [Column("contact_subtype_lookup_id")]
        public int? ContactSubTypeLookupId { get; set; }

        [ForeignKey("ContactSubTypeLookupId")]
        public ContactSubTypeLookup ContactSubTypeLookup { get; set; }

        [Column("contact_details_value")]
        public string ContactValue { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("added_by")]
        public string AddedBy { get; set; }

        [Column("date_added")]
        public DateTime DateAdded { get; set; }

        [Column("date_modified")]
        public DateTime DateLastModified { get; set; }

        [Column("modified_by")]
        public string ModifiedBy { get; set; }

        [Column("resident_id")]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident Resident { get; set; }
    }
}