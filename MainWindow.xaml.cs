using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 多线程_双色球实战.Common;

namespace 多线程_双色球实战
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] RedNums =
        {
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16","17","18","19","20",
            "21","22","23","24","25","26","27","28","29","30",
            "31","32","33"
        };
        private string[] BlueNums =
{
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16"
        };

        //添加一把锁
        private static readonly object SSQ_Lock = new object();
        private bool IsGoOn = true;//控制线程是否无线循环的开关(flag)
        private List<Task> TaskList = new List<Task>();

        public MainWindow()
        {
            InitializeComponent();
            this.BtnEnd.IsEnabled = false;
            this.BtnStart.IsEnabled = true;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.BtnStart.Content = "运行...";
                this.BtnStart.IsEnabled = false;
                //每次点击开始按钮后要更新IsGoOn，让线程可以运行
                //因为再点击结束按钮时更改了这个标志量
                this.IsGoOn = true;
                //每次运行都要清空线程数组taskList
                this.TaskList.Clear();
                this.lblBlue.Content = "00";
                this.lblRed1.Content = "00";
                this.lblRed2.Content = "00";
                this.lblRed3.Content = "00";
                this.lblRed4.Content = "00";
                this.lblRed5.Content = "00";
                this.lblRed6.Content = "00";
                Thread.Sleep(1000);
                TaskFactory taskFactory = new TaskFactory();
                foreach (var control in this.LblGrid.Children)
                {
                    if(control is Label)
                    {
                        Label lbl = (Label)control;
                        if(lbl.Name.Contains("Blue"))
                        {
                            TaskList.Add(taskFactory.StartNew(() => 
                            {
                                while(this.IsGoOn)
                                {
                                    int indexNum = new RandomHelper().GetRandomNumberLong(0, this.BlueNums.Length);
                                    string sNumber = BlueNums[indexNum];
                                    //调用方法更新UI界面
                                    this.UpdateLabelUI(lbl,sNumber);
                                }
                            }));
                        }
                        else
                        {
                            TaskList.Add(taskFactory.StartNew(()=>
                            {
                                while (this.IsGoOn)
                                {
                                    int indexNum = new RandomHelper().GetRandomNumberLong(0, this.RedNums.Length);
                                    string sNumber = RedNums[indexNum];
                                    lock (SSQ_Lock)
                                    {
                                        if (this.CheckRedLabelIsExsit(sNumber))
                                        {
                                            continue;//重复了就放弃更新，重新获取数字
                                        }
                                        //调用方法更新UI界面
                                        this.UpdateLabelUI(lbl, sNumber);
                                    }
                                }
                            }));
                        }
                    }
                }
                //再创建一个线程去更新End按钮的状态
                Task.Run(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            if (!this.CheckRedLabelIsExsit("00") && !CheckBlueLabelIsExsit("00"))
                            {
                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    this.BtnEnd.IsEnabled = true;
                                }));
                                break;
                            } 
                        }
                    });
                //当所有线程结束运行后，调用显示双色球结果的函数
                taskFactory.ContinueWhenAll(this.TaskList.ToArray(),tArray => this.ShowResult());
            }
            catch(Exception es)
            {
                Console.WriteLine(es.ToString());
            }
        }

        //检查红色label搜寻到的数字是否出现过
        private bool CheckRedLabelIsExsit(string sNumber)
        {
            bool isExist = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var control in this.LblGrid.Children)
                {
                    if (control is Label)
                    {
                        Label lbl = (Label)control;
                        //判断是否是Red按钮
                        if (lbl.Name.Contains("Red"))
                        {
                            //判断当前lbl的内容和随机获取的内容是否一样
                            if (lbl.Content.Equals(sNumber))
                            {
                                isExist = true;
                            }
                        }
                    }
                }
            }));
            return isExist;
        }
        //检查蓝色label搜寻到的数字是否出现过
        private bool CheckBlueLabelIsExsit(string sNumber)
        {
            bool isExist = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                //判断lblBlue的内容和指定内容是否一样
                if (this.lblBlue.Content.Equals(sNumber))
                {
                    isExist = true;
                }

            }));
            return isExist;
        }

        //更新UI界面元素，注意使用Dispatcher，利用委托进行更改
        //否则无法更改界面元素，因为界面是由主线程维护的，子线程操控不了
        private void UpdateLabelUI(Label lbl,string text)
        {
            this.Dispatcher.Invoke(new Action(()=>
            {
                lbl.Content = text;
            }));
        }

        private void BtnEnd_Click(object sender, RoutedEventArgs e)
        {
            this.BtnStart.IsEnabled = true;
            this.BtnEnd.IsEnabled = false;
            this.IsGoOn = false;

        }

        //展示双色球结果
        private void ShowResult()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(string.Format("本期双色球结果为:{0} {1} {2} {3} {4} {5} 篮球{6}",
               this.lblRed1.Content,
               this.lblRed2.Content,
               this.lblRed3.Content,
               this.lblRed4.Content,
               this.lblRed5.Content,
               this.lblRed6.Content,
               this.lblBlue.Content));
            }));
        }
    }
}
