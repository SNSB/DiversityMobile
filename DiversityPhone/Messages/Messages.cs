using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DiversityPhone.Messages
{
    public static class MessageContracts
    {
        public const string EDIT = "Edit";
        public const string SAVE = "Save";
        public const string SELECT = "Select";

    }

    public class DialogMessage
    {
        public DialogType Type { get; private set; }
        public string Caption { get; private set; }
        public string Text { get; private set; }

        public DialogMessage(DialogType type, string caption, string text)
        {
            Type = type;
            Caption = caption ?? "";
            Text = text ?? "";
        }


        public static implicit operator DialogMessage(string text)
        {
            return new DialogMessage(DialogType.OK, "", text);
        }
    }

    public enum DialogType
    {
        OK,
        YesNo
    }

    public enum Message 
    {       
        NavigateBack,
        ClearHistory
    };
}
