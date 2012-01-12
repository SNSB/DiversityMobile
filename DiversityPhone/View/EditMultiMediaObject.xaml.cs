﻿using System;
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

namespace DiversityPhone.View
{
    public partial class EditMultiMediaObject : PhoneApplicationPage
    {
        public EditMultiMediaObject()
        {
            InitializeComponent();
            String s=NavigationContext.QueryString["Source"];
            ViewModels.ViewModelLocator loc=this.FindName("Locator") as ViewModels.ViewModelLocator;
            if(loc == null)
                return;

            switch (s)
            {
                case "Event":                 
                    DataContext = loc.EditEVMMO;
                    break;                    
            }
        }
    }
}