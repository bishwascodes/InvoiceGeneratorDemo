 PDF Generation Implementation Guide

This guide explains how to extend the Invoice Generator app to support PDF generation and printing functionality.

## Overview

The app currently has a "Print Invoice" button that displays a placeholder alert. This guide will help you implement actual PDF generation using popular .NET libraries.

## Current Implementation

The print functionality is located in:
- **File**: `ViewModels/InvoiceViewModel.cs`
- **Method**: `PrintInvoice()`
- **Current behavior**: Shows an alert with invoice details

```csharp
private async void PrintInvoice()
{
    // TODO: Implement PDF generation here
    await Application.Current!.MainPage!.DisplayAlert(
        "Print Invoice",
        "PDF generation feature is not yet implemented...",
        "OK");
}
```

## Recommended Approach: QuestPDF

### Why QuestPDF?
- Modern, fluent API
- Excellent documentation
- Great performance
- Cross-platform support
- Free for open-source/educational use (Community License)
- Perfect for .NET MAUI applications

### Step 1: Install NuGet Package

Add QuestPDF to your project:

```bash
dotnet add package QuestPDF
```

Or via Package Manager Console in Visual Studio:
```
Install-Package QuestPDF
```

### Step 2: Create PDF Generator Service

Create a new file: `Services/PdfGeneratorService.cs`

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InvoiceGenerator.Models;

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
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(container => ComposeContent(container, invoice));
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INVOICE").FontSize(32).Bold();
                    column.Item().PaddingTop(10);
                });
            });
        }

        private void ComposeContent(IContainer container, Invoice invoice)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(10);

                // Invoice Info
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").FontSize(12);
                        col.Item().Text($"Date: {invoice.InvoiceDate:MMM dd, yyyy}").FontSize(12);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1);

                // Customer Info
                column.Item().PaddingTop(10).Column(col =>
                {
                    col.Item().Text("Bill To:").Bold().FontSize(14);
                    col.Item().Text(invoice.CustomerName);
                    if (!string.IsNullOrWhiteSpace(invoice.CustomerEmail))
                        col.Item().Text(invoice.CustomerEmail);
                    if (!string.IsNullOrWhiteSpace(invoice.CustomerAddress))
                        col.Item().Text(invoice.CustomerAddress);
                });

                column.Item().PaddingTop(15).LineHorizontal(1);

                // Items Table
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Description
                        columns.RelativeColumn(1); // Quantity
                        columns.RelativeColumn(1); // Unit Price
                        columns.RelativeColumn(1); // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Description").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Price").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();
                    });

                    // Items
                    foreach (var item in invoice.Items)
                    {
                        table.Cell().Element(CellStyle).Text(item.Description);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:F2}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"${item.Total:F2}");
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                });

                column.Item().PaddingTop(10).LineHorizontal(2);

                // Totals
                column.Item().PaddingTop(10).AlignRight().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().PaddingRight(20).Text("Subtotal:");
                        row.AutoItem().Text($"${invoice.Subtotal:F2}");
                    });
                    col.Item().Row(row =>
                    {
                        row.AutoItem().PaddingRight(20).Text($"Tax ({invoice.Tax}%):");
                        row.AutoItem().Text($"${invoice.TaxAmount:F2}");
                    });
                    col.Item().LineHorizontal(1);
                    col.Item().Row(row =>
                    {
                        row.AutoItem().PaddingRight(20).Text("Total:").Bold().FontSize(16);
                        row.AutoItem().Text($"${invoice.Total:F2}").Bold().FontSize(16);
                    });
                });

                column.Item().PaddingTop(20).LineHorizontal(2);
                column.Item().PaddingTop(10).AlignCenter().Text("Thank you for your business!").Italic().FontColor(Colors.Grey.Medium);
            });
        }
    }
}
```

### Step 3: Update ViewModel to Use PDF Generator

Update the `PrintInvoice()` method in `ViewModels/InvoiceViewModel.cs`:

```csharp
using InvoiceGenerator.Services;

// Add field at class level
private readonly PdfGeneratorService _pdfGenerator;

// Update constructor
public InvoiceViewModel()
{
    _invoice = new Invoice
    {
        InvoiceNumber = GenerateInvoiceNumber(),
        InvoiceDate = DateTime.Now,
        Tax = 10
    };

    _pdfGenerator = new PdfGeneratorService();

    // ... rest of constructor
}

// Replace the PrintInvoice method
private async void PrintInvoice()
{
    try
    {
        // Generate PDF
        var pdfBytes = _pdfGenerator.GenerateInvoicePdf(_invoice);

        // Save to file
        var fileName = $"Invoice_{_invoice.InvoiceNumber}.pdf";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        // Share the PDF
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share Invoice",
            File = new ShareFile(filePath)
        });

        await Application.Current!.MainPage!.DisplayAlert(
            "Success",
            $"Invoice PDF generated successfully!\nSaved to: {fileName}",
            "OK");
    }
    catch (Exception ex)
    {
        await Application.Current!.MainPage!.DisplayAlert(
            "Error",
            $"Failed to generate PDF: {ex.Message}",
            "OK");
    }
}
```

### Step 4: Configure QuestPDF License

Add this to your `MauiProgram.cs` before creating the app:

```csharp
using QuestPDF.Infrastructure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configure QuestPDF License (for educational/development use)
        QuestPDF.Settings.License = LicenseType.Community;

        var builder = MauiApp.CreateBuilder();
        // ... rest of configuration
    }
}
```

## Alternative Approach: PdfSharpCore

If you prefer PdfSharpCore (open-source alternative):

### Install Package
```bash
dotnet add package PdfSharpCore
```

### Example Implementation

```csharp
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

public class PdfGeneratorService
{
    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Arial", 12);
        var boldFont = new XFont("Arial", 16, XFontStyle.Bold);

        double y = 40;

        // Title
        gfx.DrawString("INVOICE", boldFont, XBrushes.Black, 
            new XRect(0, y, page.Width, 40), XStringFormats.TopCenter);
        y += 60;

        // Invoice details
        gfx.DrawString($"Invoice #: {invoice.InvoiceNumber}", font, XBrushes.Black, 40, y);
        y += 20;
        gfx.DrawString($"Date: {invoice.InvoiceDate:MMM dd, yyyy}", font, XBrushes.Black, 40, y);
        y += 40;

        // Customer info
        gfx.DrawString("Bill To:", new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black, 40, y);
        y += 20;
        gfx.DrawString(invoice.CustomerName, font, XBrushes.Black, 40, y);
        // ... continue with more details

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }
}
```

## Platform-Specific Considerations

### Android
- Ensure storage permissions are requested if saving to external storage
- Use `FileProvider` for sharing files with other apps

### iOS
- Ensure proper info.plist entries for file sharing
- Use `UIActivityViewController` for sharing

### Windows
- Use `FileSavePicker` for letting users choose save location
- Consider using Windows Print API for direct printing

## Testing Your Implementation

1. **Build the project**: Ensure QuestPDF package is restored
2. **Run the app**: Test invoice creation
3. **Click Print Invoice**: Should generate and share PDF
4. **Check output**: Open generated PDF to verify formatting

## Additional Features to Consider

1. **Email Integration**: Add email sending capability
2. **Cloud Storage**: Save to OneDrive/Google Drive
3. **Template Customization**: Allow users to choose invoice templates
4. **Logo Support**: Add company logo to invoices
5. **Multiple Currencies**: Support different currencies
6. **Print Preview**: Show PDF preview before saving

## Resources

- **QuestPDF Documentation**: https://www.questpdf.com/
- **QuestPDF Examples**: https://github.com/QuestPDF/QuestPDF/tree/main/Source/QuestPDF.Examples
- **PdfSharpCore**: https://github.com/ststeiger/PdfSharpCore
- **.NET MAUI File System**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers

## License Considerations

- **QuestPDF**: Free for Community License (open source, education, evaluation). Commercial use requires paid license.
- **PdfSharpCore**: MIT License (completely free)
- **iTextSharp**: AGPL or commercial license

Choose based on your project requirements and budget.

## Demo Presentation Tips

When presenting to your class:

1. Show the current working app without PDF generation
2. Explain the architecture (Models, ViewModels, MVVM pattern)
3. Walk through the code structure
4. Explain where PDF generation would plug in (the `PrintInvoice()` method)
5. Show this documentation as a "next steps" guide
6. Discuss the various PDF library options and their trade-offs

Good luck with your demo!
