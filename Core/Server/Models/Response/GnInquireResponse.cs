using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("cardmng")]
[Serializable]
public class GnInquireResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
    
    [XmlAttribute("binded")]
    public int Binded { get; set; }

    [XmlAttribute("dataid")] 
    public string DataId { get; set; } = "";
    
    [XmlAttribute("ecflag")]
    public int EcFlag { get; set; }

    [XmlAttribute("expired")]
    public int Expired { get; set; }
    
    [XmlAttribute("newflag")]
    public string NewFlag { get; set; } = "";

    [XmlAttribute("isgenuine")]
    public string IsGeniune { get; set; } = "";


    [XmlAttribute("altstatus")]
    public string AltStatus { get; set; } = "";

    [XmlAttribute("articleno")]
    public string ArticleNo { get; set; } = "";

    [XmlAttribute("extidflag")]
    public string ExtidFlag { get; set; } = "";

    [XmlAttribute("refid")] 
    public string RefId { get; set; } = "";
    
    [XmlAttribute("useridflag")]
    public string UserIdFlag { get; set; } = "";
}