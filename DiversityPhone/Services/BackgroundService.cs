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
using DiversityPhone.Messages;
using System.Collections.Generic;

namespace DiversityPhone.Services
{
    public class BackgroundService
    {
        List<IBackgroundProcess> waitingProcs = new List<IBackgroundProcess>();
        IBackgroundProcess runningProc = null;

        public BackgroundService(IMessageBus msngr)
        {
            msngr.Listen<IBackgroundProcess>(MessageContracts.START)
                .Subscribe(proc => startProcess(proc));

        }

        public void startProcess(IBackgroundProcess proc)
        {
            lock (this)
            {
                waitingProcs.Add(proc);
            }
            
        }

        private void nextProcess()
        {
            
            lock (this)
            {
                if (runningProc == null && waitingProcs.Count > 0)
                {
                    runningProc = waitingProcs[0];
                    waitingProcs.RemoveAt(0);
                    runningProc.Run();                   
                }
            }
            
        }

        
    }
}
