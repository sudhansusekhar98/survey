using AnalyticaDocs.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace AnalyticaDocs.pdfHelper
{
 
    public class ReportDocument : IDocument
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

                page.Header().Element(BuildHeader);
                page.Content().Element(BuildContent);
                page.Footer().Element(BuildFooter);
            });
        }

        void BuildHeader(IContainer container)
        {
            container.ShowOnce().Height(50).Row(row =>
            {
                // Left: Logo
                row.ConstantItem(60) // adjust width for logo area
                    .Image("wwwroot/img/logo/reportlogo.png")
                    .FitHeight();

                // Right: Title + Subtitle stacked vertically
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignLeft().Text(Title)
                        .FontSize(20).Bold().FontColor(Colors.Purple.Darken1);

                    col.Item().AlignLeft().Text(Subtitle).Italic()
                        .FontSize(16).SemiBold().FontColor(Colors.Grey.Lighten1);


                });


                
            });
        }


        void BuildContent(IContainer container)
        {
            container.PaddingVertical(10).Column(col =>
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
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                    });

                    // Header row
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

                    // Header cell style (bold + grid)
                    static IContainer CellHeaderStyle(IContainer container) =>
                        container
                            .Background(Colors.Grey.Lighten3) // background after padding
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(5)
                            .PaddingHorizontal(3)
                            .DefaultTextStyle(x => x.SemiBold());

                    // Body cell style (grid only)
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
            });
        }
    }
}
