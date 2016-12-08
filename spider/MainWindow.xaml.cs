using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace spider
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBox.Text.ToString();
            if (url == String.Empty)
            {
                MessageBox.Show("请填写URL地址");
                return;
            }
            try
            {
                Uri u = new Uri(url);
                string strAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:2.0b13pre) Gecko/20110307 Firefox/4.0b13pre";
                HttpWebResponse res = HttpHelper.CreateGetHttpResponse(url, 300, strAgent, null);
                string html = HttpHelper.GetResponseString(res);
                switch (u.Host)
                {
                    case "www.btba.com.cn":
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(html);
                        HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@class='left']/ul/li");
                        List<Movie> roomList = new List<Movie>();
                        foreach (HtmlNode child in collection)
                        {
                            HtmlNode linode = HtmlNode.CreateNode(child.OuterHtml);
                            string sourceUrl = linode.SelectSingleNode("//a[@class='a']").Attributes["href"].Value;
                            HttpWebResponse resBtba = HttpHelper.CreateGetHttpResponse(sourceUrl, 300, strAgent, null);
                            string htmlBtba = HttpHelper.GetResponseString(resBtba);
                            HtmlDocument docBtba = new HtmlDocument();
                            docBtba.LoadHtml(htmlBtba);
                            //获取图片
                            HtmlNode node = docBtba.DocumentNode.SelectSingleNode("//div[@class='box']/div[@class='l']/img");
                            string imgSource = node.Attributes["src"].Value;
                            //获取内容描述
                            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='detail']");
                            string desc = node.InnerText;
                            //获取BT的URL地址
                            node = docBtba.DocumentNode.SelectSingleNode("//div[@class='btinfo']/h3/a");
                            string btUrl = node.Attributes["href"].Value;
                            resBtba = HttpHelper.CreateGetHttpResponse(btUrl, 300, strAgent, null);
                            htmlBtba = HttpHelper.GetResponseString(resBtba);
                            docBtba.LoadHtml(htmlBtba);
                            //获取BT的真实种子地址
                            node = docBtba.DocumentNode.SelectSingleNode("//textarea[@id='status']");
                            string BtSource = node.InnerText;
                            var newitem = new Movie()
                            {
                                Title = linode.SelectSingleNode("//h3/a").InnerText,
                                Year = linode.SelectSingleNode("//h3/b").InnerText,
                                Url = sourceUrl,
                                Bt = BtSource,
                                Img = imgSource,
                                Desc = HttpHelper.NoHTML(desc),

                            };
                            roomList.Add(newitem);
                        }
                        listViewBox.ItemsSource = roomList; ;

                        break;
                    default:
                        MessageBox.Show("暂时不支持采集该站点");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }
           
           
        }

        private void textblock_Click(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            string btString = tb.Text.ToString();
            try
            {
                Clipboard.SetText(btString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("复制成功");
        }
    }
}
