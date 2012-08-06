using System;
using ReactiveUI;
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using System.Linq;
using System.Reactive.Linq;
using DiversityPhone.Messages;

namespace DiversityPhone.ViewModels
{
    public class ElementMultimediaVM : ReactiveCollection<MultimediaObjectVM>, IObserver<int>
    {
        ReferrerType ownerType;
        IFieldDataService Storage;
        private ReactiveAsyncCommand getMultimedia;

        public ReactiveCommand<IElementVM<MultimediaObjectVM>> SelectMultimedia { get; private set; }

        public ElementMultimediaVM(ReferrerType ownertype, IFieldDataService storage)
        {
            ownerType = ownertype;
            Storage = storage;

            getMultimedia = new ReactiveAsyncCommand();
            getMultimedia.RegisterAsyncFunction(id =>
                {
                    int owner = (int)id;
                    return storage.getMultimediaForObject(ownertype, owner)
                        .Select(mmo => new MultimediaObjectVM(mmo))
                        .ToList();
                }).SelectMany(vms => vms)
                .Subscribe(this.Add);

            SelectMultimedia = new ReactiveCommand<IElementVM<MultimediaObjectVM>>();
            SelectMultimedia
                .ToMessage(MessageContracts.EDIT);
        }

        public void OnCompleted()
        {
            
        }

        public void OnError(Exception exception)
        {
            
        }

        public void OnNext(int value)
        {
            if (getMultimedia.CanExecute(value))
            {
                this.Clear();
                getMultimedia.Execute(value);
            }
        }
    }
}
