using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(String str);

        public Form1()
        {
            InitializeComponent();
            
        }
        Socket socketSend;
        private void button1_Click(object sender, EventArgs e)
        {
            //创建负责通信的socket
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textBox1.Text);
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
            //获得远程的服务器ip和端口号
            socketSend.Connect(point);
          
             showMsg("连接成功");
            //开启一个新的线程不停的接受服务器发来的消息
            Thread th = new Thread(Revice);
            th.IsBackground = true;
            
            th.Start(socketSend);
            /// <summary>
            /// 不停接受服务器发来的消息
            /// </summary>
            void Revice(object o)
            {
                Socket socketSend = o as Socket;
                while (true)
                {
                    byte[] buffer = new byte[1024 * 1024 * 3];
                    //实际接收到的有限字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    //表示文字消息
                    if (buffer[0] == 0)
                    {
                        string s = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        showMsg(socketSend.RemoteEndPoint + ":" + s);
                    }
                    else if (buffer[0] == 1)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.InitialDirectory = @"C:\Users\14516\Desktop新建文本文档 (2).txt";
                        sfd.Title = "请选择要保存的文件";
                        sfd.Filter = "所有文件|*.*";
                        sfd.ShowDialog(this);
                        string path = sfd.FileName;
                        using (FileStream fswrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            fswrite.Write(buffer, 1, r - 1);
                        }
                        MessageBox.Show("保存成功");
                    }
                    else if (buffer[0] == 2)
                    {

                        
                        for(int i = 0; i < 500; i++)
                        {
                            if (this.Location.X == 200)
                                this.Location = new Point(210, 210);
                            this.Location = new Point(200, 200);
                        }

                    }


                }
            }
        }
       
       
        void showMsg(string str)
        {

            if (richTextBox1.InvokeRequired)
            {
                while (richTextBox1.IsHandleCreated==false)
                {
                    if (richTextBox1.Disposing || richTextBox1.IsDisposed) return;
                }
                SetTextCallback d = new SetTextCallback(showMsg);
                
                richTextBox1.Invoke(d, new object[] { str });
            }
            else
            {
                richTextBox1.Text += str + "\n";
            }


        }
        /// <summary>
        /// 客户端给服务器发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string str = richTextBox2.Text.Trim();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            socketSend.Send(buffer);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
        }


        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Location.X == 200)
                this.Location = new Point(210, 210);
            this.Location = new Point(200,200);
        }
    }
}
