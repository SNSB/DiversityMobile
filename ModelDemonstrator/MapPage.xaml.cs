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


namespace ModelDemonstrator
{
    public partial class MapPage : PhoneApplicationPage
    {
        
        private MapServiceReference.MapServiceClient m_Proxy;
        IList<String> maps;


        public MapPage()
        {
            InitializeComponent();
            maps = new List<string>();
            m_Proxy = new MapServiceReference.MapServiceClient();
            m_Proxy.GetMapListAsync();
            m_Proxy.GetMapListCompleted+=new EventHandler<MapServiceReference.GetMapListCompletedEventArgs>(m_Proxy_GetMapListCompleted);
            
           
        }

        public void m_Proxy_GetMapListCompleted(object sender, MapServiceReference.GetMapListCompletedEventArgs e)
        {

            maps = e.Result;

        }

        private void PageTitle_Tap(object sender, GestureEventArgs e)
        {

        }
    }
}