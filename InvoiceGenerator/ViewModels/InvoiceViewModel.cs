using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using InvoiceGenerator.Models;

namespace InvoiceGenerator.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        private Invoice _invoice;
        private string _newItemDescription = "Web Design Services";
        private string _newItemQuantity = "1";
        private string _newItemPrice = "150.00";

        public InvoiceViewModel()
        {
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
            return !string.IsNullOrWhiteSpace(CustomerName) && Items.Count > 0;
        }

        private async void PrintInvoice()
        {
            // TODO: Implement PDF generation here
            // See PDF_GENERATION_GUIDE.md for implementation details
            await Application.Current!.MainPage!.DisplayAlert(
                "Print Invoice",
                "PDF generation feature is not yet implemented.\n\n" +
                "This is where you would generate and save/share the PDF invoice.\n\n" +
                $"Invoice #{InvoiceNumber}\n" +
                $"Customer: {CustomerName}\n" +
                $"Total: ${Total:F2}",
                "OK");
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
    }
}
