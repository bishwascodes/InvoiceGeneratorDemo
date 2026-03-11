namespace InvoiceGenerator.Behaviors
{
    public class SelectAllOnFocusBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.Focused += OnEntryFocused;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.Focused -= OnEntryFocused;
        }

        private void OnEntryFocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            }
        }
    }
}
