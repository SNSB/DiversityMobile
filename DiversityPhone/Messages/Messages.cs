using DiversityPhone.Services;
namespace DiversityPhone.Messages
{
    public enum DialogType
    {
        OK,
        YesNo
    }

    public enum Message
    {
        NavigateBack,
        ClearHistory
    }

    public static class MessageContracts
    {
        public const string SAVE = "Save";
        public const string DELETE = "Delete";
    }

    public class NavigationMessage
    {
        public Page Destination { get; private set; }
        public string Context { get; private set; }

        public NavigationMessage(Page destination, string ctx)
        {
            this.Destination = destination;
            this.Context = ctx;
        }
    }
}
