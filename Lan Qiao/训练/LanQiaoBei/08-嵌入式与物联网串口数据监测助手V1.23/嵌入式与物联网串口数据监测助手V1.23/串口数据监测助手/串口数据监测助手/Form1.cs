using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//引入线程和串口命名空间
using System.Threading;
using System.IO.Ports;      

namespace 串口数据监测助手
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SerialPort com = new SerialPort();              //实例化一个串口对象 
        byte[] SendData = new byte[] { 0xa3 };          //定义发送数据缓存
        byte[] readBuffer = new byte[8];                //实例化接收串口数据的数组
        string[] ports;
        string[] baudrate = { "2400", "4800", "9600", "19200", "57600", "115200" };
        int F_Fire = 0;
        int num = 0;
        
        private void SetDataForSerial()
        {
            ports = SerialPort.GetPortNames();

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            foreach (string str in ports)
            {
                comboBox1.Items.Add(str);
            }
            if (comboBox1.Items.Count != 0)
            {
                comboBox1.SelectedIndex = 0;
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }

            foreach (string str in baudrate)
            {
                comboBox2.Items.Add(str);
            }
            comboBox2.SelectedIndex = 2;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
           
        } 

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            textBox1.ReadOnly = true;

            button3.Text = "自动灯熄灭";
            button3.BackColor = Color.AliceBlue;
            button3.ForeColor = Color.DarkGreen;
            button4.Text = "无火警";
            button4.BackColor = Color.AliceBlue;
            button4.ForeColor = Color.DarkGreen;

            label9.Text = "串口关闭";
            label9.ForeColor = Color.Black;

            label10.Text = "停止监测!";
            label10.ForeColor = Color.Black;

            SetDataForSerial();

            com.ReceivedBytesThreshold = 8;   //设置串口接收到8个字节数据才触发DataReceived事件
            //为串口DataReceived事件添加处理方法
            com.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

         //串口数据接收DataReceived事件触发处理方法
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string strRcv = "";
            int count = com.BytesToRead;                    //获取串口缓冲器的字节数
            if (count != 8)
            {
                return;
            }

            num = 0;
            com.Read(readBuffer, 0, 8);                     //从串口缓冲区读出数据到数组
            com.DiscardInBuffer();

            for (int i = 0; i < readBuffer.Length; i++)
            {
                strRcv += readBuffer[i].ToString("X2") + " ";       //16进制显示  
            }

            this.BeginInvoke(new Action(() =>
            {
                textBox1.Text = strRcv;
            }));

            if (readBuffer[0] == 0xBF && readBuffer[7] == 0xFB)     //判断数据的帧头和帧尾
            {
                this.BeginInvoke(new Action(() =>
                {
                    double voltage = 0;
                    double temperature = 0;

                    voltage = readBuffer[1] * 100 + readBuffer[2];
                    voltage = voltage / 100;
                    temperature = readBuffer[3] * 10 + readBuffer[4];
                    temperature = temperature / 10;

                    label4.Text = voltage.ToString("F2");

                    label13.Text = readBuffer[6].ToString();
                    if (temperature < 99)
                    {
                        label2.Text = temperature.ToString("F1");
                    }

                    label13.Text = readBuffer[6].ToString();

                    if ((readBuffer[5] & 0x10) == 0x10)
                    {
                        button4.Text = "有火警";

                    }
                    else
                    {
                        button4.Text = "无火警";
                        button4.BackColor = Color.AliceBlue;
                        F_Fire = 0;
                    }
                    if ((readBuffer[5] & 0x01) == 0x01)
                    {
                        button3.BackColor = Color.Lime;
                        button3.Text = "自动灯打开";
                    }
                    else
                    {
                        button3.BackColor = Color.AliceBlue;
                        button3.Text = "自动灯熄灭";
                    }

                }));
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开串口")
            {
                com.PortName = comboBox1.Text;              //选择串口号
                com.BaudRate = int.Parse(comboBox2.Text);   //选择波特率
                com.DataBits = 8;                           //选择数据位数
                com.StopBits = (StopBits)1;                 //选择停止位数
                com.Parity = Parity.None;                   //选择是否奇偶校验

                try
                {
                    if (com.IsOpen)                         //判断该串口是否已打开
                    {
                        com.Close();
                        com.Open();
                    }
                    else
                    {
                        com.Open();
                    }

                    button1.Text = "关闭串口";
                    label9.Text = "串口打开";
                    label9.ForeColor = Color.DeepPink;
                    button2.Enabled = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message, "串口错误提示");
                }
            }
            else if (button1.Text == "关闭串口")
            {
                com.Close();                        //关闭串口   
                button1.Text = "打开串口";
                label9.Text = "串口关闭";
                label9.ForeColor = Color.Black;

                timer1.Stop();
                button2.Enabled = false;
                button2.Text = "启动";
                label10.Text = "停止监测!";
                label10.ForeColor = Color.Black;

                label2.Text = "00.0";
                label4.Text = "0.00";
                label13.Text = "0";
                button3.BackColor = Color.AliceBlue;
                button3.Text = "自动灯熄灭";
                button4.BackColor = Color.AliceBlue;
                button4.Text = "无火警";

                textBox1.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "启动")
            {
                timer1.Start();
                button2.Text = "停止";
                label10.Text = "正在监测...";
                label10.ForeColor = Color.DarkViolet;
            }
            else if (button2.Text == "停止")
            {
                timer1.Stop();
                button2.Text = "启动";
                label10.Text = "停止监测!";
                label10.ForeColor = Color.Black;

                label2.Text = "00.0";
                label4.Text = "0.00";
                label13.Text = "0";
                button3.BackColor = Color.AliceBlue;
                button3.Text = "自动灯熄灭";
                button4.BackColor = Color.AliceBlue;
                button4.Text = "无火警";

                textBox1.Clear();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string str;
            str = "【控制命令字】：0xA3" + "\r\n";
            str += "  " + "\r\n";
            str += "【返回数据帧，8字节】：" + "\r\n";
            str += "【第0字节】，0xBF： 帧头" + "\r\n";
            str += "【第1字节】：电压整数部分的数值" + "\r\n";
            str += "【第2字节】：电压小数部分的数值" + "\r\n";
            str += "【第3字节】：温度整数部分的数值" + "\r\n";
            str += "【第4字节】：温度小数部分的数值 / 湿度值" + "\r\n";
            str += "【第5字节】：开关设备执行状态" + "\r\n";
            str += "                第7～6位：空气质量4个等级" + "\r\n";
            str += "                第5位：人体感应，0--无人；1--有人" + "\r\n";
            str += "                第4位：火焰探测，0--无火焰；1--有火焰" + "\r\n";
            str += "                第3位：自定义开关设备，0--关闭；1--打开" + "\r\n";
            str += "                第2位：自定义开关设备，0--关闭；1--打开" + "\r\n";
            str += "                第1位：自定义开关设备，0--关闭；1--打开" + "\r\n";
            str += "                第0位：自动灯状态，0--关闭；1--打开" + "\r\n";
            str += "【第6字节】：按键触发次数" + "\r\n";
            str += "【第7字节】：0xFB： 帧尾";
            MessageBox.Show(str, "串口通信规约定义");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            com.Write(SendData, 0, 1);

            if (button4.Text == "有火警")
            {
                if (F_Fire == 0)
                {
                    button4.BackColor = Color.Red;
                    F_Fire = 1;
                }
                else
                {
                    button4.BackColor = Color.AliceBlue;
                    F_Fire = 0;
                }
            }

            num++;
            if (num > 20)
            {
                num = 0;
                com.Close();                        //关闭串口   
                button1.Text = "打开串口";
                label9.Text = "串口关闭";
                label9.ForeColor = Color.Black;

                timer1.Stop();
                button2.Enabled = false;
                button2.Text = "启动";
                label10.Text = "停止监测!";
                label10.ForeColor = Color.Black;

                label2.Text = "00.0";
                label4.Text = "0.00";
                label13.Text = "0";
                button3.BackColor = Color.AliceBlue;
                button3.Text = "自动灯熄灭";
                button4.BackColor = Color.AliceBlue;
                button4.Text = "无火警";

                textBox1.Clear();
            }
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (com.IsOpen)                         //判断该串口是否已打开
            {
                com.Close();
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            ports = SerialPort.GetPortNames();
            foreach (string str in ports)
            {
                comboBox1.Items.Add(str);
            }
            if (comboBox1.Items.Count != 0)
            {
                comboBox1.SelectedIndex = 0;
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }  
    }
}
