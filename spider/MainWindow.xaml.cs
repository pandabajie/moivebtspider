using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool flag = true;

        public MainWindow()
        {
            InitializeComponent();
            Dictionary<string, string> mydic = new Dictionary<string, string>()
            {
                {"single","单面页"},
                {"list","列表页"}
            };
            comboBox.ItemsSource = mydic;
            comboBox.SelectedValuePath = "Key";
            comboBox.DisplayMemberPath = "Value";
            comboBox.SelectedIndex = 1;
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlBox.Text.ToString();
            string selectType = (string)comboBox.SelectedValue;
            infoMsg.Text = "正在采集中，请稍候";
            if (url == String.Empty)
            {
                MessageBox.Show("请填写URL地址");
                return;
            }
            try
            {
                Uri u = new Uri(url);
                Movie newitem;
                List<Movie> movieList = new List<Movie>();
                switch (selectType)
                {
                    case "single":
                        switch (u.Host)
                        {
                            case "www.btba.com.cn":
                                newitem = MovieDetailInfo.getBtbaMovieInfo(url);
                                movieList.Add(newitem);
                            break;
                        }
                    break;
                    case "list":
                        string strAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:2.0b13pre) Gecko/20110307 Firefox/4.0b13pre";
                        HttpWebResponse res = HttpHelper.CreateGetHttpResponse(url, 300, strAgent, null);
                        if (res == null)
                        {
                            MessageBox.Show("URL无法访问");
                            return;
                        }
                        string html = HttpHelper.GetResponseString(res);
                        
                        int nextPage = 1;
                        HtmlNode nextNode;
                        switch (u.Host)
                        {
                            case "www.btba.com.cn":
                                HtmlDocument doc = new HtmlDocument();
                                do
                                {
                                    //打开列表页
                                    doc.LoadHtml(html);
                                    HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div[@class='left']/ul/li");
                                    foreach (HtmlNode child in collection)
                                    {
                                        HtmlNode linode = HtmlNode.CreateNode(child.OuterHtml);
                                        //获取单页详情地址
                                        string sourceUrl = linode.SelectSingleNode("//a[@class='a']").Attributes["href"].Value;
                                        newitem = MovieDetailInfo.getBtbaMovieInfo(sourceUrl);
                                        //foreach (System.Reflection.PropertyInfo p in newitem.GetType().GetProperties())
                                        //{
                                        //    Console.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(newitem));
                                        //}
                                        if (newitem != null)
                                        {
                                            movieList.Add(newitem);
                                        }

                                    }
                                    //获取当前分页值的节点
                                    HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='page']/b");
                                    if (node != null)
                                    {
                                        int currentPage = Convert.ToInt32(node.InnerText);
                                        //获取下一个兄弟元素 因为有一个换行符的文本节点，因此要两次，跳过换行那个文本节点
                                        nextNode = node.NextSibling;
                                        //没有下一页的节点了，就跳出循环
                                        if (nextNode == null)
                                        {
                                            break;
                                        }
                                        nextNode = nextNode.NextSibling;
                                        //下一个页码
                                        nextPage = Convert.ToInt32(nextNode.InnerText);
                                        //if (nextPage == 4)
                                        //{
                                        //    break;
                                        //}
                                    }
                                    int index = url.IndexOf("?");
                                    string newUrl = url;
                                    if (index > 0)
                                    {
                                        newUrl = url.Substring(0, index);
                                    }
                                    Console.WriteLine(newUrl);
                                    string nextUrl = newUrl + "?page=" + nextPage.ToString();
                                    Console.WriteLine(nextUrl);
                                    res = HttpHelper.CreateGetHttpResponse(nextUrl, 300, strAgent, null);
                                    if (res == null)
                                    {
                                        continue;
                                    }
                                    html = HttpHelper.GetResponseString(res);
                                    if (html == String.Empty)
                                    {
                                        continue;
                                    }
                                } while (true);
                                
                                break;
                            default:
                                MessageBox.Show("暂时不支持采集该站点");
                                break;
                        }
                        break;
                }
                //数据统一装进来
                infoMsg.Text = "采集完毕";
                listViewBox.ItemsSource = movieList;
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
                MessageBox.Show("复制成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string str = this.getRealUrlUsingYoutubeDL("http://v.youku.com/v_show/id_XMTgxMzQwNTI2NA==.html?from=y1.3-edu-newgrid-2153-10194.90153-90496.3-1");
            Console.WriteLine(str);
        }


        public string getRealUrlUsingYoutubeDL(string YoukuUrl)
        {
            string fileName = @"youtube-dl.exe";
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = fileName;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = string.Format(" --get-url --skip-download {0}", YoukuUrl);
            p.Start();
            p.WaitForExit(5000);//亲测，youtube-dl会因为不知道什么原因阻塞。。
            string output = p.StandardOutput.ReadToEnd();
            return output;
            //return output.Split('\n');//最后一个是""，自己处理吧

        }

        
    }
}
