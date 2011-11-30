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
using System.Collections.Generic;

namespace DiversityPhone.Services
{
    public class PageState
    {
        public string Token { get; private set; }
        public string Context { get; private set; }
        public IDictionary<string, string> State { get; set; }

        public PageState(string token, string context)
        {
            this.Token = token;
            this.Context = context;
            this.State = new Dictionary<string, string>();
        }
    }
}
