using System;
using System.Collections.Generic;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers
{
    internal class ResearchContent
    {
        public Data data { get; set; }
        public HeaderData headers { get; set; }
    }

    public class Data
    {
        public Data()
        {
            ContentSet = "Research";
            ContentId = 34;
            _lw_data_source_s = "db_research_researchreports";
            _lw_data_source_collection_s = "mi-search-research";
            EntityType = "ResearchReport";
            Catalogs = new List<string> { "all", "documents", "default_collection", "imdata", "interactive" };
            Source = "db_research_researchreports";
            PageTitle = "Investment & Market Research : Purchase Preview";
            //catalog = "investmentresearch";
        }
        public string ContentSet { get; set; }
        public int ContentId { get; set; }
        public string _lw_data_source_s { get; set; }
        public string _lw_data_source_collection_s { get; set; }
        public string KeyFileVersion { get; set; }
        public string TextKeyFileVersion { get; set; }
        public string NonCodexTextFileVersion { get; set; }
        public int KeyFileCollection { get; set; }
        public int KeyFileFormat { get; set; }
        public long BytesExtended { get; set; }
        public string id { get; set; }
        public string EntityType { get; set; }
        public List<string> Catalogs { get; set; }
        public string Source { get; set; }
        public string Url { get; set; }
        public string Industry { get; set; }
        public string IndustryName { get; set; }
        public string GicsIndustry { get; set; }
        public string GicsIndustryName { get; set; }
        public string PageTitle { get; set; }
        public string Headline { get; set; }
        public string NewsWireHeadlineSortStr { get; set; }
        public bool IsLinkBack { get; set; }
        public bool IsRemotelyHosted { get; set; }
        public string WebsiteURLExtended { get; set; }
        public string BrokerHosted_AMR { get; set; }
        public string PrintCommand_RealTime { get; set; }
        public string PrintCommand_AMR { get; set; }
        public DateTime ResearchReportDate { get; set; }
        public string ResearchContributor { get; set; }
        public string ResearchContributorStatus { get; set; }
        public string ResearchContributorType { get; set; }
        public int ResearchContributorId { get; set; }
        public List<int> KeyCIQIRProduct { get; set; }
        public List<decimal> FileCollectionPrice { get; set; }
        public string ResearchAnalyst { get; set; }
        public List<int> ResearchAnalystId { get; set; }
        public string PrimaryAnalyst { get; set; }
        public string PrimaryAnalystId { get; set; }
        public bool IsTeam { get; set; }
        public string ResearchReportType { get; set; }

        public bool SensitiveInformation { get; set; }
        public List<int> KeyInvestResearchReportType { get; set; }
        public int Pages { get; set; }
        public string Language { get; set; }
        public int LanguageId { get; set; }
        public string PrimaryGeography { get; set; }
        public string KeyGeographyTree { get; set; }
        public string Companies { get; set; }
        public List<int> CompanyIds { get; set; }
        public int PrimaryCompanyId { get; set; }
        public string PrimaryCompany { get; set; }
        public string Synopsis { get; set; }
        public int KeyFileCollectionDetail { get; set; }
        public string Action { get; set; }

        public List<string> CategoryName { get; set; }
        public List<string> Category { get; set; }
        public DateTime UpdDate { get; set; }
        public DateTime lastUpdateDate { get; set; }
    }

    public class HeaderData {
        //public HeaderData() {
        //    ApiKey = "9416b698-3a20-3c57-8a1b-385b667a81d7";
        //   // PreferredCollection = "mi-search-research";   //handled from consumer side side so no need to pass
        //}
        public string FilePrimaryKey { get; set; }
        public List<UDR> Udr { get; set; }
        public string IssueId { get; set; }     //replacement of trackerId
        public int UpdateType { get; set; }     //replacement of isAtomic 1 = new, 0 = update
        public string ApiKey { get; set; }      //shared by Search team
        public string DocId { get; set; }
        // public string PreferredCollection { get; set; }  //handled from consumer side side so no need to pass
        public string RevisionId { get; set; }
    }

    public class UDR
    {
        public UDR()
        {
            source = "IRS3";
            parser = "text";
            destField = "Body";
        }
        public string source { get; set; }
        public string parser { get; set; }
        public string destField { get; set; }
    }

}
