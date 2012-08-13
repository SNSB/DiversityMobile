using System;
using DiversityPhone.Model;
using Funq;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels.Maps
{
    public class ViewMapVMBase : ViewPageVMBase<Map>
    {
        protected IMapStorageService Maps;
        protected IGeoLocationService Geolocation;
        protected ISettingsService Settings;

        public ViewMapVMBase(Container ioc)
        {

        }
    }
}
