using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Services
{
    public class BackgroundTaskInvocation : IComparable<BackgroundTaskInvocation>
    {
        public Type Type { get; private set; }
        public bool HasStarted { get; set; }
        public object Argument { get; set; }
        public Dictionary<string, object> State { get; set; }


        public BackgroundTaskInvocation(Type type)
        {
            Type = type;
            State = new Dictionary<string, object>();
        }

        public int CompareTo(BackgroundTaskInvocation other)
        {
            return Type.GetHashCode().CompareTo(other.Type.GetHashCode());
        }
    }
}
