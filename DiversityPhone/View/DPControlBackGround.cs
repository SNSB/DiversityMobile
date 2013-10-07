using System;
using System.Windows.Controls;

namespace DiversityPhone.View
{
    public class DPControlBackGround
    {
        public static void setTBBackgroundColor(TextBox tb)
        {
            if (tb.Text == String.Empty || tb.Text == null)
                tb.Background = DPColors.INPUTMISSSING;
            else
                tb.Background = DPColors.STANDARD;
        } 
    }
}
