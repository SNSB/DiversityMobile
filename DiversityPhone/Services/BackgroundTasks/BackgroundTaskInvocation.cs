using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DiversityPhone.Services
{
    [DataContract]
    public class BackgroundTaskInvocation : IComparable<BackgroundTaskInvocation>
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public bool HasStarted { get; set; }
        [DataMember]
        public bool WasSuspended { get; set; }
               
        [DataMember]
        public Dictionary<string, string> State { get; set; }

        public object Argument { get; set; }
    

        public BackgroundTaskInvocation(Type type)
        {
            Type = type.ToString();
            State = new Dictionary<string, string>();
        }

        public int CompareTo(BackgroundTaskInvocation other)
        {
            return Type.GetHashCode().CompareTo(other.Type.GetHashCode());
        }
    }
}
