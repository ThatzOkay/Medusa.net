using System.Xml.Serialization;

namespace Server.Models;

[XmlRoot("item")]
[Serializable]
public class PackageEntry
{
    [XmlAttribute("url")]     public required string Url     { get; set; }
    [XmlAttribute("name")]    public required string Name    { get; set; }
    [XmlAttribute("desc")]    public required string Desc    { get; set; }
    [XmlAttribute("size")]    public required long   Size    { get; set; }
    [XmlAttribute("pkgtype")] public required string PkgType { get; set; }
    [XmlAttribute("sumtype")] public required string SumType { get; set; }
    [XmlAttribute("sum")]     public required string Sum     { get; set; }
    [XmlAttribute("from")]    public required long   From    { get; set; }
    [XmlAttribute("till")]    public required long   Till    { get; set; }
}
