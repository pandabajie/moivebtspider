using HtmlAgilityPack;
using System;
using System.Net;

namespace spider
{
    internal class MovieDetailInfo
    {
        public  static Movie getBtbaMovieInfo(string sourceUrl)
        {
            string strAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:2.0b13pre) Gecko/20110307 Firefox/4.0b13pre";
            HttpWebResponse resBtba = HttpHelper.CreateGetHttpResponse(sourceUrl, 300, strAgent, null);
            if (resBtba == null)
            {
                return null;
            }
            string htmlBtba = HttpHelper.GetResponseString(resBtba);
            HtmlDocument docBtba = new HtmlDocument();
            docBtba.LoadHtml(htmlBtba);
            //获取图片
            HtmlNode node = docBtba.DocumentNode.SelectSingleNode("//div[@class='box']/div[@class='l']/img");
            string imgSource = (node != null) ? node.Attributes["src"].Value : "暂无";
            //获取标题
            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='box']/ul/li[@class='h3']");
            string movieTitle = (node != null) ? node.InnerText : "暂无";
            //获取内容描述
            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='detail']");
            string desc = node.InnerText;
            desc = HttpHelper.NoHTML(desc);
            if (desc.Length > 200)
            {
                desc = desc.Substring(0, 200) + "......";
            }
            //上映时间
            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='box']/ul/li[6]/a");
            string pubData = (node != null)?node.InnerText:"暂无";
            
            //获取BT的URL地址
            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='btinfo']/h3/a");
            string BtSource = "暂时没有片源";
            if (node != null)
            {
                string btUrl = node.Attributes["href"].Value;
                try
                {
                    resBtba = HttpHelper.CreateGetHttpResponse(btUrl, 300, strAgent, null);
                    htmlBtba = HttpHelper.GetResponseString(resBtba);
                    docBtba.LoadHtml(htmlBtba);
                    //获取BT的真实种子地址
                    node = docBtba.DocumentNode.SelectSingleNode("//textarea[@id='status']");
                    BtSource = node.InnerText;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }

            }
            var newitem = new Movie()
            {
                Title = movieTitle,
                PubDate = pubData,
                Url = sourceUrl,
                Bt = BtSource,
                Img = imgSource,
                Desc = desc,

            };
            return newitem;
        }
    }
}