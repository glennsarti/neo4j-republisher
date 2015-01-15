namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public class CypherStatement
  {
    public string statement { get; set; }
    public CypherStatementParameters parameters { get; set; }

    public CypherStatement()
    {
      parameters = new CypherStatementParameters();
    }
  }

}