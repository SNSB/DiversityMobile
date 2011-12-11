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
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.ViewModels
{
    public class MapVM : ElementVMBase<Map>
    {        
        public override string Description { get { return Model.ToString(); } }
        public override Icon Icon { get { return Icon.Map; } }

        public MapVM(Map model, IMessageBus messenger)
            : base(messenger,model)
        {
        
        }
    }
}
