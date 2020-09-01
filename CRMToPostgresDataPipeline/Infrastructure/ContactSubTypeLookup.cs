using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMToPostgresDataPipeline.Infrastructure
{
    [Table("contact_sub_type_lookup")]
    public class ContactSubTypeLookup
    {
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}