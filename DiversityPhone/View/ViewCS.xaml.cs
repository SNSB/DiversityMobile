namespace DiversityPhone.View
{
    using DiversityPhone.View.Appbar;
    using DiversityPhone.ViewModels;
    using Microsoft.Phone.Controls;
    using System;

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
            _mmo = new NewMultimediaAppBarUpdater(VM.Messenger, this, VM.MultimediaList);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.Add.Execute(null);
        }
    }
}