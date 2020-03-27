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

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(String str);

        public Form1()
        {
            InitializeComponent();
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            //当点击开始监听的时候，在服务器端创建一个负责监视IP地址和端口好的socket
            Socket socketwatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            IPAddress ip = IPAddress.Any;
            //创建端口号对象
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBox2.Text));
            //监听
            socketwatch.Bind(point);
            ShowMsg("监听成功");
            //最大连接数
            socketwatch.Listen(10);

            Thread th = new Thread(listen);
            th.IsBackground = true;
            th.Start(socketwatch);
        }
        Socket socketsend;
        /// <summary>
        /// 等待客户端的连接，并创建一个负责通信的socket
        /// </summary>
        void listen(object o)
        {
            Socket socketwatch = o as Socket;
            //等待客户的连接，兵创建一个负责通信得到socket
            while (true)
            {
                //负责和客户端通信的socket
                socketsend = socketwatch.Accept();
                disSocket.Add(socketsend.RemoteEndPoint.ToString(),socketsend);
                //存入下拉框中
                comboBox1.Items.Add(socketsend.RemoteEndPoint.ToString());
                ShowMsg(socketsend.RemoteEndPoint.ToString() + ":" + "连接成功");
                //开启一个新线程不停的接受消息
                Thread th = new Thread(Recive);
                th.IsBackground = true;
                th.Start(socketsend);
            }
        }
        /// <summary>
        /// 将远程连接的客户端的ip和socket存入集合中
        /// </summary>
        Dictionary<string, Socket> disSocket = new Dictionary<string, Socket>();
        /// <summary>
        /// 服务器不停的接受客户端发送过来的消息
        /// </summary>
        /// <param name="o"></param>
        void Recive(object o)
        {
            Socket socketsend = o as Socket;
            while (true)
            {
                try
                {
                    //客户端连接成功后，服务器端应该接受客户端发来的消息

                    byte[] buffer = new byte[1024 * 1024 * 2];//第一次见这种操作
                    //实际接受到的有效字节数
                    int r = socketsend.Receive(buffer);
                    if (r == 0)
                        break;
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    ShowMsg(socketsend.RemoteEndPoint + ":" + str);
                }
                catch
                {

                }
               
            }
           
        }
        void  ShowMsg(string str)
        {

            if (richTextBox1.InvokeRequired)
            {
                while (richTextBox1.IsHandleCreated == false)
                {
                    if (richTextBox1.Disposing || richTextBox1.IsDisposed) return;
                }
                SetTextCallback d = new SetTextCallback(ShowMsg);

                richTextBox1.Invoke(d, new object[] { str });
            }
            else
            {
                richTextBox1.Text += str + "\n";
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
        }
        /// <summary>
        /// 服务器给客户端发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string str = richTextBox2.Text;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            List<byte> list = new List<byte>();
            list.Add(0);
            list.AddRange(buffer);
            byte[] newbuffer = list.ToArray();
            buffer = list.ToArray();
            string ip = comboBox1.SelectedItem.ToString();
            disSocket[ip].Send(newbuffer);

        }
        /// <summary>
        /// 发送文件(发送大的文件的话，下面的代码不能实现，大文件---断点续传)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string path = textBox3.Text;
            using (FileStream fsread=new FileStream(path, FileMode.Open,FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = fsread.Read(buffer, 0, buffer.Length);
                List<byte> list = new List<byte>();
                list.Add(1);
                list.AddRange(buffer);
                byte[] newbuffer = list.ToArray();
                disSocket[comboBox1.SelectedItem.ToString()].Send(newbuffer, 0, r+1, SocketFlags.None);

            }
        }
        /// <summary>
        /// 选择发送文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\Users\14516\Desktop\新建文本文档 (2).txt";
            ofd.Title = "请选择发送的文件";
            ofd.Filter = "所有文件|*.*";
            ofd.ShowDialog();
            textBox3.Text= ofd.FileName;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            int r = buffer.Length;
            disSocket[comboBox1.SelectedItem.ToString()].Send(buffer);
        }
    }
}
