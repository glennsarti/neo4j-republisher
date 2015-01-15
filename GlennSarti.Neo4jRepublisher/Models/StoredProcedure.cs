using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GlennSarti.Neo4jRepublisher.Models
{
  [DataContract(Name = "storedprocedure", Namespace = "")]
  [XmlRoot(ElementName = "storedprocedure", Namespace = "")]
  public class StoredProcedure
  {
    [DataMember]
    public string name { get; set; }

    [DataMember]
    public string href { get; set; }

    [DataMember]
    [XmlArray(ElementName = "parameters")]
    [XmlArrayItem(ElementName = "parameter")]
    public StoredProcedureParameter[] parameters { get; set; }
  }
}