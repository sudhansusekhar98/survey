using AnalyticaDocs.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Models
{
    public class SurveyDetailsModel
    {

        public int ItemID { get; set; }
        public int ItemQtyExist { get; set; }
        public int ItemQtyReq { get; set; }
        public string ImgPath { get; set; } = string.Empty;
        public string ImgID { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string ItemDesc { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;



    }

    public class SurveyDetailsLocationModel
    {
        public Int64 SurveyID { get; set; }
        public int LocID { get; set; }
        public int ItemTypeID { get; set; }
        public string LocName { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string TypeDesc { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }

        public List<SurveyDetailsModel> ItemLists { get; set; } = new List<SurveyDetailsModel>();
    }

    /// <summary>
    /// Model for cable items from ItemMaster (TypeId = Cable category)
    /// </summary>
    public class CableItemModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemUOM { get; set; } = string.Empty;
        public string ItemDesc { get; set; } = string.Empty;
    }

     /// Model for survey-level cable counts (stored in SurveyDetails with LocID = 0)
     public class SurveyCableCountModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemUOM { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

     /// Request model for saving cable counts from JSON body
     public class SaveCableCountsRequest
    {
        public long SurveyId { get; set; }
        public List<CableCountItem>? CableCounts { get; set; }
    }

     /// Individual cable count item in the save request
     public class CableCountItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string? Remarks { get; set; }
    }
}