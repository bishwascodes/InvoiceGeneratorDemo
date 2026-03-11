using System.Collections.ObjectModel;

namespace InvoiceGenerator.Models
{
    public class Invoice
    {
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
        public string BusinessPhone { get; set; } = string.Empty;
        public string BusinessEmail { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public ObservableCollection<InvoiceItem> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Total);
        public decimal Tax { get; set; }
        public decimal TaxAmount => Subtotal * (Tax / 100);
        public decimal Total => Subtotal + TaxAmount;
    }
}
