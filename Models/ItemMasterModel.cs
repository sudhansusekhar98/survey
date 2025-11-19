using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Models
{
    [Table("ItemMaster")]
    // simple item model (you already have ItemMasterModel; keep for reference)
    public class ItemMasterModel
    {
        [Key]
        [Column("ItemId")]
        public int ItemId { get; set; }

        [Required]
        [Display(Name = "Type")]
        [Column("TypeId")]
        public int TypeId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Item Name")]
        [Column("ItemName")]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Item Code")]
        [Column("ItemCode")]
        public string ItemCode { get; set; } = string.Empty;

        [MaxLength(300)]
        [Display(Name = "Item Description")]
        [Column("ItemDesc")]
        public string ItemDesc { get; set; } = string.Empty;

        [Display(Name = "Active")]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created On")]
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Display(Name = "Created By")]
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Display(Name = "Sequence Number")]
        [Column("SqNo")]
        public int SqNo { get; set; }

        // Navigation property
        [ForeignKey("TypeId")]
        public virtual ItemTypeMasterModel? ItemType { get; set; }
    }

    // Cantilever item used inside pole instances
    public class CantileverItem
    {
        public string Name { get; set; } = string.Empty;

        // used in the view for binding the input quantity
        public int Quantity { get; set; }

        // view uses a Length string as well
        public string Length { get; set; } = string.Empty;
    }

    // A pole instance that the UI renders (each added pole)
    public class PoleInstance
    {
        public int InstanceId { get; set; } // unique per added pole (could be index)
        public int ItemId { get; set; }     // reference to ItemMasterModel.ItemId if needed
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CantileverItem> Cantilevers { get; set; } = new();
    }

    // Department file/remarks container used by view
    public class DepartmentSection
    {
        public List<string> Images { get; set; } = new();
        public string Remarks { get; set; } = string.Empty;
    }

    public class Gantry
    {
        public int Quantity { get; set; }
        public List<string> Images { get; set; } = new();
        public string Remarks { get; set; } = string.Empty;
    }

    // The viewmodel that your Razor expects (matches the fields used in the cshtml)
    public class PoleCantileverViewModel
    {
        // simple numeric counts shown in top inputs
        public int IronPoleCount { get; set; }
        public int ConcretePoleCount { get; set; }
        public int TelecomPoleCount { get; set; }

        // 🆕 Existing pole info section
        public List<string> ExistingPoleImages { get; set; } = new();
        public string ExistingPoleRemarks { get; set; } = string.Empty;

        // collections of poles created dynamically by the UI (Surveillance / ALPR / Traffic)
        public List<PoleInstance> SurveillancePoles { get; set; } = new();
        public List<PoleInstance> ALPRPoles { get; set; } = new();
        public List<PoleInstance> TrafficPoles { get; set; } = new();

        // department sections for images/remarks
        public DepartmentSection SurveillanceDept { get; set; } = new();
        public DepartmentSection ALPRDept { get; set; } = new();
        public DepartmentSection TrafficDept { get; set; } = new();

        public Gantry Gantry { get; set; } = new Gantry();

        // other page fields
        public string Distance { get; set; } = string.Empty;
        public string PermissionRequired { get; set; } = "no"; // "yes" or "no"

        // convenience properties
        public int PoleQuantity { get; set; } // (optional)
        public List<CantileverItem> Cantilevers { get; set; } = new(); // global defaults if needed
    }
}
