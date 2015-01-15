using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace GlennSarti.Neo4jRepublisher
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      // Web API configuration and services

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "api/{controller}/{id}",
          defaults: new {id = RouteParameter.Optional }
      );

      // Republisher Routes
      config.Routes.MapHttpRoute(
          name: "RepublisherHandler",
          routeTemplate: "{*catchall}",
          defaults: null,
          constraints: null,
          handler: new GlennSarti.Neo4jRepublisher.RepublishHandler()
      );

      // Use the Xml Serializer not the DataContractSerializer
      config.Formatters.XmlFormatter.UseXmlSerializer = true;

    }
  }
}
