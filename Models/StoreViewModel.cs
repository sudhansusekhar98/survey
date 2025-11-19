using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AnalyticaDocs.Models
{
    public class StockFeedingViewModel
    {
        public int Srno { get; set; }

        public int FeedingID { get; set; }

        [Required]
        public DateOnly? FeedingDate { get; set; }

        [Required]
        public string? ShiftId { get; set; }

        public string? ShiftName { get; set; }

        [Required]
        public int ItemCode { get; set; }

        public string? ItemName { get; set; }

        [Required]
        public string? ItemType { get; set; }

        public string? ItemUOM { get; set; }

        [Required]
        public int? ReceiptNo { get; set; }

        [Required]
        public decimal? GrossWeight { get; set; }

        [Required]
        public decimal? TareWeight { get; set; }

        [Range(100,99999),Required]
        public decimal? NetWeight { get; set; }

        public char? ReportGenerate { get; set; }

        public List<SelectListItem> ShiftOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "-- Select --", Value = "", Selected = true},
            new SelectListItem { Text = "Shift-A", Value = "A" /*, Selected = true*/ },
            new SelectListItem { Text = "Shift-B", Value = "B" },
             new SelectListItem { Text = "Shift-C", Value = "C" }
        };

        public List<SelectListItem> TypeOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "-- Select --", Value = "", Selected = true},
            new SelectListItem { Text = "Raw Material", Value = "Raw Material" /*, Selected = true*/ },
            new SelectListItem { Text = "Fines", Value = "Fines" },
             new SelectListItem { Text = "Dust", Value = "Dust" }
        };

       

        public int CreateBy { get; set; }
    }
    

    public class StockFeedingSummaryViewModel
    {
        public int Srno { get; set; }
        public int? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public string? ItemType { get; set; }

        public string? ItemUOM { get; set; }

        public decimal? GrossWeight { get; set; }

        public decimal? TareWeight { get; set; }

        public decimal? NetWeight { get; set; }

       
    }

   

    public class GroundHopperViewModel
    {
        [Required]
        public DateOnly? FromDate { get; set; }

        [Required]
        public DateOnly? ToDate { get; set; }

        public List<StockFeedingSummaryViewModel> GroundHopperSummaryList { get; set; } = new List<StockFeedingSummaryViewModel>();

        public List<StockFeedingViewModel> GroundHopperDetailList { get; set; } = new List<StockFeedingViewModel>();

        public string? AuxChartBase64 { get; set; }

    }

    
}
