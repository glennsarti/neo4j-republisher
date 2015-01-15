using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GlennSarti.Neo4jRepublisher.Models
{
  [DataContract(Name = "storedprocedurelist", Namespace = "")]
  [XmlRoot(ElementName = "storedprocedurelist",Namespace="")]
  public class StoredProcedureList
  {
    [DataMember]
    [XmlArray(ElementName="storedprocedures")]
    [XmlArrayItem(ElementName="storedprocedure")]
    public StoredProcedure[] storedprocedures { get; set; }
  }
}