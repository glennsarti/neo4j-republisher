namespace GlennSarti.Neo4jRepublisher
{
  public static class WebSiteConfiguration
  {
    public static string Neo4jServerRootURL
    {
      // TODO Need to change to an environment variable
      get
      {
        return GetConfigSetting("Neo4jServerURL", "");
      }
    }

    public static string ReadWriteCredential
    {
      get
      {
        return GetConfigSetting("ReadWriteCredential", "this:willnotwork");
      }
    }
    public static string ReadonlyCredential
    {
      get
      {
        return GetConfigSetting("ReadOnlyCredential", "this:willnotwork");
      }
    }


    //public static bool ReuseClass
    //{
    //  get
    //  {
    //    return (GetConfigSetting("ReuseClass", "") == "true");
    //  }
    //}

    public static string StoredProcedureConfigurationXMLFile
    {
      // TODO Need to change to an environment variable
      get
      {
        return GetConfigSetting("StoredProcedureConfigurationXMLFile", "");
      }
    }

    private static string GetConfigSetting(string propertyName, string propertyDefault)
    {
      string value = System.Web.Configuration.WebConfigurationManager.AppSettings[propertyName];
      if (value == null)
        return propertyDefault;
      else
        return value;
    }
  }

}