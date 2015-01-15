using System.Diagnostics;
using System.Net.Http;
using System.Security.Principal;
using System.Xml;

using System.Runtime.Caching;


namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public static class NeoStoredProcedureRepository
  {

    // TODO public static void InvalidateStoredProcConfig() <--- Do I need this?

    private static StoredProcedureList GetStoredProcConfiguration()
    {
      RepublisherCache objCache = new RepublisherCache();
      StoredProcedureList storedConfig = objCache.GetItem("StoredProcedureConfiguration") as StoredProcedureList;
      if (storedConfig != null)
        return storedConfig;

      // Item wasn't in the cache, read it
      XmlDocument xmlDoc = new XmlDocument();
      Debug.WriteLine("Reading XML Storedproc config...");
      xmlDoc.Load(GlennSarti.Neo4jRepublisher.WebSiteConfiguration.StoredProcedureConfigurationXMLFile);

      storedConfig = new StoredProcedureList();
      foreach (XmlElement xmlNode in xmlDoc.SelectNodes("/storedprocedures/storedprocedure"))
      {
        StoredProcedure value = new StoredProcedure();
        value.Initialise(xmlNode);
        storedConfig.Add(value.Name, value);
        value = null;
      }
      
      // Add it to the cache
      objCache.AddItem("StoredProcedureConfiguration", storedConfig, 10);

      return storedConfig;
    }

    public static GlennSarti.Neo4jRepublisher.Models.StoredProcedureList AllowedStoredProcList(WindowsPrincipal requestUser)
    {
      StoredProcedureList storedConfig  = GetStoredProcConfiguration();
      if (storedConfig == null)
        throw new System.Exception("Error getting stored procedure configuration");

      GlennSarti.Neo4jRepublisher.Models.StoredProcedureList value = new Models.StoredProcedureList();
      System.Collections.Generic.List<Models.StoredProcedure> procs = new System.Collections.Generic.List<Models.StoredProcedure>();

      foreach (System.Collections.Generic.KeyValuePair<string, StoredProcedure> objProc in storedConfig)
        if (objProc.Value.IsAllowed(requestUser))
          procs.Add(objProc.Value.GetAsModel());
      value.storedprocedures = procs.ToArray();

      return value;
    }

    public static GlennSarti.Neo4jRepublisher.Models.StoredProcedure GetStoredProcedure(string storedProcName, WindowsPrincipal requestUser)
    {
      StoredProcedureList storedConfig = GetStoredProcConfiguration();
      if (storedConfig == null)
        throw new System.Exception("Error getting stored procedure configuration");

      if (storedConfig.ContainsKey(storedProcName))
      {
        if (!storedConfig[storedProcName].IsAllowed(requestUser))
          throw new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
        return storedConfig[storedProcName].GetAsModel();
      }
      else
      {
        throw new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
      }
    }

    public static async System.Threading.Tasks.Task<string> ExecuteStoredProcedure(string storedProcName, RequestParameterCollections requestProperties, WindowsPrincipal requestUser, HttpRequestMessage originalRequest)
    {
      StoredProcedureList storedConfig = GetStoredProcConfiguration();
      if (storedConfig == null)
        throw new System.Exception("Error getting stored procedure configuration");

      if (storedConfig.ContainsKey(storedProcName))
      {
        StoredProcedure objProc = storedConfig[storedProcName];
        if (!objProc.IsAllowed(requestUser))
          throw new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
        // Check if supplied params are valid

        // Execute the stored proc
        // TODO Should make this URL part of the config file instead of hardcard
        string remoteURL = GlennSarti.Neo4jRepublisher.WebSiteConfiguration.Neo4jServerRootURL + "/db/data/transaction/commit";

        // -= -= -= -= -= -= -= -= -=
        HttpRequestMessage neoRequest = new HttpRequestMessage(HttpMethod.Post,remoteURL);

        // Auth header MUST be added before body content otherwise the auth header is stripped
        byte[] authBytes = System.Text.Encoding.UTF8.GetBytes(GlennSarti.Neo4jRepublisher.WebSiteConfiguration.ReadWriteCredential.ToCharArray());
        neoRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",System.Convert.ToBase64String(authBytes));

        //  Add X-Forwarded-Host and X-Forwarded-Proto headers
        neoRequest.Headers.Add("X-Forwarded-Host", originalRequest.RequestUri.Authority);
        neoRequest.Headers.Add("X-Forwarded-Proto", originalRequest.RequestUri.Scheme);

        // Add content
        neoRequest.Content = new StringContent(objProc.GetJSONBody(requestProperties), System.Text.Encoding.UTF8, "application/json");

        // Send it
        HttpClient httpClient = new HttpClient();
        HttpResponseMessage neoResponse = await httpClient.SendAsync(neoRequest);

        string result = await neoResponse.Content.ReadAsStringAsync();
        return result;
        // -= -= -= -= -= -= -= -= -=
      }
      else
      {
        throw new System.Web.Http.HttpResponseException(System.Net.HttpStatusCode.BadRequest);
      }
    }

  }
}