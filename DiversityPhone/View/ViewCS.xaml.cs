namespace DiversityPhone.View
{
    using System;
    using Microsoft.Phone.Controls;
    using DiversityPhone.ViewModels;
    
    public partial class ViewCS : PhoneApplicationPage
    {
        private ViewCSVM VM
        {
            get
            {
                return DataContext as ViewCSVM;
            }
        }

        public ViewCS()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddSubunit.Execute(null);
        }
    }
}