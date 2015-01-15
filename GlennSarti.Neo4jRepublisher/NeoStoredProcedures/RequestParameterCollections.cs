namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public class RequestParameterCollections
  {
    public System.Collections.Generic.Dictionary<string, string> ExternalParameters { get; set; }
    public System.Collections.Generic.Dictionary<string, string> InternalParameters { get; set; }

    public RequestParameterCollections()
    {
      ExternalParameters = new System.Collections.Generic.Dictionary<string, string>();
      InternalParameters = new System.Collections.Generic.Dictionary<string, string>();
    }
  }

}