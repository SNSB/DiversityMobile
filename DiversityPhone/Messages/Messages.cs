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
        public const string EDIT = "Edit";
        public const string SAVE = "Save";
        public const string DELETE = "Delete";
        public const string SELECT = "Select";
    }    
}
