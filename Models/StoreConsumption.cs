using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AnalyticaDocs.Models
{
    public class StockDailyConsumption
    {
        
        public int Srno { get; set; }
        public int TransId { get; set; }

        public int? ConsId { get; set; }

        public DateOnly? ProductionDate { get; set; }

        public int? ItemCode { get; set; }

        public string? ItemUOM { get; set; }
        public string? ItemName { get; set; }

        public string? ItemType { get; set; } 

        public decimal? OpeningQty { get; set; }

        public decimal? InwordQty { get; set; }

        public decimal? OutwordQty { get; set; }

        public decimal? ClosingQty { get; set; }

        public decimal? FinesQty { get; set; }

        public decimal? FinesMTD { get; set; }

        public decimal? F1Cons { get; set; }

        public decimal? F2Cons { get; set; }

        public decimal? TotalCons { get; set; }

        public decimal? Diff { get; set; }

        public int? CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        public string? Remarks { get; set; }
    }

    public class StockInward
    {
        public int TransId { get; set; }

        [Required]
        public DateOnly InwordDate { get; set; }

        [Required]
        public int? ItemCode { get; set; }
        
        public string? ItemUom { get; set; }

        [Required]
        public string? ItemType { get; set; }

        [Required,Range(0,999999)]
        public decimal? InwardQty { get; set; }

        public int? CreateBy { get; set; }

        public string? ItemName { get; set; }

        public bool isUpdate { get; set; } = false;

        public List<SelectListItem> TypeOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "-- Select --", Value = "", Selected = true},
            new SelectListItem { Text = "Raw Material", Value = "Raw Material" /*, Selected = true*/ },
            new SelectListItem { Text = "Consumable", Value = "Consumable" },
           
        };
    }

    public class StockDailyConsumptionReport
    {
        

        [Required]
        public DateOnly? ProductionDate { get; set; }

        public int? ConsId { get; set; }
        public int? CreateBy { get; set; }

        public DateTime? CreateOn { get; set; }

        public int ReportStatus { get; set; } = -1;

        // 1 Already Generated 0 Ready To Generate -1 Previous Report Required
    }
}
