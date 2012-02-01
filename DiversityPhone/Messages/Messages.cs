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
        ModelCreated,
    }

    public static class MessageContracts
    {
        public const string SAVE = "Save";
        public const string DELETE = "Delete";
        public const string USE = "Use";
    }

    public class NavigationMessage
    {
        public Page Destination { get; private set; }
        public string Context { get; private set; }
        public ReferrerType ReferrerType { get; private set; }
        public string Referrer { get; private set; }

        public NavigationMessage(Page destination, string ctx, ReferrerType refType = ReferrerType.None, string referrer = null)
        {
            this.Destination = destination;
            this.Context = ctx;
            this.ReferrerType = refType;
            this.Referrer = referrer;
        }

        
    }
}
