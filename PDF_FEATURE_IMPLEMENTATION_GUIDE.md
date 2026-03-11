# Invoice PDF Generation Feature - Complete Implementation Guide

---

## Step 1: Install QuestPDF Package

### 1.1 Using Package Manager Console

1. Open **Tools → NuGet Package Manager → Package Manager Console**
2. Run this command:
   ```powershell
   Install-Package QuestPDF
   ```

### 1.2 Using Visual Studio UI (Alternative)

1. Right-click on the `InvoiceGenerator` project
2. Select **Manage NuGet Packages**
3. Click the **Browse** tab
4. Search for `QuestPDF`
5. Click **Install**
6. Accept any license agreements

### 1.3 Verify Installation

Check your `.csproj` file includes:
```xml
<PackageReference Include="QuestPDF" Version="2024.x.x" />
```

---

## Step 2: Configure QuestPDF License

### 2.1 Open MauiProgram.cs

Navigate to: `InvoiceGenerator/MauiProgram.cs`

### 2.2 Add Using Statement

At the top of the file, add:
```csharp
using QuestPDF.Infrastructure;
```

### 2.3 Configure License

Add this line **before** `var builder = MauiApp.CreateBuilder();`:

**Complete MauiProgram.cs:**
```csharp
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;  // ← Add this

namespace InvoiceGenerator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Configure QuestPDF Community License (free for education/evaluation)
            QuestPDF.Settings.License = LicenseType.Community;  // ← Add this

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

---

## Step 3: Create PDF Generator Service

### 3.1 Create Services Folder

1. Right-click on the `InvoiceGenerator` project
2. Select **Add → New Folder**
3. Name it `Services`

### 3.2 Create PdfGeneratorService.cs

1. Right-click on the `Services` folder
2. Select **Add → Class**
3. Name it `PdfGeneratorService.cs`
4. Replace the entire content with:

```csharp
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
```

### 3.3 Save and Build

Press `Ctrl+Shift+B` to build and verify no errors.

---

## Step 4: Update Invoice ViewModel

### 4.1 Open InvoiceViewModel.cs

Navigate to: `InvoiceGenerator/ViewModels/InvoiceViewModel.cs`

### 4.2 Add Using Statements

At the top of the file, add:
```csharp
using InvoiceGenerator.Services;
#if WINDOWS
using Microsoft.Maui.Platform;
#endif
```

### 4.3 Add Private Fields

Inside the class, add these fields (around line 15):
```csharp
private readonly PdfGeneratorService _pdfGenerator;
private bool _isGeneratingPdf = false;
private string _statusMessage = string.Empty;
```

### 4.4 Add Public Properties

Add these properties after the private fields:
```csharp
public string StatusMessage
{
    get => _statusMessage;
    set
    {
        _statusMessage = value;
        OnPropertyChanged();
    }
}

public bool IsGeneratingPdf
{
    get => _isGeneratingPdf;
    set
    {
        _isGeneratingPdf = value;
        OnPropertyChanged();
    }
}
```

### 4.5 Update Constructor

In the constructor, add this as the **first line**:
```csharp
public InvoiceViewModel()
{
    _pdfGenerator = new PdfGeneratorService();  // ← Add this line

    _invoice = new Invoice
    {
        // ... existing code
    };

    // ... rest of constructor
}
```

### 4.6 Update CanPrintInvoice Method

Find the `CanPrintInvoice()` method and update it:
```csharp
private bool CanPrintInvoice()
{
    return !string.IsNullOrWhiteSpace(CustomerName) && Items.Count > 0 && !IsGeneratingPdf;
}
```

### 4.7 Replace PrintInvoice Method

Find the `PrintInvoice()` method and replace it completely:
```csharp
private async void PrintInvoice()
{
    // Prevent multiple clicks
    if (IsGeneratingPdf)
    {
        System.Diagnostics.Debug.WriteLine("Already generating PDF, ignoring click");
        return;
    }

    try
    {
        // Update UI immediately on main thread
        IsGeneratingPdf = true;
        StatusMessage = "Generating PDF...";
        ((Command)PrintInvoiceCommand).ChangeCanExecute();

        System.Diagnostics.Debug.WriteLine("=== Starting PDF Generation ===");

        // Create filename first (on UI thread, uses properties)
        var fileName = $"Invoice_{InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        System.Diagnostics.Debug.WriteLine($"Filename: {fileName}");

        // Generate PDF on background thread (CPU-intensive work)
        StatusMessage = "Creating PDF document...";
        byte[] pdfBytes = await Task.Run(() =>
        {
            System.Diagnostics.Debug.WriteLine("Generating PDF on background thread...");
            var bytes = _pdfGenerator.GenerateInvoicePdf(_invoice);
            System.Diagnostics.Debug.WriteLine($"PDF generated: {bytes.Length} bytes");
            return bytes;
        });

#if WINDOWS
        System.Diagnostics.Debug.WriteLine("Platform: Windows - Showing save dialog");
        StatusMessage = "Choose save location...";

        // Windows - Use File Save Picker (must be on UI thread)
        var filePath = await SaveFileWindows(pdfBytes, fileName);

        if (filePath != null)
        {
            System.Diagnostics.Debug.WriteLine($"File saved to: {filePath}");
            StatusMessage = "PDF saved successfully!";

            await Task.Delay(100); // Small delay to ensure UI updates

            await Application.Current!.MainPage!.DisplayAlert(
                "Success!",
                $"Invoice PDF saved successfully!\n\n" +
                $"Location: {filePath}\n" +
                $"Size: {pdfBytes.Length / 1024} KB",
                "OK");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Save dialog cancelled by user");
            StatusMessage = "Save cancelled";
        }
#else
        System.Diagnostics.Debug.WriteLine("Platform: Mobile - Using share");
        StatusMessage = "Preparing to share...";

        // Other platforms - Save to app directory and share
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        // Share/Open the PDF
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Share Invoice PDF",
            File = new ShareFile(filePath)
        });

        StatusMessage = "PDF shared successfully!";

        await Application.Current!.MainPage!.DisplayAlert(
            "Success!",
            $"Invoice PDF generated and ready to share!\n\n" +
            $"File: {fileName}\n" +
            $"Size: {pdfBytes.Length / 1024} KB",
            "OK");
#endif
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

        StatusMessage = "Error generating PDF";

        // Show error message
        await Application.Current!.MainPage!.DisplayAlert(
            "Error",
            $"Failed to generate PDF:\n{ex.Message}\n\n" +
            $"Type: {ex.GetType().Name}",
            "OK");
    }
    finally
    {
        // Always reset state on UI thread
        IsGeneratingPdf = false;
        StatusMessage = string.Empty;
        ((Command)PrintInvoiceCommand).ChangeCanExecute();
        System.Diagnostics.Debug.WriteLine("=== PDF Generation Complete ===");
    }
}
```

### 4.8 Add Windows Save File Method

Add this method at the end of the class (before the closing braces):
```csharp
#if WINDOWS
private async Task<string?> SaveFileWindows(byte[] pdfBytes, string fileName)
{
    try
    {
        System.Diagnostics.Debug.WriteLine("Creating FileSavePicker...");
        var savePicker = new Windows.Storage.Pickers.FileSavePicker();

        // Get the current window handle
        System.Diagnostics.Debug.WriteLine("Getting window handle...");
        var window = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler.PlatformView!);
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        // Set file type and default location
        System.Diagnostics.Debug.WriteLine("Configuring picker...");
        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.SuggestedFileName = fileName;
        savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });

        // Show save file dialog
        System.Diagnostics.Debug.WriteLine("Showing save dialog...");
        var file = await savePicker.PickSaveFileAsync();

        if (file != null)
        {
            System.Diagnostics.Debug.WriteLine($"File selected: {file.Path}");
            System.Diagnostics.Debug.WriteLine("Writing bytes to file...");

            // Save the PDF
            await Windows.Storage.FileIO.WriteBytesAsync(file, pdfBytes);

            System.Diagnostics.Debug.WriteLine("File written successfully!");
            return file.Path;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("No file selected (dialog cancelled)");
        }

        return null;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"SaveFileWindows ERROR: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
        throw; // Re-throw so it's caught by the calling method
    }
}
#endif
```

---

## Step 5: Add UI Feedback Components

### 5.1 Create InverseBoolConverter

1. Right-click on `Converters` folder
2. Select **Add → Class**
3. Name it `InverseBoolConverter.cs`
4. Replace content with:

```csharp
using System.Globalization;

namespace InvoiceGenerator.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 0.5 : 1.0; // 50% opacity when true, 100% when false
            }
            return 1.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
```

---

## Step 6: Update MainPage UI

### 6.1 Open MainPage.xaml

Navigate to: `InvoiceGenerator/MainPage.xaml`

### 6.2 Add Namespace

In the `<ContentPage>` tag, add this attribute:
```xml
xmlns:converters="clr-namespace:InvoiceGenerator.Converters"
```

### 6.3 Add Resources

After the `<ContentPage>` opening tag, before `<ContentPage.BindingContext>`, add:
```xml
<ContentPage.Resources>
    <ResourceDictionary>
        <converters:IsNotNullOrEmptyConverter x:Key="IsNotNullOrEmptyConverter"/>
        <converters:InverseBoolConverter x:Key="InverseBoolConverter"/>
    </ResourceDictionary>
</ContentPage.Resources>
```

### 6.4 Add Status Message

Find the **Print Button** section (near the end of the left panel).

**Before** the Print Button `<Grid>`, add:
```xml
<!-- Status Message -->
<Label Text="{Binding StatusMessage}"
       FontSize="14"
       TextColor="#1976D2"
       FontAttributes="Bold"
       HorizontalOptions="Center"
       IsVisible="{Binding IsGeneratingPdf}"
       Margin="0,10,0,5"/>
```

### 6.5 Update Print Button

Find the `<Button>` element inside the Print Button `<Grid>` and add this attribute:
```xml
Opacity="{Binding IsGeneratingPdf, Converter={StaticResource InverseBoolConverter}}"
```

**Complete Print Button section should look like:**
```xml
<!-- Status Message -->
<Label Text="{Binding StatusMessage}"
       FontSize="14"
       TextColor="#1976D2"
       FontAttributes="Bold"
       HorizontalOptions="Center"
       IsVisible="{Binding IsGeneratingPdf}"
       Margin="0,10,0,5"/>

<!-- Print Button -->
<Grid>
    <Button Command="{Binding PrintInvoiceCommand}"
            BackgroundColor="#1976D2"
            TextColor="White"
            HeightRequest="55"
            CornerRadius="10"
            Margin="0,10,0,20"
            Text=""
            Opacity="{Binding IsGeneratingPdf, Converter={StaticResource InverseBoolConverter}}"/>
    <HorizontalStackLayout Spacing="10" 
                           HorizontalOptions="Center"
                           VerticalOptions="Center"
                           InputTransparent="True">
        <Label Text="{x:Static helpers:FontAwesomeIcons.Print}" 
               FontFamily="FontAwesomeSolid"
               FontSize="16"
               TextColor="White"
               VerticalOptions="Center"/>
        <Label Text="Print Invoice"
               FontSize="17"
               FontAttributes="Bold"
               TextColor="White"
               VerticalOptions="Center"/>
    </HorizontalStackLayout>
</Grid>
```

---

## Step 7: Build and Test

---

## Support Resources

- **QuestPDF Documentation**: https://www.questpdf.com/
- **QuestPDF Examples**: https://github.com/QuestPDF/QuestPDF/tree/main/Source/QuestPDF.Examples
- **.NET MAUI Docs**: https://learn.microsoft.com/en-us/dotnet/maui/
- **File System Helpers**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers

---

## License Information

**QuestPDF Community License**:
- ✅ Free for education, evaluation, and open-source projects
- ✅ Perfect for your class project/demo
- ❌ Commercial use requires paid license ($499/developer)

For production/commercial apps, consider:
- Purchasing QuestPDF commercial license
- Using PdfSharpCore (MIT License - completely free)

