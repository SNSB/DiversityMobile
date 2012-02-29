using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityService.Model
{
    public class KeyProjection
    {
        //Saves the keyChanges for a hierarchysevtion in the Format [oldKey,newKey]
        public Dictionary<int, int> eventKey;
        public Dictionary<int, int> specimenKeys;
        public Dictionary<int, int> iuKeys;

        public KeyProjection()
        {
            eventKey = new Dictionary<int, int>();
            specimenKeys = new Dictionary<int, int>();
            iuKeys = new Dictionary<int, int>();
        }
    }
}