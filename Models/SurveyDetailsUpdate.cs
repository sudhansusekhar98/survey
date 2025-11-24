namespace SurveyApp.Models
{
    public class SurveyDetailsUpdate
    {
        public Int64 SurveyID { get; set; }
        public int LocID { get; set; }
        public int ItemTypeID { get; set; }
        public string? TypeName { get; set; }

        public int CreateBy { get; set; }
        public List<SurveyDetailsUpdatelist> ItemLists { get; set; } = new List<SurveyDetailsUpdatelist>();
        
        // For Cloudinary image uploads
        public List<string>? ImageUrls { get; set; }
        public List<string>? ImagePublicIds { get; set; }
    }

    public class SurveyDetailsUpdatelist
    {
        public int ItemID { get; set; }
        public int ItemQtyExist { get; set; }
        public int ItemQtyReq { get; set; }

        public string? ImgPath { get; set; } = string.Empty;
        public string? ItemCode { get; set; } = string.Empty;
        public string? ImgID { get; set; } = string.Empty;

        public string? ItemName { get; set; } = string.Empty;

        public string? ItemDesc { get; set; } = string.Empty;
        public string? Remarks { get; set; } = string.Empty;
        
        // For Cloudinary - support multiple images per item
        public List<string>? CloudinaryUrls { get; set; }
        public List<string>? CloudinaryPublicIds { get; set; }
    }
}
