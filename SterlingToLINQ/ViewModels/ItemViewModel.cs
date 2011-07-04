using System;
using System.ComponentModel;
using System.Diagnostics;
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
using SterlingToLINQ.DiversityService;

namespace SterlingToLINQ
{
    public class ItemViewModel : ReactiveObject
    {
        public CollectionEvent Model { get; private set; }

        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(Model.LocalityDescription) ? "No Description" : Model.LocalityDescription;
            }
        }

        public int EventID
        {
            get { return Model.CollectionEventID; }
        }

        public ItemViewModel(CollectionEvent model)
        {
            Model = model;
        }
    }
}