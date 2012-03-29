using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Threading;

namespace DiversityPhone.View
{
    public partial class ViewAudio : PhoneApplicationPage
    {

        private DispatcherTimer timer;
        
        private ViewAudioVM VM { get { return this.DataContext as ViewAudioVM; } }

        private ViewAudioVideoPageAppbarUpdater _appb;

        public ViewAudio()
        {
            InitializeComponent();
            _appb = new ViewAudioVideoPageAppbarUpdater(this.ApplicationBar, VM);

            // Timer to simulate the XNA Framework game loop (Microphone is 
            // from the XNA Framework). We also use this timer to monitor the 
            // state of audio playback so we can update the UI appropriately.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            VM.UnloadContent();
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        /// If sound has stopped playing, it updates the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (true == VM.SoundIsPlaying)
            {
                if (VM.sound.State != SoundState.Playing)
                {
                    // Audio has finished playing
                    VM.SoundIsPlaying = false;
                    VM.StatusImage = VM.blankImage;
                    _appb.adjustPlaying(false);
                }
            }
        }


    }
}