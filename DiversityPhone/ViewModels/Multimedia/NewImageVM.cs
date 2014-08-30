using DiversityPhone.Model;
using Microsoft.Phone.Tasks;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class NewImageVM
    {
        public NewImageVM(IMessageBus Messenger,
            IStoreImages ImageStore)
        {
            Messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.EDIT)
                .Where(vm => vm.Model.MediaType == MediaType.Image && vm.Model.IsNew())
                .SelectMany(mmo =>
                {
                    var capture = new CameraCaptureTask();
                    var results =
                        Observable.FromEventPattern<PhotoResult>(h => capture.Completed += h, h => capture.Completed -= h)
                        .Select(ev => ev.EventArgs)
                        .Catch(Observable.Empty<PhotoResult>())
                        .Select(res => new { VM = mmo, Result = res })
                        .Take(1)
                        .Replay(1);
                    results.Connect();
                    try
                    {
                        capture.Show();
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    return results;
                })
                .Where(tuple => tuple.VM != null
                    && tuple.Result != null
                    && tuple.Result.TaskResult == TaskResult.OK)
                .Do(tuple =>
                {
                    tuple.VM.Model.Uri = ImageStore.StoreImage(tuple.VM.Model.NewFileName(), tuple.Result);
                })
                .Select(t => t.VM as IElementVM<MultimediaObject>)
                .ToMessage(Messenger, MessageContracts.SAVE);
        }
    }
}