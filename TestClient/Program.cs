using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            DiversityService.DiversityServiceClient svc = new DiversityService.DiversityServiceClient();
            svc.GetEvents(0, 100);
            Console.ReadLine();
        }
    }
}
