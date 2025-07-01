using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPGMI.OData.Client;
using SPGMI.OData.Client.Functions;
using SPGMI.OData.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ODataClientHelper
    {
        #region Global instance Variable


        readonly ODataHelper _helper;
        readonly int _maxRetry;
        readonly int _haltTime;
        public ILogger Logger { get; set; }
        readonly string oDataEndpointUri;
        readonly string oDataClientId;
        readonly string oDataAppKey;
        readonly string securityServiceUrl;

        static List<string> ErrorStringsForRetry = new List<string>("The remote procedure call failed|no error message text available|deadlocked|Unspecified error|Invalid pointer|The RPC server is unavailable|The underlying connection was closed|Either enlist this session in a new transaction or the NULL transaction|Microsoft Distributed Transaction Coordinator|Unable to connect to the remote server|The transaction has aborted|Data cannot be updated because it has been changed by another process|404 - File or directory not found|Cannot insert the value NULL into column".Split(new char[] { '|' }).Select(x => x.Trim()));


        #endregion
        public ODataClientHelper(string OdataEndPoint, string ODataClientId, string ODataAppKey, string SecurityServiceUrl, ILogger logger)
        {
            _maxRetry = Convert.ToInt32(5);
            _haltTime = Convert.ToInt32(2000);
            oDataEndpointUri = OdataEndPoint;
            oDataClientId = ODataClientId;
            oDataAppKey = ODataAppKey;
            securityServiceUrl = SecurityServiceUrl;

            var appName = "ResearchIndexer_int";
            ODataConfig config = new ODataConfig()
            {
                Endpoint = new Uri(oDataEndpointUri)
            };
            config.OAuthCredential = new SPGMI.OData.Client.Models.OAuthCredential(oDataClientId, oDataAppKey, securityServiceUrl, true);
            if (config.DefaultRequestHeaders == null)
                config.DefaultRequestHeaders = new Dictionary<string, string>();
            Logger = logger;
            config.DefaultRequestHeaders.Add("ApplicationName", appName);
             _helper= ODataHelper.GetInstance(config);
        }

        public bool ExecuteODataRequest(List<IODataOperation> oDataRequests, out string odatabatchClientResponseString, int maxretry, int batchSize, int retryDelay, string uniqueKey, ILogger errorLogger, string transactionId = "1")
        {
            List<bool> successList = new List<bool>();
            int retried = 0;
            bool retryRequired = true;
            odatabatchClientResponseString = string.Empty;
            ODataBatchResponse odatabatchClientResponse = null;
            int batchStartPoint = 0;
            for (int j = batchStartPoint; j < oDataRequests.Count; j += batchSize)
            {
                retryRequired = true;
                var listOfLists = oDataRequests.Skip(j).Take(batchSize).ToList();
                while (retried < maxretry && retryRequired)
                {
                    retried++;
                    try
                    {
                        odatabatchClientResponse = ExecuteODataBatchRequest(SPGMI.OData.Client.Enums.ODataBatchOperationType.Changeset, listOfLists, transactionId);
                        odatabatchClientResponseString = odatabatchClientResponse.RawResponse;
                        if (!((odatabatchClientResponse.StatusCode == HttpStatusCode.OK || odatabatchClientResponse.StatusCode == 0 || ((int)odatabatchClientResponse.StatusCode >= 200 && (int)odatabatchClientResponse.StatusCode < 300)) && !odatabatchClientResponse.HasErrors))
                        {

                            if (!ODataHasErrors(odatabatchClientResponseString))
                                retryRequired = false;

                            errorLogger.LogDebug($"Retry {retried} Status: {(!retryRequired ? "success" : "fail")}  Odata RAW batch response {odatabatchClientResponse.RawResponse} for UniqueKey {uniqueKey}");
                            if (!retryRequired)
                            {
                                successList.Add(false);
                                break;
                            }
                        }
                        else
                        {
                            batchStartPoint += batchSize;
                            retryRequired = false;
                            successList.Add(true);
                        }
                    }
                    catch (AggregateException ex)
                    {
                        StringBuilder exMessage = new StringBuilder("Aggregate Exception : ");
                        StringBuilder stackTrace = new StringBuilder("Aggregate Exception StackTrace: ");
                        foreach (var item in ex.Flatten().InnerExceptions)
                        {
                            exMessage.Append(item.Message);
                            exMessage.Append(item.InnerException);
                            stackTrace.Append(item.StackTrace);
                        }

                        foreach (var item in ex.InnerExceptions)
                        {
                            exMessage.Append(item.Message);
                            stackTrace.Append(item.StackTrace);
                        }
                        odatabatchClientResponseString = $"Exception: {ex.Message} \n Detailed :{exMessage} \nStack Trace: {ex.StackTrace} \n Detailed : {stackTrace.ToString()} Key {uniqueKey}";
                        errorLogger.LogError(odatabatchClientResponseString);
                        Task.Delay(retryDelay).Wait();
                    }
                    catch (Exception ex)
                    {
                        odatabatchClientResponseString = $"Exception: {ex.Message}\nStack Trace: {ex.StackTrace} Key {uniqueKey}";
                        errorLogger.LogError(odatabatchClientResponseString);
                        Task.Delay(retryDelay).Wait();
                    }
                }

                if (retried >= maxretry && odatabatchClientResponse != null)
                {
                    errorLogger.LogError($"System exhausted max retry attempts. Odata raw batch response: {odatabatchClientResponse.RawResponse} Key {uniqueKey}");
                    successList.Add(false);
                }
            }
            bool anyBatchFailed = successList.Any(x => !x);
            return !anyBatchFailed; // return true if all batch executed Successfully. Else Return Success = false
        }

        /// <summary>
        /// Execution Method for Odata Batch Request.
        /// </summary>
        /// <param name="list">List of OData Bath Methods</param>
        /// <param name="transactionId">Transaction Id for OData Request</param>
        /// <returns></returns>
        public ODataBatchResponse ExecuteODataBatchRequest(SPGMI.OData.Client.Enums.ODataBatchOperationType operationType, List<IODataOperation> list, string transactionId, bool isreadOperation = false)
        {
            ODataBatchResponse oDataBatchResponse;
            if (list.Count > 0)
            {
                ODataBatch oDataBatch = new ODataBatch(operationType);
                list.ForEach(x => oDataBatch += x);
                if (isreadOperation)
                {
                    if (oDataBatch.RequestHeaders == null)
                        oDataBatch.RequestHeaders = new Dictionary<string, string>();

                    oDataBatch.RequestHeaders.Add("Prefer", "processing=parallel");
                }
                oDataBatchResponse = _helper.Execute<ODataBatchResponse>(oDataBatch).Result;
            }
            //else if (list.Count == 1)
            //{
            //	Logger.LogInformation($"TransactionId : {transactionId} Prepared Odata Method {list[0].Method} Request with Odata URL {list[0].PrepareURL()} \n Odata Request {list[0].PrepareRequest()}");

            //	var result = _helper.Execute<ODataResponseEntity>(list[0]).Result;
            //	oDataBatchResponse = new ODataBatchResponse
            //	{
            //		StatusCode = result.StatusCode,
            //		RawResponse = result.RawResponse
            //	};
            //	oDataBatchResponse.Content.Add(result);
            //}
            else
            {
                throw new ArgumentException("Data must be sent for CRUD Operation. Empty Odata Calls are not allowed", "list");
            }
            return oDataBatchResponse;
        }

        /// <summary>
        ///  Returns Odata Response with Get Request sent in Defined List Format
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="input">string Input</param>
        /// <returns>IEnumerable T</returns>
        public IEnumerable<T> GetDataList<T>(string input) where T : class
        {
            IEnumerable<T> returnObject = null;
            List<T> obj = new List<T>();

            var stringResponse = GetODataApiWithRetry(input);
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(stringResponse))
                {
                    JObject jsonResponse = JObject.Parse(stringResponse);
                    JToken valueJToken = jsonResponse["value"];
                    bool queryHasData = valueJToken.HasValues;

                    if (queryHasData)
                    {
                        returnObject = valueJToken.ToObject<IEnumerable<T>>();
                        obj.AddRange(returnObject);
                    }
                    if (stringResponse.Contains("nextLink"))
                    {
                        input = ((Newtonsoft.Json.Linq.JProperty)jsonResponse.Last).Value.Value<string>();
                        stringResponse = GetODataApiWithRetry(input);
                    }
                    else
                        stringResponse = null;
                }
                else
                    break;
            }
            return obj.AsEnumerable<T>();
        }

        /// <summary>
        ///  Returns Odata Response with Get Request sent in Defined List Format
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="input">string Input</param>
        /// <returns>IEnumerable T</returns>
        public T GetFirstRecord<T>(string input) where T : class
        {
            T returnObject = null;


            var stringResponse = GetODataApiWithRetry(input);
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(stringResponse))
                {
                    JObject jsonResponse = JObject.Parse(stringResponse);
                    JToken valueJToken = jsonResponse["value"];
                    bool queryHasData = valueJToken.HasValues;

                    if (queryHasData)
                    {
                        var result = valueJToken.ToObject<IEnumerable<T>>();
                        returnObject = result.FirstOrDefault();


                    }
                    if (stringResponse.Contains("nextLink"))
                    {
                        input = ((Newtonsoft.Json.Linq.JProperty)jsonResponse.Last).Value.Value<string>();
                        stringResponse = GetODataApiWithRetry(input);
                    }
                    else
                        stringResponse = null;
                }
                else
                    break;
            }
            return returnObject;
        }

        /// <summary>
        /// Returns Odata Response with Get Request sent
        /// </summary>
        /// <param name="query">pass string Query without Odata URL</param>
        /// <returns>HTTP Response</returns>
        public string GetODataApiWithRetry(string query)
        {
            int i = 0;
            string errorMessage = string.Empty;
            string response = string.Empty;
            bool successflag = false;

            while (!successflag && _maxRetry > i && (errorMessage == string.Empty || ODataHasErrors(errorMessage)))
            {
                errorMessage = string.Empty;
                if (i != 0)
                    Task.Delay(_haltTime).Wait();
                try
                {
                    response = _helper.GetODataAPI(query).Result;
                    successflag = true;
                }
                catch (AggregateException ae)
                {
                    successflag = false;
                    var flt = ae.Flatten();
                    errorMessage = string.Join("|", flt.InnerExceptions.Select(e => e.Message + " Stack Trace" + e.StackTrace));
                    Logger.LogError(errorMessage);
                }
                finally
                {
                    if (ODataHasErrors(response))
#pragma warning disable S1854 // Dead stores should be removed
                        successflag = false;
#pragma warning restore S1854 // Dead stores should be removed
#pragma warning disable S1854 // Dead stores should be removed
                    i++;
#pragma warning restore S1854 // Dead stores should be removed
                }

                if (successflag)
                    break;
            }
            if (!successflag)
#pragma warning disable S112 // General exceptions should never be thrown
                throw new Exception(errorMessage);
#pragma warning restore S112 // General exceptions should never be thrown
            return response;
        }

        /// <summary>
        ///  Returns Odata Response with Get Request sent in Defined List Format
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="input">string Input</param>
        /// <returns>IEnumerable T</returns>
        public string GetSingleKeyValue(string keyItemName, string query)
        {
            var stringResponse = GetODataApiWithRetry(query);
            string returnvalue = string.Empty;
            if (!string.IsNullOrWhiteSpace(stringResponse))
            {
                JObject jsonResponse = JObject.Parse(stringResponse);
                JToken valueJToken = jsonResponse["value"];
                bool queryHasData = valueJToken.HasValues;

                if (queryHasData)
                {
                    foreach (dynamic item in valueJToken)
                    {
                        returnvalue = Convert.ToString(item[keyItemName]);
                    }
                }
            }
            return returnvalue;
        }

        /// <summary>
        /// Check if the Odata Response has Error
        /// </summary>
        /// <param name="httpResponse">Odata Http Response</param>
        /// <returns>true if has errors and false is no error encountered</returns>
        public bool ODataHasErrors(string httpResponse)
        {
            return ErrorStringsForRetry.Any(x => httpResponse.Contains(x));
        }

        /// <summary>
        /// To allocate nextkey. Mainly used in case of Master/Detail ODataObject relationships.
        /// allocate key on the Master object and use that as PK on the master object and FK (Reference key) in the detail object
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<string> CreateNextKeyRequest(string inputName)
        {
            using (var client = new HttpClient(new HttpClientHandler()
            {
                Credentials = CredentialCache.DefaultCredentials
            }))
            {

                client.BaseAddress = new Uri($"{oDataEndpointUri}{inputName}/SNL.GetNextKey()");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");

                var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var odata = JsonConvert.DeserializeObject<OData>(stringResponse);
                    return odata.Value;
                }
                else return string.Empty;
            }
        }

        public class OData
        {
            [JsonProperty("@odata.context")]
            public string Metadata { get; set; }
            public string Value { get; set; }
        }

        public ODataBatchResponse OdataBatchReadParallel(List<IODataOperation> list, int maxretry, int batchSize, int retryDelay, string uniqueKey, ILogger errorLogger, string transactionId = "1")
        {
            List<bool> successList = new List<bool>();
            int retried = 0;
            bool retryRequired = true;

            ODataBatchResponse odatabatchClientResponse = null;
            int batchStartPoint = 0;
            for (int j = batchStartPoint; j < list.Count; j += batchSize)
            {
                retryRequired = true;
                var listOfLists = list.Skip(j).Take(batchSize).ToList();
                while (retried < maxretry && retryRequired)
                {
                    retried++;
                    try
                    {
                        odatabatchClientResponse = ExecuteODataBatchRequest(SPGMI.OData.Client.Enums.ODataBatchOperationType.None, listOfLists, transactionId, true);
                        var odatabatchClientResponseString = odatabatchClientResponse.RawResponse;
                        if (!((odatabatchClientResponse.StatusCode == HttpStatusCode.OK || odatabatchClientResponse.StatusCode == 0 || ((int)odatabatchClientResponse.StatusCode >= 200 && (int)odatabatchClientResponse.StatusCode < 300)) && !odatabatchClientResponse.HasErrors))
                        {

                            if (!ODataHasErrors(odatabatchClientResponseString))
                                retryRequired = false;

                            errorLogger.LogDebug($"Retry {retried} Status: {(!retryRequired ? "success" : "fail")}  Odata RAW batch response {odatabatchClientResponse.RawResponse} for UniqueKey {uniqueKey}");
                            if (!retryRequired)
                            {
                                successList.Add(false);
                                break;
                            }
                        }
                        else
                        {
                            batchStartPoint += batchSize;
                            retryRequired = false;
                            successList.Add(true);
                        }
                    }
                    catch (AggregateException ex)
                    {
                        StringBuilder exMessage = new StringBuilder("Aggregate Exception : ");
                        StringBuilder stackTrace = new StringBuilder("Aggregate Exception StackTrace: ");
                        foreach (var item in ex.Flatten().InnerExceptions)
                        {
                            exMessage.Append(item.Message);
                            exMessage.Append(item.InnerException);
                            stackTrace.Append(item.StackTrace);
                        }

                        foreach (var item in ex.InnerExceptions)
                        {
                            exMessage.Append(item.Message);
                            stackTrace.Append(item.StackTrace);
                        }

                        errorLogger.LogError($"Exception: {ex.Message} \n Detailed :{exMessage} \nStack Trace: {ex.StackTrace} \n Detailed : {stackTrace.ToString()} Key {uniqueKey}");
                        Task.Delay(retryDelay).Wait();
                    }
                    catch (Exception ex)
                    {
                        errorLogger.LogError($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace} Key {uniqueKey}");
                        Task.Delay(retryDelay).Wait();
                    }
                }

                if (retried > maxretry && odatabatchClientResponse != null)
                {
                    errorLogger.LogError($"System exhausted max retry attempts. Odata raw batch response: {odatabatchClientResponse.RawResponse} Key {uniqueKey}");
                    successList.Add(false);
                }
            }
            return odatabatchClientResponse;
        }

        public IEnumerable<T> GetDataListFromJson<T>(string stringResponse) where T : class
        {
            IEnumerable<T> returnObject = null;
            List<T> obj = new List<T>();

            while (true)
            {
                if (!string.IsNullOrWhiteSpace(stringResponse))
                {
                    JObject jsonResponse = JObject.Parse(stringResponse);
                    JToken valueJToken = jsonResponse["value"];
                    bool queryHasData = valueJToken.HasValues;

                    if (queryHasData)
                    {
                        returnObject = valueJToken.ToObject<IEnumerable<T>>();
                        obj.AddRange(returnObject);

                    }
                    if (stringResponse.Contains("nextLink"))
                    {
                        var input = ((Newtonsoft.Json.Linq.JProperty)jsonResponse.Last).Value.Value<string>();
                        var result = GetDataList<T>(input);
                        obj.AddRange(result);
                    }

                    stringResponse = null;
                }
                else
                    break;
            }
            return obj.AsEnumerable<T>();
        }

        public T GetFirstRecordFromJson<T>(string stringResponse) where T : class
        {
            T returnObject = null;

            while (true)
            {
                if (!string.IsNullOrWhiteSpace(stringResponse))
                {
                    JObject jsonResponse = JObject.Parse(stringResponse);
                    JToken valueJToken = jsonResponse["value"];
                    bool queryHasData = valueJToken.HasValues;

                    if (queryHasData)
                    {
                        var result = valueJToken.ToObject<IEnumerable<T>>();
                        returnObject = result.FirstOrDefault();
                        stringResponse = null;

                    }
                    else
                        stringResponse = null;
                }
                else
                    break;
            }
            return returnObject;
        }
    }
}
