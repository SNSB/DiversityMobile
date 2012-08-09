using DiversityPhone.View;

namespace DiversityPhone
{
    using System;
    using Microsoft.Phone.Controls;
    using DiversityPhone.ViewModels;
using DiversityPhone.View.Appbar;

    public partial class ViewEV : PhoneApplicationPage
    {
        private NewMultimediaAppBarUpdater _mmo_appbar;

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

            _mmo_appbar = new NewMultimediaAppBarUpdater(this, VM.MultimediaList);
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