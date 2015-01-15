using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GlennSarti.Neo4jRepublisher.Models;
using System.Security.Principal;
using System.Web.Routing;

namespace GlennSarti.Neo4jRepublisher.Controllers
{
    public class StoredProcController : ApiController
    {
      private WindowsPrincipal GetWindowsIdentity()
      {
        // TODO I don't like using dynamic types.  There must be a way to static type this!
        try
        {
          dynamic requestContext = Url.Request.Properties["MS_RequestContext"];
          System.Security.Principal.IIdentity requestIdentity = requestContext.Principal.Identity;

          if (!(requestIdentity is WindowsIdentity))
            return null;
          return new WindowsPrincipal((WindowsIdentity)requestIdentity);                
        }
        catch
        {
          return null;
        }
      }

      // GET: /storedproc/
      [HttpGet]
      public Models.StoredProcedureList Get()
      {
        StoredProcedureList value = GlennSarti.Neo4jRepublisher.NeoStoredProcedures.NeoStoredProcedureRepository.AllowedStoredProcList(GetWindowsIdentity());

        // Modify the href links
        HttpRequestMessage request = Url.Request;
        foreach (StoredProcedure objProc in value.storedprocedures )
        {
          objProc.href = Url.Link("DefaultApi", new { id = objProc.name });
        }

        return value;
      }


      // GET: /storedproc/id
      [HttpGet]
      public Models.StoredProcedure Get(string id)
      {
        StoredProcedure value = GlennSarti.Neo4jRepublisher.NeoStoredProcedures.NeoStoredProcedureRepository.GetStoredProcedure(id, GetWindowsIdentity());

        // Modify the href links
        HttpRequestMessage request = Url.Request;
        value.href = Url.Link("DefaultApi", new { id = value.name });

        return value;
      }

      private void GetRequestParametersForNeo(
        ref GlennSarti.Neo4jRepublisher.NeoStoredProcedures.RequestParameterCollections requestParms,
        WindowsPrincipal requestUser)
      {
        requestParms.InternalParameters.Add("username", "");
        requestParms.InternalParameters.Add("userdomain", "");
        requestParms.InternalParameters.Add("samaccount", "");

        DateTime dteNow = DateTime.Now;
        DateTime dteUtcNow = DateTime.UtcNow;

        requestParms.InternalParameters.Add("now-local", dteNow.ToString("yyyy-MM-ddTHH:mm:ssZK"));
        requestParms.InternalParameters.Add("now-local-rfc1123", dteNow.ToString("r"));
        requestParms.InternalParameters.Add("now-local-sortable", dteNow.ToString("s"));
        requestParms.InternalParameters.Add("now-local-univsortable", dteNow.ToString("u"));

        requestParms.InternalParameters.Add("now-utc", dteUtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        requestParms.InternalParameters.Add("now-utc-rfc1123", dteUtcNow.ToString("r"));
        requestParms.InternalParameters.Add("now-utc-sortable", dteUtcNow.ToString("s"));
        requestParms.InternalParameters.Add("now-utc-univsortable", dteUtcNow.ToString("u"));

        if (requestUser != null)
        {
          requestParms.InternalParameters["sAMAccount"] = requestUser.Identity.Name.ToUpper();
          requestParms.InternalParameters["username"] = requestParms.InternalParameters["sAMAccount"];
          string[] userName = requestUser.Identity.Name.ToUpper().Split('\\');
          if (userName.GetUpperBound(0) > 0)
          {
            requestParms.InternalParameters["userdomain"] = userName[0];
            requestParms.InternalParameters["username"] = userName[1];
          }
        }
      }

      
      // POST: /storedproc/id
      // I'm using the NakedBody extension because the parsing of a Json payload into a dictionary object is almost impossible.  This is due to the way WebAPI does payload parsing out of the box
      // Instead, I'm grabbing the entire payload and then attempt to convert that into a dictionary.  Any errors cause all params to be invalid (i.e. null)
      [HttpPost]
      public async System.Threading.Tasks.Task<string> Post([NakedBody]string raw)
      {
        System.Web.Http.Routing.IHttpRouteData routeData = Request.GetRouteData();
        string id = routeData.Values["id"].ToString();

        // Try to convert the payload into a dictionary object;
        GlennSarti.Neo4jRepublisher.NeoStoredProcedures.RequestParameterCollections paramCollection = new NeoStoredProcedures.RequestParameterCollections();
        if ((raw != null) && (raw != ""))
        {
          paramCollection.ExternalParameters = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(raw);
        }

        WindowsPrincipal userPrincipal = GetWindowsIdentity();
        GetRequestParametersForNeo(ref paramCollection, userPrincipal);

        //TODO Need to figure out how to send this data back better
        //  Need to hook into the republisher handler better
        return await GlennSarti.Neo4jRepublisher.NeoStoredProcedures.NeoStoredProcedureRepository.ExecuteStoredProcedure(id,paramCollection, userPrincipal, Request);
      }
    }
}
