using System.Collections.Generic;
using System.Xml.Serialization;
using Server.Models;

namespace Server.Models.Response;

[XmlRoot("package")]
[Serializable]
public class ListPackageResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlAttribute("expire")]
    public required int Expire { get; set; }

    [XmlElement("item")]
    public List<PackageEntry> Items { get; set; } = [];
}