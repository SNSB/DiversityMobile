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
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public abstract class ElementVMBase<T>
    {
        protected IMessageBus Messenger { get; private set; }

        public ReactiveCommand Select { get; protected set; }
        public ReactiveCommand Edit { get; protected set; }

        public T Model { get; private set; }
        public abstract string Description { get; }
        public abstract Icon Icon { get; }


        public ElementVMBase(IMessageBus _messenger, T model)
        {
            this.Model = model;
            this.Messenger = _messenger;    
           
        }
    }
}
