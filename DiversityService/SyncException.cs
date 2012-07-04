using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService
{
    class SyncException : Exception
    {
        public SyncException(String message)
            : base(message)
        {
        }
    }
}
