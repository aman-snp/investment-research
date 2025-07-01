using System;
using System.Collections.Generic;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers
{
    public enum ChangeType
    {
        NewDocument = 1,
        ContributorChange = 2,
        DocumentDelete = 3,
        NewCompanyLinking = 4,
        NewAnalystLinking = 8,
        NewProductBinding = 16,
        RefreshLanguage = 32,
        RefreshIndustry = 64,
        RefreshGeography = 128,
        RefreshApproach = 256,
        RefreshEvent = 512,
        RefreshCategory = 1024,
        RefreshSubCategory = 2048,
        RefreshFocus = 4096,
        BackFill = 8192,

    }
    public static class Constants
    {
        public static List<int> RefreshEntities = new List<int>() { 64 };
    }
}
