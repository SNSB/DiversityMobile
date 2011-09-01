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
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        public EditIUVM(IMessageBus messenger)
        {
            _messenger = messenger;
        }
    }
}
