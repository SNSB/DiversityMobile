using DiversityPhone.DiversityService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public interface ICurrentCredentials
    {
        UserCredentials CurrentCredentials();
    }
}
