using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SurveyApp.Models
{
    /// <summary>
    /// Represents a specification definition for an item from ItemSpecificationMaster table
    /// </summary>
    public class ItemSpecificationModel
    {
        /// <summary>
        /// The item ID this specification belongs to
        /// </summary>
        [JsonPropertyName("itemId")]
        public int ItemId { get; set; }

        /// <summary>
        /// Unique identifier for the specification
        /// </summary>
        [JsonPropertyName("specificationID")]
        public int SpecificationID { get; set; }

        /// <summary>
        /// Display name/label for the specification (e.g., "Road Width", "Pole Owner", "Height")
        /// </summary>
        [Display(Name = "Specification")]
        [JsonPropertyName("specificationName")]
        public string SpecificationName { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Input type hint (text, number, dropdown, etc.)
        /// Can be extended in the future
        /// </summary>
        [JsonPropertyName("inputType")]
        public string? InputType { get; set; }

        /// <summary>
        /// Optional: Dropdown options if InputType is dropdown (comma-separated)
        /// </summary>
        [JsonPropertyName("options")]
        public string? Options { get; set; }

        /// <summary>
        /// List of dropdown options fetched from ItemSpecificationOptionsMaster table
        /// Used when InputType is 'dropdown'
        /// </summary>
        [JsonPropertyName("optionsList")]
        public List<SpecificationOptionModel>? OptionsList { get; set; }

        /// <summary>
        /// Conditional display rule: 'Always', 'ExistingQtyOnly', 'RequiredQtyOnly', 'BothQty'
        /// </summary>
        [JsonPropertyName("conditionalDisplay")]
        public string? ConditionalDisplay { get; set; }

        /// <summary>
        /// Whether this specification allows multiple instances (e.g., multiple poles with different heights)
        /// </summary>
        [JsonPropertyName("allowMultipleInstances")]
        public bool AllowMultipleInstances { get; set; }
    }

    /// <summary>
    /// Represents a filled-in specification value from SpecificationDetailsMaster table
    /// </summary>
    public class SpecificationDetailsModel
    {
        /// <summary>
        /// Auto-generated ID (if table has identity column)
        /// </summary>
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        /// <summary>
        /// The survey this specification detail belongs to
        /// </summary>
        [JsonPropertyName("surveyID")]
        public long SurveyID { get; set; }

        /// <summary>
        /// The location within the survey
        /// </summary>
        [JsonPropertyName("locID")]
        public int LocID { get; set; }

        /// <summary>
        /// The item this specification detail is for
        /// </summary>
        [JsonPropertyName("itemID")]
        public int ItemID { get; set; }

        /// <summary>
        /// Reference to the specification definition
        /// </summary>
        [JsonPropertyName("specificationID")]
        public int SpecificationID { get; set; }

        /// <summary>
        /// The actual value entered by the user
        /// </summary>
        [Display(Name = "Value")]
        [JsonPropertyName("specificationDetails")]
        public string? SpecificationDetails { get; set; }

        /// <summary>
        /// Navigation property: the specification name (for display purposes)
        /// </summary>
        [JsonPropertyName("specificationName")]
        public string? SpecificationName { get; set; }

        /// <summary>
        /// Instance number for multiple instances of the same specification (e.g., pole 1, pole 2)
        /// </summary>
        [JsonPropertyName("instanceNumber")]
        public int InstanceNumber { get; set; } = 1;
    }

    /// <summary>
    /// Represents a dropdown option for a specification from ItemSpecificationOptionsMaster table
    /// </summary>
    public class SpecificationOptionModel
    {
        [JsonPropertyName("optionID")]
        public int OptionID { get; set; }

        [JsonPropertyName("specificationID")]
        public int SpecificationID { get; set; }

        [JsonPropertyName("optionValue")]
        public string OptionValue { get; set; } = string.Empty;

        [JsonPropertyName("optionText")]
        public string OptionText { get; set; } = string.Empty;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for submitting multiple specification details at once
    /// </summary>
    public class SpecificationDetailsSubmitModel
    {
        [JsonPropertyName("surveyID")]
        public long SurveyID { get; set; }
        
        [JsonPropertyName("locID")]
        public int LocID { get; set; }
        
        [JsonPropertyName("itemID")]
        public int ItemID { get; set; }
        
        [JsonPropertyName("specifications")]
        public List<SpecificationDetailItem> Specifications { get; set; } = new();
    }

    /// <summary>
    /// Individual specification detail item for submission
    /// </summary>
    public class SpecificationDetailItem
    {
        [JsonPropertyName("specificationID")]
        public int SpecificationID { get; set; }
        
        [JsonPropertyName("specificationDetails")]
        public string? SpecificationDetails { get; set; }
        
        [JsonPropertyName("instanceNumber")]
        public int InstanceNumber { get; set; } = 1;
    }
}
