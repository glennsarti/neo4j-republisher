using System.Web;
using System.Web.Mvc;

namespace GlennSarti.Neo4jRepublisher
{
  public class FilterConfig
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }
  }
}
