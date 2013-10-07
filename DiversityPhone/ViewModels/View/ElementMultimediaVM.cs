using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DiversityPhone.ViewModels
{
    public class ElementMultimediaVM : ReactiveCollection<MultimediaObjectVM>, IObserver<IMultimediaOwner>
    {        
        readonly IFieldDataService Storage;

        private ISubject<IMultimediaOwner> ownerSubject = new ReplaySubject<IMultimediaOwner>(1);
        private ObservableAsPropertyHelper<IMultimediaOwner> _Owner;
        private IMultimediaOwner Owner { get { return _Owner.Value; } }
        private ReactiveAsyncCommand getMultimedia;

        public ReactiveCommand<IElementVM<MultimediaObject>> SelectMultimedia { get; private set; }
        public ReactiveCommand AddMultimedia { get; private set; }
        public IObservable<IMultimediaOwner> NewMultimediaObservable { get; private set; }

        public ElementMultimediaVM(IFieldDataService storage)
        {            
            this.Storage = storage;

            getMultimedia = new ReactiveAsyncCommand();
            getMultimedia.RegisterAsyncFunction(own =>
                {
                    var owner = own as IMultimediaOwner;
                    if (owner == null)
                        return Enumerable.Empty<MultimediaObjectVM>();
                    return storage.getMultimediaForObject(owner)
                        .Select(mmo => new MultimediaObjectVM(mmo))
                        .ToList();
                }).SelectMany(vms => vms)
                .Subscribe(this.Add);
            ownerSubject
                .Where(owner => getMultimedia.CanExecute(owner))                
                .Do(_ => this.Clear())
                .Subscribe(getMultimedia.Execute);

            _Owner = new ObservableAsPropertyHelper<IMultimediaOwner>(ownerSubject, (o) => {}, null);

            this.ListenToChanges<MultimediaObject, MultimediaObjectVM>(mmo => Owner != null && mmo.OwnerType == Owner.EntityType && mmo.RelatedId == Owner.EntityID);

            SelectMultimedia = new ReactiveCommand<IElementVM<MultimediaObject>>();
            SelectMultimedia
                .Where(mmo => mmo.Model.MediaType == MediaType.Image)
                .ToMessage(MessageContracts.VIEW);

            SelectMultimedia
                .Where(mmo => mmo.Model.MediaType != MediaType.Image)
                .ToMessage(MessageContracts.EDIT);

            AddMultimedia = new ReactiveCommand();
            var newmmo =
            AddMultimedia
                .Select(_ => ownerSubject.FirstOrDefault())
                .Publish();
            NewMultimediaObservable = newmmo;
            newmmo.Connect();

        }

        public void OnCompleted()
        {
            ownerSubject.OnCompleted();
        }

        public void OnError(Exception exception)
        {
            ownerSubject.OnError(exception);   
        }

        public void OnNext(IMultimediaOwner value)
        {
            ownerSubject.OnNext(value);
        }
    }
}
