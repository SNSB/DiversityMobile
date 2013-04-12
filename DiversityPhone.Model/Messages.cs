namespace DiversityPhone.Model
{
    using System;

    public enum DialogType
    {
        OK,
        YesNo
    }

    public enum DialogResult
    {
        OKYes,
        CancelNo
    }

    public class DialogMessage
    {
        public DialogMessage(DialogType type, string caption, string text, Action<DialogResult> callback = null)
        {
            this.Type = type;
            this.Caption = caption ?? string.Empty;
            this.Text = text ?? string.Empty;
            this.CallBack = callback;
        }

        public DialogType Type { get; private set; }

        public string Caption { get; private set; }

        public string Text { get; private set; }

        public Action<DialogResult> CallBack { get; private set; }

        public static implicit operator DialogMessage(string text)
        {
            return new DialogMessage(DialogType.OK, string.Empty, text);
        }
    }

    public static class VMMessages
    {
        public const string USED_EVENTPROPERTY_IDS = "UEID";
    }

    public static class MessageContracts
    {
        public const string SAVE = "Save";
        public const string DELETE = "Delete";
        public const string USE = "Use";

        public const string START = "Start";
        public const string STOP = "Stop";

        public const string VIEW = "View";
        public const string EDIT = "Edit";

        //Events
        /// <summary>
        /// Signals an application wide initialization prompting all VMs to reload data.
        /// i.e. when setup is finished, or new data has been downloaded
        /// </summary>
        public const string INIT = "Init";
        /// <summary>
        /// Signals that Vocabulary has been refreshed
        /// </summary>
        public const string REFRESH = "Refresh";
    }

    public class EventMessage
    {
        public static EventMessage Default = new EventMessage();

        private EventMessage()
        {

        }

    }
}