using System.Reflection;

namespace AnalyticaDocs.Models
{
    public class PowerChartViewModel
    {

        public string ShiftDay { get; set; } = string.Empty;
        public int ConsTarget { get; set; }
        public decimal APEX { get; set; }
        public decimal Incomer_HT { get; set; }
        public decimal Furnace_All { get; set; }
        public decimal Auxilary { get; set; }
        public decimal FurnaceI { get; set; }
        public decimal FurnaceII { get; set; }
        public decimal Incomer_LT { get; set; }
        public decimal GCP { get; set; }
        public decimal PUMP { get; set; }
        public decimal PDBII { get; set; }
        public decimal RMHS { get; set; }
        public decimal HYD { get; set; }
        public decimal PDBI { get; set; }
        public decimal Compressor { get; set; }

    }

    public class PowerChartViewModelList : List<PowerChartViewModel>
    {
        public List<string> Labels => this.Select(x => x.ShiftDay).ToList();
        public List<int> Target => this.Select(x => x.ConsTarget).ToList();
        public List<decimal> APEX => this.Select(x => x.APEX).ToList();
        public List<decimal> Incomer_HT => this.Select(x => x.Incomer_HT).ToList();
        public List<decimal> Furnace_All => this.Select(x => x.Furnace_All).ToList();
        public List<decimal> Auxilary => this.Select(x => x.Auxilary).ToList();
        public List<decimal> FurnaceI => this.Select(x => x.FurnaceI).ToList();
        public List<decimal> FurnaceII => this.Select(x => x.FurnaceII).ToList();
        public List<decimal> Incomer_LT => this.Select(x => x.Incomer_LT).ToList();
        public List<decimal> GCP => this.Select(x => x.GCP).ToList();
        public List<decimal> PUMP => this.Select(x => x.PUMP).ToList();
        public List<decimal> PDBII => this.Select(x => x.PDBII).ToList();
        public List<decimal> RMHS => this.Select(x => x.RMHS).ToList();
        public List<decimal> HYD => this.Select(x => x.HYD).ToList();
        public List<decimal> PDBI => this.Select(x => x.PDBI).ToList();
        public List<decimal> Compressor => this.Select(x => x.Compressor).ToList();



    }

    public class RMChartViewModel
    {

        public string ShiftDay { get; set; } = string.Empty;
        public int ConsTarget { get; set; }
        public int DayID { get; set; }
        public decimal Quartz { get; set; }
        public decimal Charcoal { get; set; }
        public decimal MillScale { get; set; }
        public decimal LamCoke { get; set; }
        public decimal WoodChip { get; set; }

    }


    
    public class RMChartViewModelList : List<RMChartViewModel>
    {
        public List<string> Labels => this.Select(x => x.ShiftDay).ToList();
        public List<int> ConsTarget => this.Select(x => x.ConsTarget).ToList();
        public List<int> DayID => this.Select(x => x.DayID).ToList();
        public List<decimal> Quartz => this.Select(x => x.Quartz).ToList();
        public List<decimal> Charcoal => this.Select(x => x.Charcoal).ToList();
        public List<decimal> MillScale => this.Select(x => x.MillScale).ToList();
        public List<decimal> LamCoke => this.Select(x => x.LamCoke).ToList();
        public List<decimal> WoodChip => this.Select(x => x.WoodChip).ToList();

    }


    public class PowerChartAuxModel
    {
        public string PowerConLabels { get; set; } = string.Empty;
        public decimal PowerConData { get; set; }
    }
    public class PowerChartAuxiList : List<PowerChartAuxModel>
    {
        public List<string> PowerAuxLabels => this.Select(x => x.PowerConLabels).ToList();
        public List<decimal> PowerAuxData => this.Select(x => x.PowerConData).ToList();
    }

    public class InventoryChartViewModel
    {

        public string ItemName { get; set; } = string.Empty;
        public int ItemCode { get; set; }
        public decimal InStock { get; set; }
        public decimal InQC { get; set; }
        public decimal InWIP { get; set; }
        public decimal InTransit { get; set; }
        public decimal SPQty { get; set; }
        public int LeftDays { get; set; }

    }
    public class InventoryChartViewModelList : List<InventoryChartViewModel>
    {
        public List<string> Labels => this.Select(x => x.ItemName).ToList();
        public List<int> ItemCode => this.Select(x => x.ItemCode).ToList();
        public List<int> LeftDays => this.Select(x => x.LeftDays).ToList();
        public List<decimal> InStock => this.Select(x => x.InStock).ToList();
        public List<decimal> InQC => this.Select(x => x.InQC).ToList();
        public List<decimal> InWIP => this.Select(x => x.InWIP).ToList();
        public List<decimal> InTransit => this.Select(x => x.InTransit).ToList();
        public List<decimal> SPQty => this.Select(x => x.SPQty).ToList();

    }


    public class TextChartViewModel
    {
        public List<string> Labels { get; set; } 
        public List<double> Values { get; set; }
        public string ChartTitle { get; set; }
        public string ChartId { get; set; } // e.g., "powerDetailChart"
    }

    

}
