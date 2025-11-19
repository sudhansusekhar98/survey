using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Models
{
    [Table("ItemTypeMaster")]
    public class ItemTypeMasterModel
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Type Name")]
        [Column("TypeName")]
        public string TypeName { get; set; } = string.Empty;

        [MaxLength(300)]
        [Display(Name = "Type Description")]
        [Column("TypeDesc")]
        public string TypeDesc { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Group Name")]
        [Column("GroupName")]
        public string GroupName { get; set; } = string.Empty;

        [Display(Name = "Active")]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created On")]
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Display(Name = "Created By")]
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Display(Name = "Modified Date")]
        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Modified By")]
        [Column("ModifiedBy")]
        public int? ModifiedBy { get; set; }

        // For tracking assignment status in UI
        [NotMapped]
        public bool IsAssigned { get; set; } = false;

        // Navigation property
        public virtual ICollection<ItemMasterModel> Items { get; set; } = new List<ItemMasterModel>();
    }
}
