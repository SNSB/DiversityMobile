namespace DiversityPhone.View
{
    using System;
    using Microsoft.Phone.Controls;
    using DiversityPhone.ViewModels;
    using DiversityPhone.View.Appbar;
    
    public partial class ViewCS : PhoneApplicationPage
    {
        private NewMultimediaAppBarUpdater _mmo;

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
            _mmo = new NewMultimediaAppBarUpdater(this, VM.MultimediaList);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Add.Execute(null);
        }
    }
}