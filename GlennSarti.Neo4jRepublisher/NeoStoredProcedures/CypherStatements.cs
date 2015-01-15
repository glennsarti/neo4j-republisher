namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public class CypherStatements
  {
    public CypherStatement[] statements { get; set; }

    public CypherStatements Clone()
    {
      CypherStatements objClone = null;

      // TODO This is kind of crappy and probably not very performant.  Need to do a better way of cloning
      string serial = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
      objClone = Newtonsoft.Json.JsonConvert.DeserializeObject<CypherStatements>(serial);

      return objClone;
    }
  }

}
