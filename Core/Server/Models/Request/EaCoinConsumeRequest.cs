using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("eacoin")]
public class EaCoinConsumeRequest
{
    [XmlAttribute("esid")]
    public string EsId { get; set; }

    [XmlAttribute("esdate")]
    public string EsDate { get; set; }

    [XmlElement("sessid")]
    public string sessionId { get; set; }
    
    [XmlElement("sequence")]
    public short Sequence { get; set; }
    
    [XmlElement("payment")]
    public int Payment { get; set; }
    
    [XmlElement("service")]
    public short service { get; set; }
    
    [XmlElement("itemtype")]
    public string ItemType { get; set; }
    
    [XmlElement("detail")]
    public string Detail { get; set; }
}