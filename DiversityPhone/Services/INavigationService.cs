using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface INavigationService
    {
        void Navigate(Page p);
        bool CanNavigateBack();
        void NavigateBack();
        //void ClearHistory();
    }
}
