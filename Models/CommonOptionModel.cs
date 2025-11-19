namespace AnalyticaDocs.Models
{
    public class DepartmentList
    {
        public int? DeptId { get; set; }
        public string? DeptName { get; set; }
    }

    public class UsersList
    {
        public int? UserID { get; set; }
        public string? LoginName { get; set; }

        public string? NameDesc { get; set; }
    }

    public class ItemMaster
    {
        public int ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? ItemDesc { get; set; }

        public string? ItemType { get; set; }

        public string? ItemUom { get; set; }

        public string? ProductionUom { get; set; }

        public int? ConsumptionSn { get; set; }

        public int? ConsumptionTypeId { get; set; }

        public int? GroupId { get; set; }

        public int? GroupSn { get; set; }

        public int? ChildId { get; set; }

    }

    public class CostPeriod
    {
        public int PeriodId { get; set; }

        public int? PeriodMonth { get; set; }

        public int? PeriodYear { get; set; }

        public DateOnly? PeriodFrom { get; set; }

        public DateOnly? PeriodTo { get; set; }

        public string? PeriodName { get; set; }

        public string? PeriodDesc { get; set; }

        public char? FixCostApproved { get; set; }

        public string? IsApproved { get; set; }
       
        public string? IsActive { get; set; }
    }

    public class ReportStatusModel
    {
        public int StatusStock { get; set; } = 0;
        public int StatusProduction { get; set; } = 0;
        public int StatusCostSheet { get; set; } = 0;

        public int PreviousStock { get; set; } = 0;
        public int PreviousProduction { get; set; } = 0;
        public int PreviousCostSheet { get; set; } = 0;

        public DateOnly MaxStock { get; set; } = DateOnly.MinValue;
        public DateOnly MaxProduction { get; set; } = DateOnly.MinValue;
        public DateOnly MaxCostSheet { get; set; } = DateOnly.MinValue;


    }
}
