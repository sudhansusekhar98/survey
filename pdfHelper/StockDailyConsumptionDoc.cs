using AnalyticaDocs.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace AnalyticaDocs.pdfHelper
{
    public class StockDailyConsumptionDoc : IDocument
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<StockDailyConsumption> TableData { get; set; }

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

                //// Row 2: two images side-by-side, scale by width (no fixed height)
                //col.Item().PaddingTop(6).Border(1)
                //.BorderColor(Colors.Grey.Lighten1).Row(row =>
                //{
                //    row.RelativeItem(3).AlignMiddle()
                //        .Image("wwwroot/img/sample1.png").FitWidth();

                //    row.RelativeItem(1).AlignMiddle()
                //        .Image("wwwroot/img/sample2.png").FitWidth();
                //});
            });
        }

        void BuildContent(IContainer container)
        {
            container.PaddingVertical(4).Column(col =>
            {
                col.Item().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // #
                        columns.RelativeColumn(3); // Date
                        columns.RelativeColumn(2); // Code
                        columns.RelativeColumn(6); // Name
                        columns.RelativeColumn(2); // UOM
                        columns.RelativeColumn(3); // Opening
                        columns.RelativeColumn(3); // Inward
                        columns.RelativeColumn(3); // Outward
                        columns.RelativeColumn(3); // Closing
                        columns.RelativeColumn(3); // Fines
                        columns.RelativeColumn(3); // Fines-MTD
                    });

                    // Table header (QuestPDF repeats this across pages automatically)
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("#").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Date").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Code").AlignCenter();
                        //header.Cell().Element(CellHeaderStyle).Text("Item Type").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Item Name").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("UOM").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Opening").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Inward").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Outward").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Closing").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Fines").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Fines-MTD").AlignCenter();
                    });

                    // Data rows
                    foreach (var row in TableData)
                    {
                        table.Cell().Element(CellBodyStyle).Text(row.Srno.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ProductionDate.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ItemCode.ToString()).AlignCenter();
                        //table.Cell().Element(CellBodyStyle).Text(row.ItemType.ToString()).AlignLeft();
                        table.Cell().Element(CellBodyStyle).Text(row.ItemName.ToString()).AlignLeft();
                        table.Cell().Element(CellBodyStyle).Text(row.ItemUOM.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.OpeningQty.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.InwordQty.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.OutwordQty.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.ClosingQty.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.FinesQty.ToString()).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(row.FinesMTD.ToString()).AlignRight();
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
                            .PaddingHorizontal(5);
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
