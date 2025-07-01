using System;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    static class Program
    {
        public static string ActorType = null;
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args != null)
                    ActorType = args[0];

                Console.WriteLine("Inside the Main Method - Research Indexer");
                SPGMI.ContainerHost.Startup.Initialize();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString() + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
