using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using InvoiceGenerator.Models;
using InvoiceGenerator.Services;
#if WINDOWS
using Microsoft.Maui.Platform;
#endif

namespace InvoiceGenerator.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        private Invoice _invoice;
        private string _newItemDescription = "Web Design Services";
        private string _newItemQuantity = "1";
        private string _newItemPrice = "150.00";

        private readonly PdfGeneratorService _pdfGenerator;
        private bool _isGeneratingPdf = false;
        private string _statusMessage = string.Empty;


        public InvoiceViewModel()
        {
            _pdfGenerator = new PdfGeneratorService();

            _invoice = new Invoice
            {
                BusinessName = "Your Company Name",
                BusinessAddress = "123 Business St, Suite 100, City, State 12345",
                BusinessPhone = "(555) 123-4567",
                BusinessEmail = "contact@yourcompany.com",
                InvoiceNumber = GenerateInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                CustomerName = "Acme Corporation",
                CustomerEmail = "billing@acmecorp.com",
                CustomerAddress = "456 Client Avenue\nNew York, NY 10001",
                Tax = 10
            };

            // Add a sample item for demo
            _invoice.Items.Add(new InvoiceItem
            {
                Description = "Consulting Services",
                Quantity = 5,
                UnitPrice = 100.00m
            });

            AddItemCommand = new Command(AddItem, CanAddItem);
            RemoveItemCommand = new Command<InvoiceItem>(RemoveItem);
            PrintInvoiceCommand = new Command(PrintInvoice, CanPrintInvoice);
        }

        public string BusinessName
        {
            get => _invoice.BusinessName;
            set
            {
                _invoice.BusinessName = value;
                OnPropertyChanged();
            }
        }

        public string BusinessAddress
        {
            get => _invoice.BusinessAddress;
            set
            {
                _invoice.BusinessAddress = value;
                OnPropertyChanged();
            }
        }

        public string BusinessPhone
        {
            get => _invoice.BusinessPhone;
            set
            {
                _invoice.BusinessPhone = value;
                OnPropertyChanged();
            }
        }

        public string BusinessEmail
        {
            get => _invoice.BusinessEmail;
            set
            {
                _invoice.BusinessEmail = value;
                OnPropertyChanged();
            }
        }

        public string InvoiceNumber
        {
            get => _invoice.InvoiceNumber;
            set
            {
                _invoice.InvoiceNumber = value;
                OnPropertyChanged();
            }
        }

        public DateTime InvoiceDate
        {
            get => _invoice.InvoiceDate;
            set
            {
                _invoice.InvoiceDate = value;
                OnPropertyChanged();
            }
        }

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

        public string CustomerEmail
        {
            get => _invoice.CustomerEmail;
            set
            {
                _invoice.CustomerEmail = value;
                OnPropertyChanged();
            }
        }

        public string CustomerAddress
        {
            get => _invoice.CustomerAddress;
            set
            {
                _invoice.CustomerAddress = value;
                OnPropertyChanged();
            }
        }

        public decimal Tax
        {
            get => _invoice.Tax;
            set
            {
                _invoice.Tax = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TaxAmount));
                OnPropertyChanged(nameof(Total));
            }
        }

        public ObservableCollection<InvoiceItem> Items => _invoice.Items;

        public decimal Subtotal => _invoice.Subtotal;
        public decimal TaxAmount => _invoice.TaxAmount;
        public decimal Total => _invoice.Total;

        public string NewItemDescription
        {
            get => _newItemDescription;
            set
            {
                _newItemDescription = value;
                OnPropertyChanged();
                ((Command)AddItemCommand).ChangeCanExecute();
            }
        }

        public string NewItemQuantity
        {
            get => _newItemQuantity;
            set
            {
                _newItemQuantity = value;
                OnPropertyChanged();
                ((Command)AddItemCommand).ChangeCanExecute();
            }
        }

        public string NewItemPrice
        {
            get => _newItemPrice;
            set
            {
                _newItemPrice = value;
                OnPropertyChanged();
                ((Command)AddItemCommand).ChangeCanExecute();
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand PrintInvoiceCommand { get; }

        private bool CanAddItem()
        {
            return !string.IsNullOrWhiteSpace(NewItemDescription) &&
                   int.TryParse(NewItemQuantity, out int qty) && qty > 0 &&
                   decimal.TryParse(NewItemPrice, out decimal price) && price >= 0;
        }

        private void AddItem()
        {
            if (!CanAddItem()) return;

            var item = new InvoiceItem
            {
                Description = NewItemDescription,
                Quantity = int.Parse(NewItemQuantity),
                UnitPrice = decimal.Parse(NewItemPrice)
            };

            Items.Add(item);

            NewItemDescription = "Web Design Services";
            NewItemQuantity = "1";
            NewItemPrice = "150.00";

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(TaxAmount));
            OnPropertyChanged(nameof(Total));
            UpdatePrintButtonState();
        }

        private void RemoveItem(InvoiceItem? item)
        {
            if (item != null)
            {
                Items.Remove(item);
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(TaxAmount));
                OnPropertyChanged(nameof(Total));
                UpdatePrintButtonState();
            }
        }

        private bool CanPrintInvoice()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) && Items.Count > 0 && !IsGeneratingPdf;
        }

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

        private void UpdatePrintButtonState()
        {
            ((Command)PrintInvoiceCommand).ChangeCanExecute();
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
    }
}
