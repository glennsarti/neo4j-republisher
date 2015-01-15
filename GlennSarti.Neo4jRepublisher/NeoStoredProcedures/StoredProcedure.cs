using System.Security.Principal;
using System.Xml;
using Newtonsoft.Json;

namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public class StoredProcedure
  {
    private CypherStatements _procCypherStatements = null;

    public string Name { get; set; }
    public System.Collections.Generic.IList<SecurityIdentifier> AllowedSIDs { get; set; }
    public System.Collections.Generic.IDictionary<string,StoredProcedureParam> QueryParameters { get; set; }

    private string GetParameterValueByName(string paramName, RequestParameterCollections parameterValues)
    {
      if (parameterValues.InternalParameters.ContainsKey(paramName.ToLower()))
        return parameterValues.InternalParameters[paramName.ToLower()];
      if (parameterValues.ExternalParameters.ContainsKey(paramName.ToLower()))
        return parameterValues.ExternalParameters[paramName.ToLower()];
      return null;
    }

    public string GetJSONBody(RequestParameterCollections parameterValues)
    {
      // TODO This process is probably ineffecient
      CypherStatements objStatements = _procCypherStatements.Clone();

      foreach(CypherStatement objStatement in objStatements.statements)
      {
        string[] paramKeys = new string[objStatement.parameters.Keys.Count];
        objStatement.parameters.Keys.CopyTo(paramKeys,0);

        foreach (string paramKey in paramKeys)
        {
          string paramValue = GetParameterValueByName(paramKey, parameterValues);
          if (paramValue == null)
            throw new System.Exception("Missing required parameter " + paramKey);

          switch (QueryParameters[paramKey].ParamType)
          {
            case "int":
              objStatement.parameters[paramKey] = int.Parse(paramValue);
              break;
            default:
              objStatement.parameters[paramKey] = paramValue;
              break;
          }
        }
      }

      string jsonBody = JsonConvert.SerializeObject(objStatements, Newtonsoft.Json.Formatting.Indented);
      return jsonBody;
    }

    public Models.StoredProcedure GetAsModel()
    {
      Models.StoredProcedure value = new Models.StoredProcedure();
      value.name = Name;

      System.Collections.Generic.List<Models.StoredProcedureParameter> parameters = new System.Collections.Generic.List<Models.StoredProcedureParameter>();
      foreach (string paramName in QueryParameters.Keys)
      {
        StoredProcedureParam objparam = QueryParameters[paramName];
        Models.StoredProcedureParameter objModel = objparam.GetAsModel();
        if (objModel != null)
          parameters.Add(objModel);
      }
      value.parameters = parameters.ToArray();

      return value;
    }

    public bool IsAllowed(WindowsPrincipal requestUser)
    {
      if (AllowedSIDs.Count == 0)
        return true;

      foreach (SecurityIdentifier procSid in AllowedSIDs)
      {
        if (requestUser.IsInRole(procSid))
          return true;
      }

      return false;
    }

    public bool Initialise(XmlElement xmlNode)
    {
      this.Name = xmlNode.Attributes["name"].Value;

      // Allowed SIDs
      foreach (XmlElement value in xmlNode.SelectNodes("allowsid"))
      {
        SecurityIdentifier objSid = new SecurityIdentifier(value.InnerText);
        AllowedSIDs.Add(objSid);
      }

      // Client provided params
      foreach (XmlElement value in xmlNode.SelectNodes("param"))
      {
        StoredProcedureParam objParam = new StoredProcedureParam();
        if (!objParam.Initialise(value))
          throw new System.Exception("Unable to parse storedprocedure parameter");
        QueryParameters.Add(objParam.Name,objParam);
      }
      // Server provided params
      foreach (XmlElement value in xmlNode.SelectNodes("internalparam"))
      {
        StoredProcedureParam objParam = new StoredProcedureParam();
        if (!objParam.Initialise(value))
          throw new System.Exception("Unable to parse storedprocedure parameter");
        QueryParameters.Add(objParam.Name,objParam);
      }

      // Convert the statements into a Neo Transactional DB Endpoint JSON payload template.
      System.Collections.Generic.List<CypherStatement> colStatements = new System.Collections.Generic.List<CypherStatement>();
      foreach (XmlElement statementElement in xmlNode.SelectNodes("statement"))
      {
        CypherStatement objMisc = new CypherStatement();
        objMisc.statement = statementElement.SelectSingleNode("cypher").InnerText;
        foreach(XmlElement paramElement in statementElement.SelectNodes("param"))
          objMisc.parameters.Add(paramElement.InnerText, null);
        colStatements.Add(objMisc);
      }
      CypherStatements objRoot = new CypherStatements();
      objRoot.statements = colStatements.ToArray();
      _procCypherStatements = objRoot;

      return true;
    }

    public StoredProcedure()
    {
      AllowedSIDs = new System.Collections.Generic.List<SecurityIdentifier>();
      QueryParameters = new System.Collections.Generic.Dictionary<string,StoredProcedureParam>();
    }
  }
}