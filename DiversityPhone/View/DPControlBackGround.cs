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

namespace DiversityPhone.View
{
    public class DPControlBackGround
    {
        public static void adjustStoreBackgroundColors(IList<Control> toStore)
        {
            if (toStore != null)
            {
                foreach (Control c in toStore)
                {
                    if (c.GetType().Equals(typeof(TextBox)))
                    {
                        TextBox tb = (TextBox)c;
                        setTBBackgroundColor(tb);
                    }
                }
            }
        }

        public static void setTBBackgroundColor(TextBox tb)
        {
            if (tb.Text == String.Empty || tb.Text == null)
                tb.Background = DPColors.INPUTMISSSING;
            else
                tb.Background = DPColors.STANDARD;
        } 
    }
}
