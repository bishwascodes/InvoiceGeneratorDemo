# PDF Generation Implementation Guide

This comprehensive guide will help you implement PDF generation and printing functionality for the Invoice Generator app. Follow these steps to make the "Print Invoice" button work!

---

## Current State

### Where We Are Now

The "Print Invoice" button exists in the UI and calls `PrintInvoice()` method in `InvoiceViewModel.cs`, which currently shows a placeholder alert.

**Location**: `ViewModels/InvoiceViewModel.cs` (Line ~240)

```csharp
private async void PrintInvoice()
{
    // TODO: Implement PDF generation here
    await Application.Current!.MainPage!.DisplayAlert(
        "Print Invoice",
        "PDF generation feature is not yet implemented.\n\n" +
        "This is where you would generate and save/share the PDF invoice.\n\n" +
        $"Invoice #{InvoiceNumber}\n" +
        $"Customer: {CustomerName}\n" +
        $"Total: ${Total:F2}",
        "OK");
}
```

### What We Need to Implement

✅ Generate a professional PDF document  
✅ Include business information (name, address, phone, email)  
✅ Display invoice details (number, date)  
✅ Show customer information  
✅ List all invoice items with quantities and prices  
✅ Calculate and display subtotal, tax, and total  
✅ Save the PDF file  
✅ Share/Open the generated PDF  

---

## QuestPDF Library

**Pros:**
- Modern, fluent C# API (no XML/HTML)
- Excellent documentation with examples
- Great performance and small file sizes
- Beautiful, professional output
- Cross-platform (works on all MAUI platforms)
- Active development and support
- Free for Community License (open source, education, evaluation)

**Cons:**
- Commercial use requires paid license ($499/developer)
- Relatively newer (less Stack Overflow answers)

**Best For:** Your project! Perfect for educational/demo purposes

---

### Step 1: Install QuestPDF Package

Open **Package Manager Console** (Tools → NuGet Package Manager → Package Manager Console) and run:

```powershell
Install-Package QuestPDF
```

Or using **.NET CLI** in terminal:

```bash
dotnet add package QuestPDF
```

Or via **Visual Studio UI**:
1. Right-click on project → Manage NuGet Packages
2. Search for "QuestPDF"
3. Click Install

### Step 2: Configure QuestPDF License

Open `MauiProgram.cs` and add the license configuration **before** building the app:

```csharp
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;  // ← Add this

namespace InvoiceGenerator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // ← Add this line for Community License (free for education/evaluation)
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome6Free-Solid.otf", "FontAwesomeSolid");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
```

### Step 3: Create PDF Generator Service

Create a new folder called `Services` in your project, then create `PdfGeneratorService.cs`:

**Right-click project → Add → New Folder** (name it "Services")  
**Right-click Services folder → Add → Class** (name it "PdfGeneratorService.cs")

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
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    page.Header().Element(container => ComposeHeader(container, invoice));
                    page.Content().Element(container => ComposeContent(container, invoice));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, Invoice invoice)
        {
            container.Row(row =>
            {
                // Left side - Business Info
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(invoice.BusinessName)
                        .FontSize(20).Bold().FontColor("#1976D2");

                    column.Item().PaddingTop(5).Text(invoice.BusinessAddress)
                        .FontSize(10).FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(3).Text(text =>
                    {
                        text.Span(invoice.BusinessPhone).FontSize(10);
                        text.Span(" • ").FontSize(10);
                        text.Span(invoice.BusinessEmail).FontSize(10);
                    }).FontColor(Colors.Grey.Darken2);
                });

                // Right side - INVOICE title
                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text("INVOICE")
                        .FontSize(36).Bold().FontColor("#1976D2");
                });
            });

            container.PaddingTop(10).BorderBottom(2).BorderColor("#1976D2");
        }

        private void ComposeContent(IContainer container, Invoice invoice)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(15);

                // Invoice Details
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Invoice #: ").Bold();
                            text.Span(invoice.InvoiceNumber);
                        }).FontSize(12);

                        col.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Date: ").Bold();
                            text.Span(invoice.InvoiceDate.ToString("MMM dd, yyyy"));
                        }).FontSize(12);
                    });
                });

                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Customer Information
                column.Item().Column(col =>
                {
                    col.Item().Text("BILL TO")
                        .FontSize(13).Bold().FontColor("#1976D2");

                    col.Item().PaddingTop(5).Text(invoice.CustomerName)
                        .FontSize(13).Bold();

                    if (!string.IsNullOrWhiteSpace(invoice.CustomerEmail))
                        col.Item().Text(invoice.CustomerEmail).FontSize(12).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrWhiteSpace(invoice.CustomerAddress))
                        col.Item().Text(invoice.CustomerAddress).FontSize(12).FontColor(Colors.Grey.Darken1);
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
                    static IContainer HeaderCellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingVertical(8).PaddingHorizontal(5);
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(10).PaddingHorizontal(5);
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
                    col.Item().PaddingVertical(5).Width(220).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    // Total
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(120).Text("TOTAL:").FontSize(17).Bold().FontColor("#1976D2");
                        row.AutoItem().Width(100).AlignRight().Text($"${invoice.Total:F2}").FontSize(17).Bold().FontColor("#1976D2");
                    });
                });

                column.Item().PaddingTop(20).BorderTop(2).BorderColor(Colors.Grey.Lighten2);

                // Footer Text
                column.Item().PaddingTop(10).AlignCenter().Column(col =>
                {
                    col.Item().Text("Thank you for your business!")
                        .FontSize(13).Bold().FontColor("#1976D2");
                    col.Item().PaddingTop(3).Text("Please remit payment within 30 days")
                        .FontSize(11).Italic().FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            }).FontSize(10).FontColor(Colors.Grey.Medium);
        }
    }
}
```

### Step 4: Update InvoiceViewModel

Open `ViewModels/InvoiceViewModel.cs` and make these changes:

**1. Add using statements at the top:**

```csharp
using InvoiceGenerator.Services;  // ← Add this
```

**2. Add a field for the PDF service (around line 11):**

```csharp
private Invoice _invoice;
private string _newItemDescription = "Web Design Services";
private string _newItemQuantity = "1";
private string _newItemPrice = "150.00";
private readonly PdfGeneratorService _pdfGenerator;  // ← Add this
```

**3. Initialize the service in the constructor (around line 15):**

```csharp
public InvoiceViewModel()
{
    _pdfGenerator = new PdfGeneratorService();  // ← Add this line

    _invoice = new Invoice
    {
        BusinessName = "Your Company Name",
        // ... rest of initialization
    };

    // ... rest of constructor
}
```

**4. Replace the `PrintInvoice()` method (around line 240):**

```csharp
private async void PrintInvoice()
{
    try
    {
        // Show loading indicator
        // Note: In a real app, you'd use a proper loading indicator

        // Generate PDF bytes
        var pdfBytes = _pdfGenerator.GenerateInvoicePdf(_invoice);

        // Create filename
        var fileName = $"Invoice_{InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        // Save PDF to app data directory
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        // Share/Open the PDF
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share Invoice PDF",
            File = new ShareFile(filePath)
        });

        // Optional: Show success message
        await Application.Current!.MainPage!.DisplayAlert(
            "Success!",
            $"Invoice PDF generated and ready to share!\n\n" +
            $"File: {fileName}\n" +
            $"Size: {pdfBytes.Length / 1024} KB",
            "OK");
    }
    catch (Exception ex)
    {
        // Show error message
        await Application.Current!.MainPage!.DisplayAlert(
            "Error",
            $"Failed to generate PDF:\n{ex.Message}\n\n" +
            $"Details: {ex.InnerException?.Message}",
            "OK");
    }
}
```

### Step 5: Test Your Implementation!

1. **Build the project**: Press `Ctrl+Shift+B` or Build → Build Solution
2. **Fix any errors**: Make sure all NuGet packages are restored
3. **Run the app**: Press `F5` or Debug → Start Debugging
4. **Fill in invoice details**: Add some items, customer info, etc.
5. **Click "Print Invoice"**: Should generate and share the PDF!

---


## Platform-Specific Features

### Android

For saving to Downloads folder or external storage:

```csharp
#if ANDROID
using Android.Content;
using AndroidX.Core.Content;

private async Task SaveToDownloads(byte[] pdfBytes, string fileName)
{
    var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
        Android.OS.Environment.DirectoryDownloads).AbsolutePath;

    var filePath = Path.Combine(downloadsPath, fileName);
    await File.WriteAllBytesAsync(filePath, pdfBytes);

    // Notify media scanner
    var context = Android.App.Application.Context;
    MediaScannerConnection.ScanFile(context, new[] { filePath }, null, null);
}
#endif
```

### iOS

For better sharing options:

```csharp
#if IOS
using UIKit;
using Foundation;

private async Task SharePdfIOS(string filePath)
{
    var fileUrl = NSUrl.FromFilename(filePath);
    var activityController = new UIActivityViewController(new[] { fileUrl }, null);

    var viewController = Platform.GetCurrentUIViewController();
    await viewController.PresentViewControllerAsync(activityController, true);
}
#endif
```

### Windows

For save file dialog:

```csharp
#if WINDOWS
using Windows.Storage.Pickers;
using Windows.Storage;

private async Task SaveFileWindows(byte[] pdfBytes, string fileName)
{
    var savePicker = new FileSavePicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = fileName
    };
    savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });

    var file = await savePicker.PickSaveFileAsync();
    if (file != null)
    {
        await FileIO.WriteBytesAsync(file, pdfBytes);
    }
}
#endif
```

---

### 5. Print Preview

Show PDF preview before sharing:

```csharp
// Save to temp location
var tempPath = Path.Combine(FileSystem.CacheDirectory, "preview.pdf");
await File.WriteAllBytesAsync(tempPath, pdfBytes);

// Open with default PDF viewer
await Launcher.Default.OpenAsync(new OpenFileRequest
{
    File = new ReadOnlyFile(tempPath)
});
```

---

## Resources & Documentation

### QuestPDF
- **Official Docs**: https://www.questpdf.com/
- **API Reference**: https://www.questpdf.com/api-reference/
- **Examples**: https://github.com/QuestPDF/QuestPDF/tree/main/Source/QuestPDF.Examples
- **Community**: https://github.com/QuestPDF/QuestPDF/discussions

### .NET MAUI
- **File System Helpers**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers
- **Share**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/share
- **Launcher**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/launcher


