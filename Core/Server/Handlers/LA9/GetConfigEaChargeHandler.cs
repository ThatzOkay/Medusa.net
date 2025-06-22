using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.LA9
{
    public class GetConfigEaChargeHandler(XDocument body): Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("eacharge");
            Method("getconf");
        }

        public override XDocument Handle(string model)
        {
            var articlerevision = new XElement("articlerevision", 1, new XAttribute("__type", "str"));

            var articletype1 = new XElement("articletype", "DOCMT_0000", new XAttribute("__type", "str"));
            var articledoc1 = new XElement("articledoc", new XAttribute("__type", "str"));
            var item1 = new XElement("item", articletype1, articledoc1);

            var articletype2 = new XElement("articletype", "DOCMT_0001", new XAttribute("__type", "str"));
            var articledoc2 = new XElement("articledoc", new XAttribute("__type", "str"));
            var item2 = new XElement("item", articletype2, articledoc2);

            var articletype3 = new XElement("articletype", "DOCMT_0002", new XAttribute("__type", "str"));
            var articledoc3 = new XElement("articledoc", new XAttribute("__type", "str"));
            var item3 = new XElement("item", articletype3, articledoc3);

            var articletype4 = new XElement("articletype", "DOCMT_0003", new XAttribute("__type", "str"));
            var articledoc4 = new XElement("articledoc", new XAttribute("__type", "str"));
            var item4 = new XElement("item", articletype4, articledoc4);

            var articletype5 = new XElement("articletype", "URL_0001_A", new XAttribute("__type", "str"));
            var articledoc5 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item5 = new XElement("item", articletype5, articledoc5);

            var articletype6 = new XElement("articletype", "URL_0001_B", new XAttribute("__type", "str"));
            var articledoc6 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item6 = new XElement("item", articletype6, articledoc6);

            var articletype7 = new XElement("articletype", "URL_0002_A", new XAttribute("__type", "str"));
            var articledoc7 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item7 = new XElement("item", articletype7, articledoc7);

            var articletype8 = new XElement("articletype", "URL_0002_B", new XAttribute("__type", "str"));
            var articledoc8 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item8 = new XElement("item", articletype8, articledoc8);

            var articletype9 = new XElement("articletype", "URL_0003_A", new XAttribute("__type", "str"));
            var articledoc9 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item9 = new XElement("item", articletype9, articledoc9);

            var articletype10 = new XElement("articletype", "URL_0003_B", new XAttribute("__type", "str"));
            var articledoc10 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item10 = new XElement("item", articletype10, articledoc10);

            var articletype11 = new XElement("articletype", "URL_0004_A", new XAttribute("__type", "str"));
            var articledoc11 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item11 = new XElement("item", articletype11, articledoc11);

            var articletype12 = new XElement("articletype", "URL_0004_B", new XAttribute("__type", "str"));
            var articledoc12 = new XElement("articledoc", "http://eagate.573.jp", new XAttribute("__type", "str"));
            var item12 = new XElement("item", articletype12, articledoc12);

            var articletype13 = new XElement("articletype", "PASELIUNIT", new XAttribute("__type", "str"));
            var articledoc13 = new XElement("articledoc", "P", new XAttribute("__type", "str"));
            var item13 = new XElement("item", articletype13, articledoc13);

            var articlelist = new XElement("articlelist", item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12, item13);
            var chargearticle = new XElement("chargearticle", articlerevision, articlelist);

            var chargedaily = new XElement("chargedaily", 0, new XAttribute("__type", "s32"));
            var chargedailylimit = new XElement("chargedailylimit", 100000, new XAttribute("__type", "s32"));
            var chargedetail = new XElement("chargedetail", chargedaily, chargedailylimit);

            var currentcode = new XElement("currentcode", 0, new XAttribute("__type", "s32"));
            var lastchargestatus = new XElement("lastchargestatus", 1, new XAttribute("__type", "s32"));
            var chargestatus = new XElement("chargestatus", currentcode, lastchargestatus);

            var eacharge = new XElement("eacharge",
                new XAttribute("status", "0"), new XAttribute("expire", "1200"), chargearticle, chargedetail, chargestatus);

            var document = new XDocument(
                new XElement("response",
                    eacharge
                )
            );

            return document;
        }
    }
}
