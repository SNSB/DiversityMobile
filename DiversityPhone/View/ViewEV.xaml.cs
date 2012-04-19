
namespace DiversityPhone
{
    using System;
    using Microsoft.Phone.Controls;
    using DiversityPhone.ViewModels;

    public partial class ViewEV : PhoneApplicationPage
    {
        private ViewEVVM VM
        {
            get
            {
                return DataContext as ViewEVVM;
            }
        }

        public ViewEV()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Add.Execute(null);
        }

        private void Map_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Maps.Execute(null);
        }
    }
}