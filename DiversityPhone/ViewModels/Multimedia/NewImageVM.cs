using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Model;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Tasks;
using System.Reactive;
using Microsoft.Xna.Framework.Media;
using DiversityPhone.Services;

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
                .Do(tuple =>
                    {
                        var res = tuple.Result;
                        if (res != null && res.TaskResult == TaskResult.OK)
                        {
                            tuple.VM.Model.Uri = ImageStore.StoreImage(tuple.VM.Model.NewFileName(), res);
                        }
                    })
                .Select(t => t.VM as IElementVM<MultimediaObject>)
                .ToMessage(MessageContracts.SAVE);
        }
    }
}
