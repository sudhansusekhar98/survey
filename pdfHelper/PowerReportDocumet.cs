using AnalyticaDocs.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace AnalyticaDocs.pdfHelper
{
    public class PowerReportDocumet : IDocument
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<MeterConsumptionViewModel> TableData { get; set; }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(0.8f, Unit.Centimetre);

                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Helvetica"));

                // --- Put header as first content item (will render only once)
                page.Content().Column(col =>
                {
                    col.Item().Element(BuildHeader);   // header shown once as normal content
                    col.Item().Element(BuildContent);  // table starts immediately after header
                });

                // footer repeats on each page
                page.Footer().Element(BuildFooter);
            });
        }

        void BuildHeader(IContainer container)
        {
            // Avoid forcing heights; let QuestPDF calculate sizes.
            container.Column(col =>
            {
                // Row 1: Logo + Title + Subtitle
                col.Item().Row(row =>
                {
                    row.ConstantItem(60)
                        .Image("wwwroot/img/logo/reportlogo.png")
                        .FitWidth();       // scale by width to avoid height conflicts

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignLeft().Text(Title)
                            .FontSize(20).Bold().FontColor(Colors.Purple.Darken1);

                        c.Item().AlignLeft().Text(Subtitle)
                            .Italic().FontSize(16).SemiBold().FontColor(Colors.Grey.Lighten1);
                    });
                });

                // Row 2: two images side-by-side, scale by width (no fixed height)
                col.Item().PaddingTop(6).Border(1)
                .BorderColor(Colors.Grey.Lighten1).Row(row =>
                {
                    row.RelativeItem(3).AlignMiddle()
                        .Image("wwwroot/img/sample1.png").FitWidth();

                    row.RelativeItem(1).AlignMiddle()
                        .Image("wwwroot/img/sample2.png").FitWidth();
                });
            });
        }

        void BuildContent(IContainer container)
        {
            container.PaddingVertical(0).Column(col =>
            {
                col.Item().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // #
                        columns.RelativeColumn(4); // Date
                        columns.RelativeColumn(4); // APEX
                        columns.RelativeColumn(4); // FURNACE1
                        columns.RelativeColumn(4); // FURNACE2
                        columns.RelativeColumn(3); // GCP
                        columns.RelativeColumn(4); // PUMP HOUSE
                        columns.RelativeColumn(3); // RMHS
                        columns.RelativeColumn(3); // PDB-2
                        columns.RelativeColumn(4); // PDB-1&3
                    });

                    // Table header (QuestPDF repeats this across pages automatically)
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("#").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Date").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("APEX").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("FURNACE1").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("FURNACE2").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("GCP").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("PUMP HOUSE").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("RMHS").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("PDB-2").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("PDB-1&3").AlignCenter();
                    });

                    // Data rows
                    foreach (var row in TableData)
                    {
                        table.Cell().Element(CellBodyStyle).Text(row.Srno.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ShiftDate.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption100.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption104.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption105.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption107.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption108.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption110.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption109.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.Consumption112.ToString()).AlignRight();
                    }

                    // Header cell style
                    static IContainer CellHeaderStyle(IContainer container) =>
                        container
                            .Background(Colors.Grey.Lighten3)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(5)
                            .PaddingHorizontal(3)
                            .DefaultTextStyle(x => x.SemiBold());

                    // Body cell style
                    static IContainer CellBodyStyle(IContainer container) =>
                        container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(3)
                            .PaddingHorizontal(2);
                });
            });
        }

        void BuildFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        }
    }
}
