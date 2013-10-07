using Ninject;

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
