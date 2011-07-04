using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Discovery;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8000/Diversity");

            // Create a ServiceHost for the CalculatorService type.
            ServiceHost serviceHost = new ServiceHost(typeof(DiversityService), baseAddress);

            serviceHost.AddServiceEndpoint(typeof(IDiversityService), new BasicHttpBinding(), String.Empty);           

            try
            {       

                serviceHost.Open();

                Console.WriteLine("Div Service started at {0}", baseAddress);
                Console.WriteLine();
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
