using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace NFCManager
{
    public partial class MainForm : Form
    {
        public bool bConnectedDevice;/*是否连接上设备*/
        private ConfigData configData;//配置文件
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private List<int> baudList = new List<int>
        {
            9600,
            14400,
            19200,
            28800,
            38400,
            57600,
            115200
        };

        public int currentPort;
        public int currentBaud;
        public MainForm()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            configData = new ConfigData();
        }
        public void InitNfcEr302()
        {
#warning  这里确定一个检测NFC读写器是否连接着
            if (!bConnectedDevice)
            {
                int status = -1;
                if (!bConnectedDevice)
                {
                    try
                    {
                        status = Er302Helper.rf_init_com(configData.CurrentPort, configData.CurrentBaud);
                        if (status == 0)
                        {
                            bConnectedDevice = true;
                        }
                    }
                    catch (Exception exception)
                    {
                    }
                }
                if (!bConnectedDevice)
                {

                    for (int i = 1; i < 16; i++)
                    {
                        foreach (var baud in baudList)
                        {
                            try
                            {
                                status = Er302Helper.rf_init_com(i, baud);
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                                status = 2;
                            }
                            if (0 == status)
                            {
                                bConnectedDevice = true;
                                configData.CurrentPort = i;
                                configData.CurrentBaud = baud;
                                break;
                            }
                        }
                        if (0 == status)
                        {
                            break;
                        }
                    }
                    bConnectedDevice = false;
                }
                if (bConnectedDevice)
                {
                    try
                    {
                        this.roundButton1.IconColor = Color.LawnGreen;
                        roundButton1.Refresh();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    //MessageBox.Show("Connect device success!");
                }
                else
                {
                    try
                    {
                        this.roundButton1.IconColor = Color.Red;
                        roundButton1.Refresh();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    //MessageBox.Show("Connect device success!");
                }
            }
        }
        //回调函数;
        public void CallBack()
        {
            try
            {
                _timer.Elapsed += new ElapsedEventHandler(TimeElapse);
                _timer.AutoReset = true;
                _timer.Enabled = true;
                TimeStart();
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 执行记录
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void TimeElapse(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int intSecond = e.SignalTime.Second;
                if (intSecond%13 == 0)
                {
                    InitNfcEr302();
                }
            }
            catch (Exception exception)
            {
            }
        }
        #region 启动Timer
        /// <summary>
        /// 启动Timer
        /// </summary>
        public void TimeStart()
        {
            _timer.Start();
        }
        #endregion 

        #region 结束Timer
        /// <summary>
        /// 结束Timer
        /// </summary>
        public void TimeEnd()
        {
            try
            {
                _timer.Stop();
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bConnectedDevice)
            {
                try
                {
                    Er302Helper.rf_ClosePort();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                bConnectedDevice = false;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            //InitNfcEr302();
            CallBack();
        }
    }
}
