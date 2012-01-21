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
        /// <summary>
        /// Information that should be displayed by the visited Page
        /// e.g. ID of the object that the user wants to edit etc...
        /// </summary>        
        public string Context { get;  set; }

        /// <summary>
        /// Type of the Referrer (Object that originated the Request)
        /// </summary>        
        public ReferrerType ReferrerType { get;  set; }

        /// <summary>
        /// Information that shows the Origin of the Request, if needed
        /// e.g. ID of the parent object
        /// </summary>        
        public string Referrer { get;  set; }

        /// <summary>
        /// State Dictionary, that should be used by the Page to persist custom state
        /// (internal to the Page)
        /// </summary>        
        public IDictionary<string, string> State { get; set; }

        public PageState(string context, ReferrerType refType = ReferrerType.None, string referrer = null)
        {            
            this.Context = context;
            this.ReferrerType = refType;
            this.Referrer = (ReferrerType != ReferrerType.None) ? referrer : null;
            this.State = new Dictionary<string, string>();
        }
        public PageState()
        {

        }
    }
}
