namespace DiversityPhone.Services
{
    using DiversityPhone.Messages;

    public class DialogMessage
    {
        public DialogMessage(DialogType type, string caption, string text)
        {
            this.Type = type;
            this.Caption = caption ?? string.Empty;
            this.Text = text ?? string.Empty;
        }

        public DialogType Type { get; private set; }

        public string Caption { get; private set; }

        public string Text { get; private set; }        

        public static implicit operator DialogMessage(string text)
        {
            return new DialogMessage(DialogType.OK, string.Empty, text);
        }
    }
}
