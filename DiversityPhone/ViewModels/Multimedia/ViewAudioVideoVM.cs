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
    public abstract class ViewAudioVideoVM : EditElementPageVMBase<MultimediaObject>
    {

        #region Commands
        public ReactiveCommand Play { get; protected set; }
        public ReactiveCommand Stop { get; protected set; }
        
        #endregion

        /// <summary>
        /// Determines, whether Save can execute
        /// </summary>
        /// <returns>Observable that will be used to enable/disable Save</returns>
        protected abstract IObservable<bool> CanPlay();
        protected abstract IObservable<bool> CanStop();


        public ViewAudioVideoVM() :base()
        {

            Play = new ReactiveCommand();
            Stop = new ReactiveCommand();
        }



    }
}
