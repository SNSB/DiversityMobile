

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Collections;
using System;
using DiversityPhone.ViewModels;
using DiversityPhone.Services;
using DiversityPhone.Messages;
namespace DiversityPhone.View.Appbar
{
    public class NewMultimediaAppBarUpdater<T> 
    {
        PhoneApplicationPage _page;
        IApplicationBar _appbar;
        ElementPageVMBase<T> _vm;
        IMultimediaOwner _mmowner;

        ApplicationBarIconButton _image, _audio, _video;
        IList _buttons, _mmobuttons;
        IDisposable _back_key;

        public NewMultimediaAppBarUpdater(PhoneApplicationPage page)
        {
            _page = page;
            _vm = page.DataContext as ElementPageVMBase<T>;

            if (_vm == null)
                throw new ArgumentNullException("vm");

            _vm.Messenger.Listen<IMultimediaOwner>(MessageContracts.MULTIMEDIA)
                .Subscribe(m => show_mmo_buttons(m));

            _mmobuttons = new List<object>();
            _buttons = new List<object>();
        }

        void show_mmo_buttons(IMultimediaOwner mmowner)
        {
            _mmowner = mmowner;
            _appbar.Buttons.Clear();
            foreach (var btn in _mmobuttons)
            {
                _appbar.Buttons.Add(btn);
            }
        }

        void restore_buttons()
        {
            _appbar.Buttons.Clear();
            foreach (var btn in _buttons)
            {
                _appbar.Buttons.Add(btn);
            }
            _back_key.Dispose();
        }
    }
}
