using DiversityPhone.View.Appbar;
using DiversityPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiversityPhone
{
    public partial class ViewES : PhoneApplicationPage
    {

        private ViewESVM VM { get { return DataContext as ViewESVM; } }

        private CommandButtonAdapter _add, _map;

        public ViewES()
        {
            InitializeComponent();

            _add = new CommandButtonAdapter(ApplicationBar.Buttons[0] as IApplicationBarIconButton, VM.AddEvent);
            _map = new CommandButtonAdapter(ApplicationBar.Buttons[1] as IApplicationBarIconButton, VM.Maps);
        }

        
    }
}