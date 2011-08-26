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

        public const string NAVIGATE_TO = "Navigate To";

        //Dialog Messages
        public const string SELECT_DATE = "Select Date"; //For Use with Message
        public const string SELECTED_DATE = "Selected Date"; //For Use with DateTime


    }

    public class Message { };
}
