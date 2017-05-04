using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Windows.Threading;
namespace Dz_Gismeteo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon nIcon = new NotifyIcon();
        DispatcherTimer timer = new DispatcherTimer();
        string pathHttp = "https://www.gismeteo.ua/";
        string[] tagHttp = { "class=\"png\" title", "class=\'value m_temp c\'>", "<div class=\"wicon wind\">", "class=\"wicon barp\"", "class=\"wicon hum\"", "<h2 class=\"typeM\">" };

        StreamReader sr = null;
        public MainWindow()
        {
            InitializeComponent();
            this.StartProg();
        }

        private void StartProg()
        {
            Task.Run(() => this.GetMeteos());
            this.timer.Tick += new EventHandler(this.Timer_tick);
            this.timer.Interval = new TimeSpan(0, 0, 1);
            this.timer.Start();
            this.NotifyPropert();
            this.ShowInTaskbar = false;
        }

        void GetMeteos()
        {
            try
            {
                HttpWebRequest http = WebRequest.CreateHttp(this.pathHttp);
                WebResponse res = http.GetResponse();
                this.sr = new StreamReader(res.GetResponseStream());
                string str = this.sr.ReadToEnd();
                this.GetImage(str);
                this.GetTemp(str);
                this.GetWind(str);
                this.GetBar(str);
                this.GetHum(str);
                this.GetCity(str);
                this.Dispatcher.Invoke(new Action(() => this.ConectLab.Content = 
                                       string.Format("Time refresh : {0}", DateTime.Now.ToShortTimeString())));
            }
            catch (WebException)
            {
                this.Dispatcher.Invoke(new Action(() => this.ConectLab.Content = "Not Connect"));
                this.Dispatcher.Invoke(new Action(() => this.nIcon.ShowBalloonTip(5000,
                                                       "Not Connect",
                                                       "There is no connection to network! You will check connection to the Internet.",
                                                       ToolTipIcon.Error)));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void GetImage(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[0]);
                int index2 = str.IndexOf(">", index1);
                string s = str.Substring(index1 + tagHttp[0].Length, index2 - index1);
                index1 = s.IndexOf("\"") + 1;
                index2 = s.IndexOf("\"", index1);
                string title = s.Substring(index1, index2 - index1);
                index1 = s.IndexOf("url(//");
                index2 = s.IndexOf(")", index1);
                s = s.Substring(index1 + 6, index2 - (index1 + 4) - 2);
                s = string.Format("{0}{1}", "http://", s);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.TitleImage.Content = title;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(s);
                    bi.EndInit();
                    this.Image.Source = bi;
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void GetTemp(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[1]) + tagHttp[1].Length;
                int index2 = str.IndexOf("<", index1);
                string s = str.Substring(index1, index2 - index1);
                this.Dispatcher.Invoke(new Action(() => this.LabTemp.Content = s));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void GetWind(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[2]) + tagHttp[2].Length;
                int index2 = str.IndexOf("class=\'value m_wind mih\'", index1);
                string s = str.Substring(index1, index2 - index1);
                string s3 = s.Substring(s.IndexOf("title=\"") + 7, s.IndexOf("\"", s.IndexOf("title=\"") + 7) - (s.IndexOf("title=\"") + 7));
                string s1 = s.Substring(s.IndexOf("style='display:inline'>") + 23, s.IndexOf("<span") - (s.IndexOf("style='display:inline'>") + 23));
                string s2 = s.Substring(s.IndexOf("class=\"unit\">") + 13, s.IndexOf("</span>") - (s.IndexOf("class=\"unit\">") + 13));
                this.Dispatcher.Invoke(new Action(() => {
                    this.Wind.Content = s3;
                    this.WindSpeed.Content = string.Format("{0}{1}", s1, s2);
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        void GetBar(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[3]) + tagHttp[3].Length;
                int index2 = str.IndexOf("class='value m_press hpa'", index1);
                string s = str.Substring(index1, index2 - index1);
                string s3 = s.Substring(s.IndexOf("title=\"") + 7, s.IndexOf("\"", s.IndexOf("title=\"") + 7) - (s.IndexOf("title=\"") + 7));
                string s1 = s.Substring(s.IndexOf("class='value m_press torr'>") + 27, s.IndexOf("<span") - (s.IndexOf("class='value m_press torr'>") + 27));
                string s2 = s.Substring(s.IndexOf("class=\"unit\">") + 13, s.IndexOf("</span>") - (s.IndexOf("class=\"unit\">") + 13));
                this.Dispatcher.Invoke(new Action(() => {
                    this.Barp.Content = s3;
                    this.BarpSpeed.Content = string.Format("{0}{1}", s1, s2);
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void GetHum(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[4]) + tagHttp[4].Length;
                int index2 = str.IndexOf("class=\"meas_hum_txt hidden\"", index1);
                string s = str.Substring(index1, index2 - index1);
                string s3 = s.Substring(s.IndexOf("title=\"") + 7, s.IndexOf("\"", s.IndexOf("title=\"") + 7) - (s.IndexOf("title=\"") + 7));
                string s1 = s.Substring(s.IndexOf(">") + 1, s.IndexOf("<span") - (s.IndexOf(">") + 1));
                string s2 = s.Substring(s.IndexOf("class=\"unit\">") + 13, s.IndexOf("<span", s.IndexOf("class=\"unit\">") + 13) - (s.IndexOf("class=\"unit\">") + 13));
                this.Dispatcher.Invoke(new Action(() => {
                    this.Hum.Content = s3;
                    this.HumSpeed.Content = string.Format("{0}{1}", s1, s2);
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void GetCity(string str)
        {
            try
            {
                int index1 = str.IndexOf(tagHttp[5]) + tagHttp[5].Length;
                int index2 = str.IndexOf("</h2>", index1);
                string s = str.Substring(index1, index2 - index1);
                if (s.Length > 50)
                    s = "Not find city";
                this.Dispatcher.Invoke(new Action(() => {
                    this.City.Content = s;
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        void Timer_tick(object sender, EventArgs ev)
        {
            Task.Run(() => this.GetMeteos());
            this.timer.Interval = new TimeSpan(0, 0, 60);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void BtRefersch_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => this.GetMeteos());
        }

        void Show_window(object sender, EventArgs ev)
        {
            this.Visibility = Visibility.Visible;
        }

        private void HideWindw_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void NotifyPropert()
        {
            nIcon.Icon = new System.Drawing.Icon("Oxygen-Icons.org-Oxygen-Status-weather-showers-day.ico");
            nIcon.Visible = true;
            this.nIcon.Text = "Weather";
            this.nIcon.BalloonTipText = "Start weather";
            this.nIcon.BalloonTipIcon = ToolTipIcon.None;
            this.nIcon.ShowBalloonTip(500);
            nIcon.Click += new EventHandler(this.Show_window);
        }
    }
}
