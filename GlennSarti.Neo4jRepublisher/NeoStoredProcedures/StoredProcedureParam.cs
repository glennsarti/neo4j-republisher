using System.Xml;

namespace GlennSarti.Neo4jRepublisher.NeoStoredProcedures
{
  public class StoredProcedureParam
  {
    public string Name { get; set; }

    public string ParamType { get; set; }

    public string ValidationRegEx { get; set; }

    public bool IsInternalParam { get; set; }

    public Models.StoredProcedureParameter GetAsModel()
    {
      Models.StoredProcedureParameter value = null;
      if (!IsInternalParam)
      {
        value = new Models.StoredProcedureParameter();
        value.name = Name;
      }
      return value;
    }

    public bool Initialise(XmlElement xmlNode)
    {
      return Initialise(xmlNode, "string");
    }
    public bool Initialise(XmlElement xmlNode, string internalParamType)
    {
      switch (xmlNode.Name)
      {
        case "param":
          XmlNode objMisc = xmlNode.SelectSingleNode("name");
          if (objMisc == null)
            return false;
          Name = objMisc.InnerText;

          // Validation regex
          objMisc = xmlNode.SelectSingleNode("regex");
          if (objMisc != null)
          {
            ValidationRegEx = objMisc.InnerText;
          }
          else
          {
            ValidationRegEx = null;
          }

          // Param type
          objMisc = xmlNode.SelectSingleNode("type");
          if (objMisc != null)
          {
            switch (objMisc.InnerText.ToLower())
            {
              case "int":
                ParamType = objMisc.InnerText.ToLower();
                break;
              default:
                throw new System.Exception("Unknown param type of " + objMisc.InnerText.ToLower());
            }
          }
          else
          {
            ParamType = "string";
          }
          
          IsInternalParam = false;
          break;
        case "internalparam":
          Name = xmlNode.InnerText;
          IsInternalParam = true;
          ParamType = internalParamType;
          ValidationRegEx = null;
          break;
        default:
          return false;
      }

      return true;
    }
  }
}