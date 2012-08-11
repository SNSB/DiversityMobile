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
using DiversityPhone.Services;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class ViewVideoVM : EditPageVMBase<MultimediaObject>, IAudioVideoPageVM
    {
        #region Commands
        public IReactiveCommand Play { get; protected set; }
        public IReactiveCommand Stop { get; protected set; }

        #endregion
      

        #region Properties


        private string _Uri;
        public string Uri
        {
            get { return _Uri; }
            set { this.RaiseAndSetIfChanged(x => x.Uri, ref _Uri, value); }
        }


        private bool _videoIsPlaying = false;
        public bool VideoIsPlaying
        {
            get { return _videoIsPlaying; }
            set { this.RaiseAndSetIfChanged(x => x.VideoIsPlaying, ref _videoIsPlaying, value); }
        }



        #endregion


        public ViewVideoVM()   
            : base(mmo => mmo.MediaType == MediaType.Video)
        {
            Play = new ReactiveCommand();
            Stop = new ReactiveCommand();

            CurrentModelObservable
              .Select(mmo => mmo.Uri)
              .BindTo(this, x => x.Uri);
        }
    }
}
