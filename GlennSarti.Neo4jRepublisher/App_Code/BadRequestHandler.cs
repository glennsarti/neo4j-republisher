//using System.Diagnostics;
//using System.Web;

//namespace GlennSarti.Neo4jRepublisher
//{

//  public class BadRequestHandler : IHttpHandler
//  {
//    public BadRequestHandler()
//    {
//      // Default constructor does not get called.  Don't put stuff here.
//    }

//    public void ThrowHTTPError(int errorCode, System.Web.HttpContext context, string errorText = "")
//    {
//      HttpResponse Response = context.Response;
//      Response.Clear();
//      Response.StatusCode = errorCode;
//      Debug.WriteLine("Threw error " + errorCode.ToString() + " because " + errorText);
//      return;
//    }


//    public void ProcessRequest(System.Web.HttpContext context)
//    {
//      ThrowHTTPError(400, context);
//      return;
//    }
//    public bool IsReusable
//    {
//      get
//      {
//        return true;
//      }
//    }
//  }
//}