using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace AnalyticaDocs.Models
{
    public class MeterConsumptionModel
    {
        public int Srno { get; set; }
        public int TransId { get; set; }

        public int? MeterId { get; set; }

        public string? MeterName { get; set; }

        public string? Capacity { get; set; }

        [Required]
        public decimal? PrvRead { get; set; }

        [Range(1.000, 99999999.000),Required]
        public decimal? CurRead { get; set; }

        [Required]
        public decimal? MF { get; set; }

        public decimal? MeterMF { get; set; }

        [Range(0.000, 600.000),Required]
        public decimal? Consumption { get; set; }

       
    }

    public class MeterConsumptionFromModel
    {
        public DateOnly? ReadDate { get; set; }
        public TimeOnly? ReadTime { get; set; }
        public DateOnly? ShiftDate { get; set; }

        public decimal? TotalConsumption { get; set; }

        [Required,Range(0.20,1.0)]
        public decimal? PF { get; set; }
        public int CreateBy { get; set; }
        public List<MeterConsumptionModel> ConsumptionList { get; set; } = new List<MeterConsumptionModel>();
    }

    public class RequiredAndRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public RequiredAndRangeAttribute(double min, double max)
        {
            _min = (decimal)min;
            _max = (decimal)max;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value == null)
                return new ValidationResult("PF is required.");

            var val = (decimal)value;
            if (val < _min || val > _max)
                return new ValidationResult($"PF must be between {_min} and {_max}.");

            return ValidationResult.Success;
        }
    }




}
