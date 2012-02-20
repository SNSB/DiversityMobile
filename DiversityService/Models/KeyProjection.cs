using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class KeyProjection
    {
        //Saves the keyChanges for a hierarchysevtion in the Format [oldKey,newKey]
        public KeyValuePair<int?, int?> eventKey;
        public Dictionary<int, int> specimenKeys;
        public Dictionary<int, int> iuKeys;

        public KeyProjection()
        {
            eventKey = new System.Collections.Generic.KeyValuePair<int?, int?>(null, null);
            specimenKeys = new Dictionary<int, int>();
            iuKeys = new Dictionary<int, int>();
        }
    }
}