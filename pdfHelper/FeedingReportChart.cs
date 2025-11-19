using AnalyticaDocs.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace AnalyticaDocs.pdfHelper
{
    public class FeedingReportChart : IDocument
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<StockFeedingSummaryViewModel> TableHead { get; set; }

        public List<StockFeedingViewModel> TableData { get; set; }
        // NEW: two charts
        //public byte[]? HeaderChartImage { get; set; }
        public byte[]? AuxChartImage { get; set; }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(0.8f, Unit.Centimetre);

                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));

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
            container.Column(col =>
            {
               
                // Row 1: Logo + Title + Subtitle
                col.Item().Row(row =>
                {
                    row.ConstantItem(60)
                       .Image("wwwroot/img/logo/reportlogo.png")
                       .FitWidth();

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignLeft().Text(Title)
                            .FontSize(20).Bold().FontColor(Colors.Purple.Darken1);

                        c.Item().AlignLeft().Text(Subtitle)
                            .Italic().FontSize(16).SemiBold().FontColor(Colors.Grey.Lighten1);
                    });
                });

                // Row 3: Feeding Summary
                col.Item().PaddingTop(5).PaddingBottom(1).Row(row =>
                {

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignLeft().Text("Summary : ")
                            .Italic().FontSize(14).SemiBold().FontColor(Colors.Grey.Lighten1);
                    });
                });

                // Row 2: left - summary table, right - aux image
                col.Item().PaddingTop(0).Border(0.2f).BorderColor(Colors.Grey.Lighten1)
                   .Row(row =>
                   {
                       // LEFT: Summary table (3x wider than aux image area)
                       row.RelativeItem(2)
                          .AlignTop()
                          .PaddingLeft(0).PaddingRight(0).PaddingBottom(0)
                          .Element(summaryContainer =>
                          {
                              if (TableHead == null || !TableHead.Any())
                              {
                                  summaryContainer.AlignCenter().Text("No summary data")
                                      .FontSize(10).FontColor(Colors.Grey.Darken1);
                                  return;
                              }

                              // local cell style helpers
                              static IContainer CellHeaderStyle(IContainer container) =>
                                  container
                                      .Background(Colors.Grey.Lighten3)
                                      .Border(0.2f)
                                      .BorderColor(Colors.Grey.Lighten1)
                                      .PaddingVertical(3)
                                      .PaddingHorizontal(3)
                                      .DefaultTextStyle(x => x.SemiBold());

                              static IContainer CellBodyStyle(IContainer container) =>
                                  container
                                      .Border(0.2f)
                                      .BorderColor(Colors.Grey.Lighten1)
                                      .PaddingVertical(2)
                                      .PaddingHorizontal(3);

                              // Build compact table for TableHead
                              summaryContainer.Table(table =>
                              {
                                  // define columns: #, ItemName, Type, UOM, Gross, Tare, Net
                                  table.ColumnsDefinition(columns =>
                                  {
                                      columns.ConstantColumn(24);   // #
                                      columns.RelativeColumn(4);    // ItemName
                                      columns.RelativeColumn(2);    // UOM
                                      columns.RelativeColumn(2);    // Gross
                                      columns.RelativeColumn(2);    // Tare
                                      columns.RelativeColumn(2);    // Net
                                  });

                                  // header row (small) — matches columns above
                                  table.Header(header =>
                                  {
                                      header.Cell().Element(CellHeaderStyle).Text("#").AlignCenter();
                                      header.Cell().Element(CellHeaderStyle).Text("Item").AlignLeft();
                                      header.Cell().Element(CellHeaderStyle).Text("UOM").AlignCenter();
                                      header.Cell().Element(CellHeaderStyle).Text("Gross").AlignRight();
                                      header.Cell().Element(CellHeaderStyle).Text("Tare").AlignRight();
                                      header.Cell().Element(CellHeaderStyle).Text("Net").AlignRight();
                                  });

                                  // data rows
                                  foreach (var item in TableHead)
                                  {
                                      table.Cell().Element(CellBodyStyle).Text(item.Srno.ToString()).AlignCenter();
                                      table.Cell().Element(CellBodyStyle).Text(item.ItemName ?? "").AlignLeft();
                                      table.Cell().Element(CellBodyStyle).Text(item.ItemUOM ?? "").AlignCenter();
                                      table.Cell().Element(CellBodyStyle).Text(item.GrossWeight?.ToString("N2") ?? "").AlignRight();
                                      table.Cell().Element(CellBodyStyle).Text(item.TareWeight?.ToString("N2") ?? "").AlignRight();
                                      table.Cell().Element(CellBodyStyle).Text(item.NetWeight?.ToString("N2") ?? "").AlignRight();
                                  }
                              });
                          });

                       // Vertical separator (1px). Let height be automatic.
                      // row.ConstantItem(1).Background(Colors.Grey.Lighten3);

                       // RIGHT: Aux chart image (if present)
                       row.RelativeItem(1)
                          .AlignMiddle()
                          .PaddingLeft(5).PaddingRight(5).PaddingBottom(5)
                          .Element(imgContainer =>
                          {
                              if (AuxChartImage != null && AuxChartImage.Length > 0)
                              {
                                  imgContainer.Image(AuxChartImage).FitWidth();
                              }
                              else
                              {
                                  imgContainer.AlignCenter().Text("No chart image")
                                      .FontSize(10).FontColor(Colors.Grey.Darken1);
                              }
                          });
                   });

                // Row 3: Feeding details
                col.Item().PaddingTop(10).PaddingBottom(1).Row(row =>
                {

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignLeft().Text("Feeding Details : ")
                            .Italic().FontSize(14).SemiBold().FontColor(Colors.Grey.Lighten1);
                    });
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
                        columns.RelativeColumn(3); // Date
                        columns.RelativeColumn(3); // ReceiptN
                        columns.RelativeColumn(5); // Item
                        columns.RelativeColumn(3); // Shift
                        columns.RelativeColumn(3); // UOM
                        columns.RelativeColumn(3); // Gross
                        columns.RelativeColumn(3); // Tare
                        columns.RelativeColumn(3); // Net
                    });

                    // Table header (QuestPDF repeats this across pages automatically)
                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("#").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Date").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("ReceiptNo").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Item").AlignLeft();
                        header.Cell().Element(CellHeaderStyle).Text("Shift").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("UOM").AlignCenter();
                        header.Cell().Element(CellHeaderStyle).Text("Gross").AlignRight();
                        header.Cell().Element(CellHeaderStyle).Text("Tare").AlignRight();
                        header.Cell().Element(CellHeaderStyle).Text("Net").AlignRight();
                    });

                    // Data rows
                    foreach (var row in TableData)
                    {
                        table.Cell().Element(CellBodyStyle).Text(row.Srno.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.FeedingDate.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ReceiptNo.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ItemName.ToString()).AlignLeft();
                        table.Cell().Element(CellBodyStyle).Text(row.ShiftName.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(row.ItemUOM.ToString()).AlignCenter();
                        table.Cell().Element(CellBodyStyle).Text(string.Format("{0:F2}", row.GrossWeight)).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(string.Format("{0:F2}", row.TareWeight)).AlignRight();
                        table.Cell().Element(CellBodyStyle).Text(string.Format("{0:F2}", row.NetWeight)).AlignRight();

                    }

                    // Header cell style
                    static IContainer CellHeaderStyle(IContainer container) =>
                        container
                            .Background(Colors.Grey.Lighten3)
                            .Border(0.2f)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(5)
                            .PaddingHorizontal(3)
                            .DefaultTextStyle(x => x.SemiBold());

                    // Body cell style
                    static IContainer CellBodyStyle(IContainer container) =>
                        container
                            .Border(0.2f)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(3)
                            .PaddingHorizontal(3);
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
