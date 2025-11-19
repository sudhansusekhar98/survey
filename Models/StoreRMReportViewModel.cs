using System.ComponentModel.DataAnnotations;

namespace AnalyticaDocs.Models
{
    public class StoreRMReportViewModel
    {
        [Display(Name = "Srno")]
        public int Srno { get; set; }

        [Display(Name = "ProductionDate")]
        public DateTime ProductionDate { get; set; }

        // Quartz
        public decimal QuartzInwardQty { get; set; }
        public decimal QuartzOutwardQty { get; set; }
        public decimal QuartzFinesQty { get; set; }
        public decimal QuartzClosingQty { get; set; }

        // BCharcoal
        public decimal BCharcoalInwardQty { get; set; }
        public decimal BCharcoalOutwardQty { get; set; }
        public decimal BCharcoalFinesQty { get; set; }
        public decimal BCharcoalClosingQty { get; set; }

        // TNCharcoal
        public decimal TNCharcoalInwardQty { get; set; }
        public decimal TNCharcoalOutwardQty { get; set; }
        public decimal TNCharcoalFinesQty { get; set; }
        public decimal TNCharcoalClosingQty { get; set; }

        // NCharcoal
        public decimal NCharcoalInwardQty { get; set; }
        public decimal NCharcoalOutwardQty { get; set; }
        public decimal NCharcoalFinesQty { get; set; }
        public decimal NCharcoalClosingQty { get; set; }

        // RCharcoal
        public decimal RCharcoalInwardQty { get; set; }
        public decimal RCharcoalOutwardQty { get; set; }
        public decimal RCharcoalFinesQty { get; set; }
        public decimal RCharcoalClosingQty { get; set; }

        // MillScale
        public decimal MillScaleInwardQty { get; set; }
        public decimal MillScaleOutwardQty { get; set; }
        public decimal MillScaleFinesQty { get; set; }
        public decimal MillScaleClosingQty { get; set; }

        // LamCoke
        public decimal LamCokeInwardQty { get; set; }
        public decimal LamCokeOutwardQty { get; set; }
        public decimal LamCokeFinesQty { get; set; }
        public decimal LamCokeClosingQty { get; set; }

        // SemiCoke1040MM
        public decimal SemiCoke1040MMInwardQty { get; set; }
        public decimal SemiCoke1040MMOutwardQty { get; set; }
        public decimal SemiCoke1040MMFinesQty { get; set; }
        public decimal SemiCoke1040MMClosingQty { get; set; }

        // ECPasteWE
        public decimal ECPasteWEInwardQty { get; set; }
        public decimal ECPasteWEOutwardQty { get; set; }
        public decimal ECPasteWEFinesQty { get; set; }
        public decimal ECPasteWEClosingQty { get; set; }

        // ECPasteCRP
        public decimal ECPasteCRPInwardQty { get; set; }
        public decimal ECPasteCRPOutwardQty { get; set; }
        public decimal ECPasteCRPFinesQty { get; set; }
        public decimal ECPasteCRPClosingQty { get; set; }

        // MSBar25MM
        public decimal MSBar25MMInwardQty { get; set; }
        public decimal MSBar25MMOutwardQty { get; set; }
        public decimal MSBar25MMFinesQty { get; set; }
        public decimal MSBar25MMClosingQty { get; set; }

        // MSBar20MM
        public decimal MSBar20MMInwardQty { get; set; }
        public decimal MSBar20MMOutwardQty { get; set; }
        public decimal MSBar20MMFinesQty { get; set; }
        public decimal MSBar20MMClosingQty { get; set; }

        // LancingPipe
        public decimal LancingPipeInwardQty { get; set; }
        public decimal LancingPipeOutwardQty { get; set; }
        public decimal LancingPipeFinesQty { get; set; }
        public decimal LancingPipeClosingQty { get; set; }

        // TampingPaste
        public decimal TampingPasteInwardQty { get; set; }
        public decimal TampingPasteOutwardQty { get; set; }
        public decimal TampingPasteFinesQty { get; set; }
        public decimal TampingPasteClosingQty { get; set; }

        // SodiumSilicate
        public decimal SodiumSilicateInwardQty { get; set; }
        public decimal SodiumSilicateOutwardQty { get; set; }
        public decimal SodiumSilicateFinesQty { get; set; }
        public decimal SodiumSilicateClosingQty { get; set; }

        // Scrap
        public decimal ScrapInwardQty { get; set; }
        public decimal ScrapOutwardQty { get; set; }
        public decimal ScrapFinesQty { get; set; }
        public decimal ScrapClosingQty { get; set; }

        // CasingSheet
        public decimal CasingSheetInwardQty { get; set; }
        public decimal CasingSheetOutwardQty { get; set; }
        public decimal CasingSheetFinesQty { get; set; }
        public decimal CasingSheetClosingQty { get; set; }

        // WoodChip
        public decimal WoodChipInwardQty { get; set; }
        public decimal WoodChipOutwardQty { get; set; }
        public decimal WoodChipFinesQty { get; set; }
        public decimal WoodChipClosingQty { get; set; }

        // Oxygen
        public decimal OxygenInwardQty { get; set; }
        public decimal OxygenOutwardQty { get; set; }
        public decimal OxygenFinesQty { get; set; }
        public decimal OxygenClosingQty { get; set; }

        // SilicaFines
        public decimal SilicaFinesInwardQty { get; set; }
        public decimal SilicaFinesOutwardQty { get; set; }
        public decimal SilicaFinesFinesQty { get; set; }
        public decimal SilicaFinesClosingQty { get; set; }

        // QuartzWashingDust
        public decimal QuartzWashingDustInwardQty { get; set; }
        public decimal QuartzWashingDustOutwardQty { get; set; }
        public decimal QuartzWashingDustFinesQty { get; set; }
        public decimal QuartzWashingDustClosingQty { get; set; }

        // LimeStone
        public decimal LimeStoneInwardQty { get; set; }
        public decimal LimeStoneOutwardQty { get; set; }
        public decimal LimeStoneFinesQty { get; set; }
        public decimal LimeStoneClosingQty { get; set; }

        // RiverSand
        public decimal RiverSandInwardQty { get; set; }
        public decimal RiverSandOutwardQty { get; set; }
        public decimal RiverSandFinesQty { get; set; }
        public decimal RiverSandClosingQty { get; set; }

        // Helper: Return true if this instance represents the Total row
        public bool IsTotalRow => Srno > 0 && ProductionDate == DateTime.MinValue;
    }
}
