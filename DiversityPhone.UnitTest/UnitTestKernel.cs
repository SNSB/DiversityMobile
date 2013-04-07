using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiversityPhone.Test
{
    static class UnitTestKernel
    {
        public static IKernel TestKernel;
        
        static UnitTestKernel()
        {
            TestKernel = new Ninject.StandardKernel();
            



            
        }

    }
}
