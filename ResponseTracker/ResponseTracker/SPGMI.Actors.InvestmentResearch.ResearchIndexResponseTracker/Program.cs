
using System;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Inside the Main Method - Response Tracker");
                SPGMI.ContainerHost.Startup.Initialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {

            }


        }
    }
}
