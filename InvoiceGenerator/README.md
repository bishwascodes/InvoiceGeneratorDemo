# Invoice Generator - .NET MAUI App

A simple invoice generator application built with .NET MAUI that allows you to create professional invoices with live preview.

## Features

### Current Implementation ✅

- **Customer Information Entry**
  - Customer Name
  - Customer Email
  - Customer Address

- **Invoice Details**
  - Auto-generated Invoice Number
  - Customizable Invoice Date
  - Editable Tax Rate

- **Invoice Items Management**
  - Add items with description, quantity, and unit price
  - Remove items with a single click
  - Automatic total calculation per item

- **Live Invoice Preview**
  - Real-time preview of the invoice as you type
  - Professional invoice layout
  - Displays all customer information
  - Shows itemized list with calculations
  - Subtotal, tax, and grand total calculations

- **Print Invoice Button**
  - Ready placeholder for PDF generation
  - Shows invoice summary in alert dialog

### Architecture

The app follows the **MVVM (Model-View-ViewModel)** pattern:

```
InvoiceGenerator/
├── Models/
│   ├── Invoice.cs          # Invoice data model
│   └── InvoiceItem.cs      # Invoice item data model
├── ViewModels/
│   └── InvoiceViewModel.cs # Business logic and data binding
├── Converters/
│   └── IsNotNullOrEmptyConverter.cs  # XAML value converter
├── MainPage.xaml           # UI layout
└── MainPage.xaml.cs        # Code-behind (minimal)
```

## How to Use

### Running the App

1. Open the solution in Visual Studio 2026
2. Select your target platform (Android, iOS, Windows, or Mac Catalyst)
3. Press F5 or click "Run"

### Creating an Invoice

1. **Fill in Invoice Information**
   - Invoice number is auto-generated (you can edit it)
   - Select the invoice date

2. **Enter Customer Details**
   - Customer name (required for printing)
   - Customer email (optional)
   - Customer address (optional)

3. **Add Invoice Items**
   - Enter item description
   - Enter quantity
   - Enter unit price
   - Click "Add Item"
   - Repeat for multiple items
   - Use the ✕ button in the preview to remove items

4. **Set Tax Rate**
   - Enter the tax percentage (default is 10%)

5. **View Live Preview**
   - The right panel shows a real-time preview of your invoice
   - All changes are reflected immediately

6. **Print Invoice**
   - Ensure you have at least one item and a customer name
   - Click "Print Invoice" button
   - Currently shows a summary alert (PDF generation not implemented)

## Code Highlights

### Data Binding

The app uses two-way data binding to connect the UI with the ViewModel:

```xaml
<Entry Text="{Binding CustomerName}" Placeholder="Enter customer name"/>
```

### Commands

Actions like adding/removing items use ICommand for clean separation:

```csharp
public ICommand AddItemCommand { get; }
public ICommand RemoveItemCommand { get; }
public ICommand PrintInvoiceCommand { get; }
```

### Observable Collections

Items are stored in an ObservableCollection for automatic UI updates:

```csharp
public ObservableCollection<InvoiceItem> Items => _invoice.Items;
```

### Property Change Notification

The ViewModel implements INotifyPropertyChanged for reactive updates:

```csharp
public string CustomerName
{
    get => _invoice.CustomerName;
    set
    {
        _invoice.CustomerName = value;
        OnPropertyChanged();
        UpdatePrintButtonState();
    }
}
```

## Next Steps: PDF Generation

The app is ready for PDF generation! See **PDF_GENERATION_GUIDE.md** for:

- Recommended libraries (QuestPDF, PdfSharpCore)
- Step-by-step implementation guide
- Code examples
- Platform-specific considerations
- Testing instructions

## For Your Class Demo

### What to Show

1. **App Functionality**
   - Create a sample invoice
   - Add multiple items
   - Show live preview updates
   - Demonstrate item removal
   - Show calculated totals

2. **Code Structure**
   - Explain MVVM pattern
   - Show Model classes
   - Explain ViewModel logic
   - Show XAML data binding

3. **Extension Point**
   - Show the `PrintInvoice()` method
   - Explain where PDF generation plugs in
   - Reference the PDF_GENERATION_GUIDE.md

### Demo Script

1. "Let me create an invoice for a customer..."
2. "As I type, notice the preview updates in real-time..."
3. "I can add multiple items with different quantities and prices..."
4. "The app automatically calculates subtotal, tax, and total..."
5. "Now, if I click Print Invoice, it shows a summary..."
6. "The actual PDF generation is where you'd extend this - see the guide..."

## Technology Stack

- **.NET 10**
- **.NET MAUI** (Multi-platform App UI)
- **XAML** for UI
- **C#** for logic
- **MVVM Pattern**

## Compatibility

- ✅ Android 5.0+ (API 21+)
- ✅ iOS 15.0+
- ✅ macOS 12.0+ (Mac Catalyst)
- ✅ Windows 10 Build 17763+

## Project Structure Explanation

### Models
Contains the data structures:
- `Invoice`: Represents the entire invoice with customer info and items
- `InvoiceItem`: Represents a single line item

### ViewModels
Contains the business logic:
- `InvoiceViewModel`: Manages invoice data, commands, and validation

### Converters
Contains XAML value converters:
- `IsNotNullOrEmptyConverter`: Shows/hides UI elements based on string values

### Views
Contains the UI:
- `MainPage.xaml`: The invoice generator interface

## Tips for Extension

1. **Add More Fields**: Edit the Invoice model and add corresponding UI elements
2. **Custom Styling**: Modify the XAML to change colors, fonts, and layout
3. **Validation**: Add more validation logic in the ViewModel
4. **Persistence**: Save invoices to local storage or a database
5. **Templates**: Create multiple invoice templates to choose from

## License

This is an educational project. Feel free to use and modify for learning purposes.

## Questions?

For questions about the code or how to extend it, refer to:
- The inline code comments
- PDF_GENERATION_GUIDE.md
- .NET MAUI documentation: https://learn.microsoft.com/en-us/dotnet/maui/

Good luck with your demo! 🚀
