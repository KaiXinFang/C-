using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace 多线程开发WPF
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

        /*
         * 同步单线程：
         *      按顺序执行，每次调用完成后，才能执行下一行，是同一个线程运行
         */

        //同步方法
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnSync_Click 同步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);
            //int j = 0;
            //int k = 1;
            //int m = j + k;
            Action<string> action = this.DoSomethingLong;
            for (int i=0;i<5;i++)
            {
                string name = string.Format("{0}_{1}", "btnSync_Click", i);
                //调用需要在线程内执行的方法
                //this.DoSomethingLong(name);
                action.Invoke(name);//invoke方法是同步执行的
            }
            Console.WriteLine("**********btnSync_Click 同步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
        }

        //创建启动线程时执行的方法DoSomethingLong
        private void DoSomethingLong(string name)
        {
            Console.WriteLine();
            Console.WriteLine("************DoSomethingLong start {0} {1} {2} *************",name,Thread.CurrentThread.ManagedThreadId.ToString("00")
                ,DateTime.Now.ToString("HHmmss:fff"));
            long lResult = 0;
            for(int i=0;i<100000000;i++)
            {
                lResult += i;
            }
            Console.WriteLine("************DoSomethingLong end {0} {1} {2} {3} *************", name, Thread.CurrentThread.ManagedThreadId.ToString("00")
                , DateTime.Now.ToString("HHmmss:fff"),lResult);
            Console.WriteLine();
        }
        //创建启动线程时执行的方法UpDateDB
        private void UpDateDB(string name)
        {
            Console.WriteLine();
            Console.WriteLine("************UpDateDB start {0} {1} {2} *************", name, Thread.CurrentThread.ManagedThreadId.ToString("00")
                , DateTime.Now.ToString("HHmmss:fff"));
            long lResult = 0;
            for (int i = 0; i < 1000000000; i++)
            {
                lResult += i;
            }
            Console.WriteLine("************UpDateDB end {0} {1} {2} {3} *************", name, Thread.CurrentThread.ManagedThreadId.ToString("00")
                , DateTime.Now.ToString("HHmmss:fff"), lResult);
            Console.WriteLine();
        }

        /*异步多线程
         *      任何的异步多线程都离不开委托delegate--lambda--action/func(泛型委托)
         *委托的异步调用
         *  异步多线程:
         *      发起调用，不等待结束就直接进入下一行(主线程),动作会由一个新线程执行(子线程)
         */
        //异步方法
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnAsync_Click 异步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);

            Action<string> action = this.DoSomethingLong;
            //下面是启动委托的三种方法
            action.Invoke("Button_Click_1");
            action("Button_Click_1");
            action.BeginInvoke("Button_Click_1", null, null);//只有用BeginInvoke启动，才会使用异步，上面两种是不会使用异步的

            Console.WriteLine("**********btnAsync_Click 异步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();

        }

        //异步进阶函数
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnAsyncAdvanced_Click 异步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);

            // 业务需求：用户点击按钮，希望业务操作要做，但是不能卡页面，所以可以用异步多线程
            // 项目经理：不允许没有监控的项目上线-----需要在业务操作后记录下日志

            Action<string> action = this.UpDateDB;

            
            
            AsyncCallback callback = ar =>
            {
                Console.WriteLine(ar.AsyncState);
                Console.WriteLine($"btnAsyncAdvanced_Click操作已经完成了...{Thread.CurrentThread.ManagedThreadId}");
            };

            //使用委托启动线程(并调用回调函数callback)
            action.BeginInvoke("btnAsyncAdvanced_Click_2", callback, "sunday");//只有用BeginInvoke启动，才会使用异步，上面两种是不会使用异步的

            //下面这句话必须在异步线程执行结束后才能出现
            //( 怎么保证呢？)
            //  可以在启动线程时使用回调函数
            //Console.WriteLine($"btnAsyncAdvanced_Click操作已经完成了...{Thread.CurrentThread.ManagedThreadId}");

            //需求2：用户必须确定操作完成，才能返回----上传文件，只有成功之后才能预览---需要等到任务计算真的完成后才能给用户返回
            //希望一方面文件上传，完成后才预览；另一方面，还希望有个进度提示--只有主线程才能操作界面
            Console.WriteLine("**********btnAsyncAdvanced_Click 异步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
        }

        //使用BeginInvoke返回值的方法
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnAsyncRunProcess_Click 异步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);

            Action<string> action = this.UpDateDB; 

            //使用委托启动线程(并接收BeginInvoke返回值)
            IAsyncResult asyncresult = action.BeginInvoke("文件上传", null, null);

            //需求2：用户必须确定操作完成，才能返回----上传文件，只有成功之后才能预览---需要等到任务计算真的完成后才能给用户返回
            //希望一方面文件上传，完成后才预览；另一方面，还希望有个进度提示--只有主线程才能操作界面
            int i = 0;
            while(!asyncresult.IsCompleted)//IsCompleted是一个属性，用来描述异步动作是否完成：其实一开始是个false，异步动作完成后会去修改这个属性为true
            {
                if(i<9)
                {
                    this.ShowConsoleAndView($"当前文件上传进度为{++i * 10}%");
                }
                else
                {
                    this.ShowConsoleAndView("当前文件上传进度为99.999%");
                }
                Thread.Sleep(200);
            }

            Console.WriteLine("完成文件上传，执行预览，绑定到界面");

            Console.WriteLine("**********btnAsyncRunProcess_Click 异步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
        }

        //界面label控件显示
        private void ShowConsoleAndView(string text)
        {
            Console.WriteLine(text);
            this.lblProcessing.Content = text;
        }

        //使用信号量控制线程
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnAsyncRunSemaphore_Click 异步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);

            Action<string> action = this.UpDateDB;

            //使用委托启动线程(并接收BeginInvoke返回值)
            IAsyncResult asyncresult = action.BeginInvoke("调用接口", null, null);

            //有时启动线程之后需要并发执行一些需求，需要等待线程执行完毕后，再一起执行后续的代码，
            //此时就可使用信号量来实现（比如下面的代码还要执行一些功能）
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            Console.WriteLine("正在执行一些需要的功能.....");
            //使用内置的接口设置信号量
            asyncresult.AsyncWaitHandle.WaitOne();//阻塞当前线程，直到收到信号量，该信号量是从asyncresult发出的
            asyncresult.AsyncWaitHandle.WaitOne(-1);//一之等待
            asyncresult.AsyncWaitHandle.WaitOne(1000);//等待指定时间，超过就不再等待 (可用来做超时控制 微服务架构，一个操作需要调用5个接口
                                                      //如果某个接口很慢，会影响整个流程，可以做超时控制，超时就更换接口或者放弃或者给个结果)
            Console.WriteLine("完成文件上传，执行预览，绑定到界面");

            Console.WriteLine("**********btnAsyncRunSemaphore_Click 异步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
        }
        private int RemoteService()
        {
            long lResult = 0;
            for(int i=0;i<1000000000;i++)
            {
                lResult += i;
            }
            return DateTime.Now.Day;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("**********btnAsyncRunSemaphore_Click 异步方法 start {0}************", Thread.CurrentThread.ManagedThreadId);

            Func<int> func = this.RemoteService;//注意返回值为int

            //使用委托启动线程(并接收BeginInvoke返回值)
            IAsyncResult asyncResult = func.BeginInvoke(null, null);//此处的asyncResult是异步调用结果，描述异步操作的，不是线程中代码的返回值

            //获取线程中的返回值,只能使用EndInvoke
            //EndInvoke的返回值类型是由创建委托时的Func决定的
            int iResult1 = func.EndInvoke(asyncResult);

            //将返回值设置为button的内容，进行显示
            this.asrncResult.Content = $"返回值为:{iResult1}";

            {
                //另一种写法
                IAsyncResult asyncResult2 = func.BeginInvoke(ar =>
                {
                    int iResult4 = func.EndInvoke(ar);
                }, null);
            }

            {
                Func<string> func2 = () => DateTime.Now.ToString();
                string sResult = func2.EndInvoke(func2.BeginInvoke(null,null));
            }

            {
                Func<string, string> func3 = s => $"1+{s}";
                string sResult2 = func3.EndInvoke(func3.BeginInvoke("Jason", null, null));
            }

            Console.WriteLine("**********btnAsyncRunSemaphore_Click 异步方法 end {0}************", Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine();
        }
    }
}
