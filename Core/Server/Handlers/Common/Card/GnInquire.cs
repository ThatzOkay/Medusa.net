using Abstractions.Handlers;
using System.Xml.Linq;

namespace Server.Handlers.Common.Card;

public class GnInquire : HandlerWithoutRequest<XDocument>
{
    public override void Configure()
    {
        Module("cardmng");
        Method("gninquire");
    }

    public override XDocument Handle(string model) {
        var response = new XDocument(
            new XElement("response",
                new XElement("status", "0"),
                new XElement("data",
                    new XElement("inquire",
                        new XElement("card",
                            new XElement("cardid", "1234567890123456"),
                            new XElement("konamiid", "KONAMI123456"),
                            new XElement("profileexists", "1")
                        )
                    )
                )
            )
        );
        return response;
    }
}
