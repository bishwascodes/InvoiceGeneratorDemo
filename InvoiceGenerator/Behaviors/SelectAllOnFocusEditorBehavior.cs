namespace InvoiceGenerator.Behaviors
{
    public class SelectAllOnFocusEditorBehavior : Behavior<Editor>
    {
        protected override void OnAttachedTo(Editor editor)
        {
            base.OnAttachedTo(editor);
            editor.Focused += OnEditorFocused;
        }

        protected override void OnDetachingFrom(Editor editor)
        {
            base.OnDetachingFrom(editor);
            editor.Focused -= OnEditorFocused;
        }

        private void OnEditorFocused(object? sender, FocusEventArgs e)
        {
            if (sender is Editor editor && !string.IsNullOrEmpty(editor.Text))
            {
                editor.CursorPosition = 0;
                editor.SelectionLength = editor.Text.Length;
            }
        }
    }
}
