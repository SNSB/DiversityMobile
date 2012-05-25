using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Linq.Expressions;
using Microsoft.Phone.Shell;

namespace DiversityPhone.View
{
    public class ProgressBinding<VMType> where VMType : ReactiveUI.ReactiveObject
    {
        IDisposable _subscription;

        static ProgressIndicator Progress { get { return SystemTray.ProgressIndicator; } }

        public ProgressBinding(VMType viewmodel, Expression<Func<VMType,bool>> isBusyProperty )
        {
            if (viewmodel == null)
                throw new ArgumentNullException("viewmodel");
            if (isBusyProperty == null)
                throw new ArgumentNullException("isBusyProperty");

            _subscription =
                viewmodel
                    .ObservableForProperty(isBusyProperty)
                    .Value()
                    .Subscribe(isBusy =>
            {
                var p = Progress;
                if (p != null)
                    p.IsIndeterminate = p.IsVisible = isBusy;
            });            
        }

        public ProgressBinding(VMType viewmodel, Expression<Func<VMType, int>> progressProperty)
        {
            if (viewmodel == null)
                throw new ArgumentNullException("viewmodel");
            if (progressProperty == null)
                throw new ArgumentNullException("progressProperty");

            _subscription =
                viewmodel
                    .ObservableForProperty(progressProperty)
                    .Value()
                    .Subscribe(progress =>
                         {
                             var p = Progress;
                             if (p != null)
                                 p.Value = progress;
                         });
        }
    }
}
