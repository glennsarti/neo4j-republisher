using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.IO;

namespace GlennSarti.Neo4jRepublisher
{
  public class RepublishHandler : System.Net.Http.DelegatingHandler
  {
    private string _Neo4jServerRootURL = null;
    private string _ReadWriteCredential = null;
    private string _ReadOnlyCredential = null;

    public RepublishHandler()
    {
      Debug.WriteLine("Constructor");

      _Neo4jServerRootURL = WebSiteConfiguration.Neo4jServerRootURL;
      _ReadWriteCredential = WebSiteConfiguration.ReadWriteCredential;
      _ReadOnlyCredential = WebSiteConfiguration.ReadonlyCredential;
    }

    private HttpResponseMessage ThrowHTTPError(HttpStatusCode errorCode, string errorText = "")
    {

      var response = new HttpResponseMessage(errorCode)
      {
        Content = new StringContent(errorText)
      };

      return response;
    }

    protected async override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      Debug.WriteLine("Process request");

      // TODO I don't like using dynamic types.  There must be a way to static type this!
      dynamic requestContext = request.Properties["MS_RequestContext"];
      System.Security.Principal.IIdentity requestIdentity = null;
      try
      {
        requestIdentity = requestContext.Principal.Identity;
      }
      catch (Exception ex)
      {
        return ThrowHTTPError(HttpStatusCode.BadRequest, ex.ToString());
      }

      // Fatal sanity checks
      if (!requestIdentity.IsAuthenticated)
        return ThrowHTTPError(HttpStatusCode.Forbidden, "User not authenticated");
      if (!(requestIdentity is WindowsIdentity))
        return ThrowHTTPError(HttpStatusCode.Forbidden, "User not windows authenticated");
      Debug.WriteLine("Request is from user " + requestIdentity.Name);

      // Get contextual information
      string originalPath = request.RequestUri.AbsolutePath;
      string originalRoot = request.RequestUri.Scheme + "://" + request.RequestUri.Authority;
      string remoteURL = _Neo4jServerRootURL + originalPath;
      Debug.WriteLine("Received request for " + originalRoot + "/" + originalPath + ".  Redirecting to " + remoteURL);

      // TODO: Deny WebAdmin etc.  Only allow /db/data ?

      // TODO: Implement group based authentication
      WindowsPrincipal User = new WindowsPrincipal((WindowsIdentity)requestIdentity);
      try
      {
        if ((User.IsInRole(@"BUILTIN\Administrators")) || (User.IsInRole(@"Domain\GroupName")))
        {
          Debug.WriteLine("User is allowed");
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error occured enumerating user roles " + ex.ToString());
        return ThrowHTTPError(HttpStatusCode.InternalServerError, "Error enumerating user role");
      }

      // Check if the response accepts XML
      // TODO Change this to use a specific header, not ContentType 
      bool wantsXMLResponse = false;
      //if (request.Headers.Accept != null)
      //  foreach (System.Net.Http.Headers.MediaTypeWithQualityHeaderValue acceptType in request.Headers.Accept)
      //  {
      //    wantsXMLResponse = wantsXMLResponse || (acceptType.MediaType.ToLower() == "application/xml");
      //  }
      if (wantsXMLResponse)
        Debug.WriteLine("Request would like the response back in XML format");

      // TODO Use HTTPRequestMessage and HTTPResponseMessage. Create a proxied request
      // http://stackoverflow.com/questions/18000583/re-send-httprequestmessage-exception
      // http://stackoverflow.com/questions/21467018/how-to-forward-an-httprequestmessage-to-another-server
      //HttpRequestMessage hr = request.C  <--- break Me
      HttpWebRequest objNeoRequest = (HttpWebRequest)WebRequest.Create(remoteURL);
      objNeoRequest.Method = request.Method.Method;
      if (request.Method == HttpMethod.Get)
      {
        objNeoRequest.ContentType = null;
        objNeoRequest.ContentLength = 0;
      }
      else
      {
        objNeoRequest.ContentType = request.Content.Headers.ContentType.MediaType;
        objNeoRequest.ContentLength = (long)request.Content.Headers.ContentLength;
      }
      objNeoRequest.AllowAutoRedirect = false;

      // Auth header MUST be added before body content otherwise the auth header is stripped
      // TODO: extract these credentials out
      byte[] authBytes = System.Text.Encoding.UTF8.GetBytes(_ReadWriteCredential.ToCharArray());
      objNeoRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);

      //  Add X-Forwarded-Host and X-Forwarded-Proto headers
      objNeoRequest.Headers["X-Forwarded-Host"] = request.RequestUri.Authority;
      objNeoRequest.Headers["X-Forwarded-Proto"] = request.RequestUri.Scheme;

      // Duplicate the request content if required
      if (request.Content.Headers.ContentLength > 0)
      {
        Stream dataStream = objNeoRequest.GetRequestStream();
        await request.Content.CopyToAsync(dataStream);
        dataStream.Close();
        dataStream = null;
      }

      HttpWebResponse objNeoResponse = null;
      HttpResponseMessage response = request.CreateResponse();
      try
      {
        Debug.WriteLine("Sending request to Neo ...");
        objNeoResponse = (HttpWebResponse)objNeoRequest.GetResponse();
        Debug.WriteLine("Received response from Neo");

        // Copy the stream
        //Stream outStream = response.Content. context.Response.OutputStream;
        if ((objNeoResponse.ContentType.StartsWith("application/json")) && wantsXMLResponse)
        {
          //TODO Need to do the JSON conversion still
          //string responseString = "{\r\n\"response\": \r\n" + StreamToString(objResponse.GetResponseStream()) + "\r\n}";

          //Debug.WriteLine("Converting JSON to XML...");
          //System.Xml.XmlDocument xmlDocument = new XmlDocument();
          //XmlDeclaration declaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes");
          //xmlDocument.AppendChild(declaration);
          //var xml = JsonConvert.DeserializeXmlNode(responseString);
          //var root = xmlDocument.ImportNode(xml.DocumentElement, true);
          //xmlDocument.AppendChild(root);

          //Debug.WriteLine("Changing content type to XML");
          //context.Response.ContentType = "application/xml";
          //using (Stream responseStream = StringToStream(xmlDocument.OuterXml))
          //{
          //  responseStream.CopyTo(outStream);
          //}
        }
        else
        {
          // Copy the content type and repsonse
          // TODO Can't get the StreamContent type to work properly.  For the moment, just convert it to a string and send that.  VERY ineffecient but meh.
          //response.Content = new StreamContent(objNeoResponse.GetResponseStream());
          response.Content = new StringContent(StreamToString(objNeoResponse.GetResponseStream()));

          System.Net.Http.Headers.MediaTypeHeaderValue tempMthv;
          if (System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(objNeoResponse.ContentType, out tempMthv))
          {
            response.Content.Headers.ContentType = tempMthv;
          }

          //response.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(objNeoResponse.ContentType);
          Debug.WriteLine("Copying response content type of " + objNeoResponse.ContentType + " of " + objNeoResponse.ContentLength.ToString() + " bytes");
        }

        // Copy the status code
        response.StatusCode = objNeoResponse.StatusCode;
      }
      catch (System.Net.WebException ex)
      {
        return ThrowHTTPError(HttpStatusCode.InternalServerError, ex.ToString());
      }
      catch (Exception ex)
      {
        return ThrowHTTPError(HttpStatusCode.InternalServerError, ex.ToString());
      }
      finally
      {
        // Cleanup
        objNeoRequest = null;
        if (objNeoResponse != null)
          objNeoResponse.Close();
        objNeoResponse = null;
      }



      //response.Content = new StringContent("Hello!");

      // -----------------------------------------------

      // Create the response.
      //response = new HttpResponseMessage(HttpStatusCode.OK)
      //{
      //  Content = new StringContent("Hello!")
      //};

      // Note: TaskCompletionSource creates a task that does not contain a delegate.
      //var tsc = new TaskCompletionSource<HttpResponseMessage>();
      //tsc.SetResult(response);   // Also sets the task state to "RanToCompletion"
      return response; // tsc.Task;      
      
      
      //// Call the inner handler.
      //var response = await base.SendAsync(request, cancellationToken);
      //Debug.WriteLine("Process response");
      //return response;
    }

    private string StreamToString(Stream stream)
    {
      if (stream.CanSeek) { stream.Position = 0; }
      using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
      {
        return reader.ReadToEnd();
      }
    }
  
  }
}