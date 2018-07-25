using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Week7
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SearchWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string cityToSearch = SearchWeatherBox.Text;
            if (cityToSearch != "")
            {
                getWeatherResult(cityToSearch);
            }
            else
            {
                string str = "请输入城市名称";
                var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
                await messageDialog.ShowAsync();
            }
            SearchWeatherBox.Text = "";
        }
        async void getWeatherResult(string city)
        {
            string url = "https://free-api.heweather.com/v5/weather?city=" + city + "&key=da6917b9e39a42888ffc3a9969f72de4";
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(url);
            JObject jsonObj = JObject.Parse(result);
            var data = jsonObj["HeWeather5"][0];
            long count = data.LongCount();
            if (count == 1)
            {
                ShowWeatherBlock.Text = "";
                string str = "请输入城市名称";
                var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
                await messageDialog.ShowAsync();
                return;
            }
            /*获取城市信息*/
            string cityName = "City: " + (string)data["basic"]["city"];
            string Date = "Date: " + DateTime.Today.ToString();
            string updateTime = "Update: " + (string)data["basic"]["update"]["loc"];

            /*获取今日数据*/
            var today = data["daily_forecast"][0];
            string weather_day = "Day Weather: " + (string)today["cond"]["txt_d"];
            string weather_night = "Night Weather: " + (string)today["cond"]["txt_n"];
            string tmp_high = "Highest Temperature: " + (string)today["tmp"]["max"] + "℃";
            string tmp_low = "Lowest Temperature: " + (string)today["tmp"]["min"] + "℃";

            /*获取天气舒适度*/
            string comf = "Comfort Index: " + (string)data["suggestion"]["comf"]["brf"] + ", " + (string)data["suggestion"]["comf"]["txt"];

            /*获取流感指数*/
            string flu = "Flu Index: " + (string)data["suggestion"]["flu"]["brf"] + (string)data["suggestion"]["flu"]["txt"];

            /*获取穿衣指数*/
            string drsg = "Dress Suggestion: " + (string)data["suggestion"]["drsg"]["brf"] + (string)data["suggestion"]["drsg"]["txt"];

            /*获取当前天气和温度*/
            string now_weather = "Now Weather: " + (string)data["now"]["cond"]["txt"];
            string now_tmp = "Now Temperature: " + (string)data["now"]["tmp"] + "℃";
            /*show*/
            ShowWeatherBlock.Text = cityName + "\n" + Date + "\n" + updateTime + "\n"
                                + weather_day + "\n" + weather_night + "\n" + tmp_high + "\n" + tmp_low + "\n"
                                + comf + "\n" + flu + "\n" + drsg + "\n"
                                + now_weather + "\n" + now_tmp;
        }

        private async void SearchIDButton_Click(object sender, RoutedEventArgs e)
        {
            string cardNum = SearchIDBox.Text;
            if (cardNum.Length != 18 || isExists(cardNum))
            {
                ShowWeatherBlock.Text = "";
                string str = "请输入正确的身份证号码";
                var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
                await messageDialog.ShowAsync();
                return;
            }
            getIDResult(cardNum);
            SearchIDBox.Text = "";
        }
        async void getIDResult(string card)
        {
            string url = "http://apis.juhe.cn/idcard/index?key=aa00abd65c65732b45353b91461f4343&cardno=" + card + "&dtype=xml";
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(url);

            XmlDocument document = new XmlDocument();
            document.LoadXml(result);

            /*处理错误情况*/
            XmlNodeList list = document.GetElementsByTagName("resultcode");
            IXmlNode node = list.Item(0);
            if (node.InnerText == "201")
            {
                ShowIDBlock.Text = "";
                string str = "请输入正确的身份证号码";
                var messageDialog = new Windows.UI.Popups.MessageDialog(str, "Warning");
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("确定", cmd => { }, "退出"));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("取消", cmd => { }));
                await messageDialog.ShowAsync();
                return;
            }

            /*获取地区信息*/
            list = document.GetElementsByTagName("area");
            node = list.Item(0);
            string area = "Area： " + node.InnerText;

            /*获取性别信息*/
            list = document.GetElementsByTagName("sex");
            node = (IXmlNode)list.Item(0);
            string sex = "Sex: " + node.InnerText;

            /*获取生日信息*/
            list = document.GetElementsByTagName("birthday");
            node = (IXmlNode)list.Item(0);
            string birthday = "Birthday: " + node.InnerText;

            ShowIDBlock.Text = "Person Info:"+ "\n" + area + "\n" + sex + "\n" + birthday;
        }

        private bool isExists(string str)
        {
            return Regex.Matches(str, "[a-zA-Z]").Count > 0;
        }
    }
}
