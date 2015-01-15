using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GlennSarti.Neo4jRepublisher.Models
{
  [DataContract(Name = "param", Namespace = "")]
  [XmlRoot(ElementName = "param", Namespace = "")]
  public class StoredProcedureParameter
  {
    [DataMember]
    public string name { get; set; }
  }
}