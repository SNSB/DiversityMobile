using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;


namespace DiversityPhone.View
{
    public partial class MapManagement : PhoneApplicationPage
    {
        private MapManagementVM VM { get { return DataContext as MapManagementVM; } }

        public MapManagement()
        {
            InitializeComponent();
        }
    }
}