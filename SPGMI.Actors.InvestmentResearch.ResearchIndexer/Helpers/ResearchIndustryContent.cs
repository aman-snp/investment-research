using System;
using System.Collections.Generic;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers
{
    internal class ResearchIndustryContent
    {
        public ResearchIndustryData data { get; set; }
        public IndustryHeaderData headers { get; set; }
    }

    public class ResearchIndustryData
    {
        public ResearchIndustryData()
        {
            ContentSet = "Research";
        }
        public string ContentSet { get; set; }
        public string id { get; set; }
        public string Industry { get; set; }
        public string IndustryName { get; set; }
        public string GicsIndustry { get; set; }
        public string GicsIndustryName { get; set; }
    }

    public class IndustryHeaderData {
        //public IndustryHeaderData()
        //{
        //    ApiKey = "9416b698-3a20-3c57-8a1b-385b667a81d7";
        //    // PreferredCollection = "mi-search-research";   //handled from consumer side side so no need to pass
        //}
        //public string FilePrimaryKey { get; set; }
        public List<UDR> Udr { get; set; }
        public string IssueId { get; set; }     //replacement of trackerId
        public int UpdateType { get; set; }     //replacement of isAtomic 1 = new, 0 = update
        public string ApiKey { get; set; }      //shared by Search team
        public string DocId { get; set; }
        // public string PreferredCollection { get; set; }  //handled from consumer side side so no need to pass
        public string RevisionId { get; set; }
    }
}
