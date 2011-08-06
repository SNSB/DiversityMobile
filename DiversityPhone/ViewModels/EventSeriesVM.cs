using System;
using System.Net;
using ReactiveUI;

namespace DiversityPhone.ViewModels
{
    public class EventSeriesVM : ReactiveObject
    {
        public string HeadLine { get; private set; }
        public string SubLine { get; private set; }
        public bool Selected { get; set; }

        private int _modelID;

        public EventSeriesVM(int modelID)
        {
            _modelID = modelID;
        }
    }
}
