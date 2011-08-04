using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Discovery;

namespace DiversityService
{
    class Program
    {
        static void Main(string[] args)
        {
            

            // Create a ServiceHost for the CalculatorService type.
            ServiceHost serviceHost = new ServiceHost(typeof(DiversityService));                       

            try
            {       

                serviceHost.Open();

                
                Console.WriteLine("Press <ENTER> to terminate the service.");
                Console.WriteLine();
                Console.ReadLine();
                serviceHost.Close();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }

            if (serviceHost.State != CommunicationState.Closed)
            {
                Console.WriteLine("Aborting the service...");
                serviceHost.Abort();
            }
            
        }
    }
}
