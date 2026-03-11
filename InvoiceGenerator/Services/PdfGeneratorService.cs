using InvoiceGenerator.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPdfColors = QuestPDF.Helpers.Colors;
using QuestPdfContainer = QuestPDF.Infrastructure.IContainer;

namespace InvoiceGenerator.Services
{
    public class PdfGeneratorService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPdfColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(QuestPdfColors.Black));

                    page.Header().Element(c => ComposeHeader(c, invoice));
                    page.Content().Element(c => ComposeContent(c, invoice));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(QuestPdfContainer container, Invoice invoice)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Left side - Business Info
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(invoice.BusinessName)
                            .FontSize(20).Bold().FontColor("#1976D2");

                        col.Item().PaddingTop(5).Text(invoice.BusinessAddress)
                            .FontSize(10).FontColor(QuestPdfColors.Grey.Darken2);

                        col.Item().PaddingTop(3).Text(invoice.BusinessPhone + " • " + invoice.BusinessEmail)
                            .FontSize(10).FontColor(QuestPdfColors.Grey.Darken2);
                    });

                    // Right side - INVOICE title
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("INVOICE")
                            .FontSize(36).Bold().FontColor("#1976D2");
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor("#1976D2");
            });
        }

        private void ComposeContent(QuestPdfContainer container, Invoice invoice)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(15);

                // Invoice Details
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("Invoice #: ").Bold().FontSize(12);
                            r.AutoItem().Text(invoice.InvoiceNumber).FontSize(12);
                        });

                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.AutoItem().Text("Date: ").Bold().FontSize(12);
                            r.AutoItem().Text(invoice.InvoiceDate.ToString("MMM dd, yyyy")).FontSize(12);
                        });
                    });
                });

                column.Item().LineHorizontal(1).LineColor(QuestPdfColors.Grey.Lighten2);

                // Customer Information
                column.Item().Column(col =>
                {
                    col.Item().Text("BILL TO")
                        .FontSize(13).Bold().FontColor("#1976D2");

                    col.Item().PaddingTop(5).Text(invoice.CustomerName)
                        .FontSize(13).Bold();

                    if (!string.IsNullOrWhiteSpace(invoice.CustomerEmail))
                        col.Item().Text(invoice.CustomerEmail).FontSize(12).FontColor(QuestPdfColors.Grey.Darken1);

                    if (!string.IsNullOrWhiteSpace(invoice.CustomerAddress))
                        col.Item().Text(invoice.CustomerAddress).FontSize(12).FontColor(QuestPdfColors.Grey.Darken1);
                });

                column.Item().PaddingTop(10).BorderBottom(2).BorderColor("#1976D2");

                // Items Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);  // Description
                        columns.RelativeColumn(1);  // Quantity
                        columns.RelativeColumn(1);  // Unit Price
                        columns.RelativeColumn(1);  // Total
                    });

                    // Table Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("DESCRIPTION").Bold().FontColor("#1976D2");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("QTY").Bold().FontColor("#1976D2");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("RATE").Bold().FontColor("#1976D2");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("AMOUNT").Bold().FontColor("#1976D2");
                    });

                    // Table Rows
                    foreach (var item in invoice.Items)
                    {
                        table.Cell().Element(CellStyle).Text(item.Description);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:F2}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.Total:F2}").Bold();
                    }

                    // Styling helpers
                    static QuestPdfContainer HeaderCellStyle(QuestPdfContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(QuestPdfColors.Grey.Medium).PaddingVertical(8).PaddingHorizontal(5);
                    }

                    static QuestPdfContainer CellStyle(QuestPdfContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(QuestPdfColors.Grey.Lighten3).PaddingVertical(10).PaddingHorizontal(5);
                    }
                });

                column.Item().PaddingTop(15).BorderTop(2).BorderColor("#1976D2");

                // Totals Section
                column.Item().PaddingTop(10).AlignRight().Column(col =>
                {
                    col.Spacing(5);

                    // Subtotal
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(120).Text("Subtotal:").FontSize(13);
                        row.AutoItem().Width(100).AlignRight().Text($"${invoice.Subtotal:F2}").FontSize(13);
                    });

                    // Tax
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(120).Text($"Tax ({invoice.Tax}%):").FontSize(13);
                        row.AutoItem().Width(100).AlignRight().Text($"${invoice.TaxAmount:F2}").FontSize(13);
                    });

                    // Divider
                    col.Item().PaddingVertical(5).Width(220).LineHorizontal(1).LineColor(QuestPdfColors.Grey.Medium);

                    // Total
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(120).Text("TOTAL:").FontSize(17).Bold().FontColor("#1976D2");
                        row.AutoItem().Width(100).AlignRight().Text($"${invoice.Total:F2}").FontSize(17).Bold().FontColor("#1976D2");
                    });
                });

                column.Item().PaddingTop(20).BorderTop(2).BorderColor(QuestPdfColors.Grey.Lighten2);

                // Footer Text
                column.Item().PaddingTop(10).AlignCenter().Column(col =>
                {
                    col.Item().Text("Thank you for your business!")
                        .FontSize(13).Bold().FontColor("#1976D2");
                    col.Item().PaddingTop(3).Text("Please remit payment within 30 days")
                        .FontSize(11).Italic().FontColor(QuestPdfColors.Grey.Medium);
                });
            });
        }

        private void ComposeFooter(QuestPdfContainer container)
        {
            container.AlignCenter().Text(t =>
            {
                t.Span("Page ");
                t.CurrentPageNumber();
                t.Span(" of ");
                t.TotalPages();
                t.DefaultTextStyle(s => s.FontSize(10).FontColor(QuestPdfColors.Grey.Medium));
            });
        }
    }
}
