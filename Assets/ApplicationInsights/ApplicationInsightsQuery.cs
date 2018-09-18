using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityApplicationInsights
{
    /// <summary>
    /// A sample Application Insights Query script
    /// </summary>
    public class ApplicationInsightsQuery : MonoBehaviour
    {
        /// <summary>
        /// Insert your Application Insights App Id here
        /// </summary>
        [SerializeField]
        internal string applicationId = "";

        /// <summary>
        /// Insert your Application Insights API (read-only) key here
        /// </summary>
        [SerializeField]
        internal string API_Key = "";

        /// <summary>
        /// Number of hours to query
        /// </summary>
        public int TimepspanAsHours = 24;

        /// <summary>
        /// Container for holograms
        /// </summary>
        public GameObject Root;

        public bool Refresh = true;
        /// <summary>
        /// Update interval in seconds
        /// </summary>
        [Range(1, 300), Tooltip("Update interval in seconds")]
        public int UpdateInterval = 60; // seconds
        private float timer = 0.0f;

        /// <summary>
        /// Kusto query. 
        /// You can test the query string in https://dev.applicationinsights.io/apiexplorer/postQuery
        /// </summary>
        [TextArea(5,10)]
        public string Query = "customEvents " +
            "\n| project timestamp, name, target=tostring(customDimensions.target) " +
            "\n| where name=='Near' " +
            "\n| summarize total = count() by target " +
            "\n| top 10 by total desc";

        void Awake()
        {
            if (string.IsNullOrEmpty(applicationId) || string.IsNullOrEmpty(API_Key))
            {
                Debug.LogWarning("Application Insights App Id and Key are required for Application Insights Query script.");
                this.enabled = false;
                return;
            }
        }

        void Start()
        {
            FetchAnalytics();
        }


        void Update()
        {
            if (!Refresh)
            {
                return;
            }
            timer += Time.deltaTime;
            if (timer > UpdateInterval)
            {
                FetchAnalytics();
                timer = 0;
            }
        }

        private void FetchAnalytics()
        {
            if (string.IsNullOrEmpty(Query))
            {
                Debug.LogWarning("No query");
                return;
            } 
            Debug.Log("Fetch Analytics: \n" + Query);
            // Application Insights Kusto query    
            StartCoroutine(SendQuery(Query.Replace("\n","")));
        }

        private IEnumerator SendQuery(string query)
        {
            string queryUrl = Uri.EscapeUriString(string.Format("https://api.applicationinsights.io/v1/apps/{0}/query?timespan=PT{1}H", applicationId.Trim(), TimepspanAsHours));
            string body = "{\"query\": \"" + query + "\"}";
            Debug.Log(queryUrl + "\n" + body);

            using (UnityWebRequest unityWebRequest = new UnityWebRequest(queryUrl, UnityWebRequest.kHttpVerbPOST))
            {
                unityWebRequest.SetRequestHeader("X-Api-Key", API_Key.Trim());
                unityWebRequest.SetRequestHeader("Content-Type", "application/json");
                unityWebRequest.SetRequestHeader("Accept", "application/json");
                unityWebRequest.chunkedTransfer = false;
                unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                byte[] data = Encoding.UTF8.GetBytes(body);
                unityWebRequest.uploadHandler = new UploadHandlerRaw(data);

                yield return unityWebRequest.SendWebRequest();

                if (unityWebRequest.isNetworkError)
                {
                    // Failure with web request.
                    Debug.LogError("<color=red>Error Sending:</color> " + unityWebRequest.error);
                    yield break;
                }

                QueryResult<RowTargetTotal> result;
                try
                {
                    result = JsonConvert.DeserializeObject<QueryResult<RowTargetTotal>>(unityWebRequest.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    Debug.LogError("<color=red>Failed to parse query result:</color> " + ex.Message);
                    yield break;
                }
                if (result != null)
                {
                    ProcessResult(result);
                }
            }
        }

        private void ProcessResult(QueryResult<RowTargetTotal> result)
        {
            if (result != null && result.tables.Length > 0 && result.tables[0].rows.Length > 0)
            {
                string target = result.tables[0].rows[0].target;
                int total = result.tables[0].rows[0].total;
                Debug.LogFormat("<color=magenta>Top target name: {0} total: {1}</color>", target, total);

                if (Root == null) return;

                Transform child;
                for (var i = 0; i < Root.transform.childCount; i++)
                {
                    child = Root.transform.GetChild(i);
                    if (string.Equals(child.name, target))
                    {
                        child.gameObject.GetComponent<Renderer>().material.color = Color.magenta;
                    }
                }
            }
        }
    }
}
