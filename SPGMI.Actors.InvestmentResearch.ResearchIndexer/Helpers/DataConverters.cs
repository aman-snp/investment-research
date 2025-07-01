using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers
{
    public class DataConverters
    {
        //check impact
        internal static string ToJSONString(dynamic researchcontent)
        {
            StringBuilder JSONresult = new StringBuilder();
            JSONresult.Append(JsonConvert.SerializeObject(researchcontent));
            return JSONresult.ToString();
        }
    }
}
