using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CH_Server
{
    public partial class CH_Server : Form
    {
        TcpListener server = null;
        TcpClient clientSocket = null;
        static int counter = 0;
        public int people = 0;
        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();
        public int close = 0;
        public static List<string> people_name = new List<string>();
        public static List<string> PTEMP_name = new List<string>();
        public static int True_False = 0;
        public static string people_dis = "";

        public CH_Server()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.Visible = false;
            this.notifyIcon1.Visible = true;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            // socket start
            Thread t = new Thread(InitSocket);
            t.IsBackground = true;
            t.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InitSocket()
        {
            server = new TcpListener(IPAddress.Any, 9999);
            clientSocket = default(TcpClient);
            server.Start();
            DisplayText(">> Server Started");

            while (true)
            {
                try
                {
                    counter++;
                    clientSocket = server.AcceptTcpClient();
                    DisplayText(">> Accept connection from client");

                    NetworkStream stream = clientSocket.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string user_name = Encoding.Unicode.GetString(buffer, 0, bytes);
                    user_name = user_name.Substring(0, user_name.IndexOf("$"));

                    clientList.Add(clientSocket, user_name);

                    // send message all user
                    SendMessageAll(user_name + " 사용자 접속 ", "", true);

                    richTextBox2.AppendText(user_name+Environment.NewLine);
                    this.richTextBox2.ScrollToCaret();
                    people++;
                    people_name.Add(user_name);
                    label2.Text = Convert.ToString(people);

                    handleClient h_client = new handleClient();
                    h_client.OnReceived += new handleClient.MessageDisplayHandler(OnReceived);
                    h_client.OnDisconnected += new handleClient.DisconnectedHandler(h_client_OnDisconnected);
                    h_client.startClient(clientSocket, clientList);
                }
                catch (SocketException se)
                {
                    Trace.WriteLine(string.Format("InitSocket - SocketException : {0}", se.Message));
                    break;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("InitSocket - Exception : {0}", ex.Message));
                    break;
                }
            }

            clientSocket.Close();
            server.Stop();
        }

        void h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (clientList.ContainsKey(clientSocket))
                clientList.Remove(clientSocket);

            foreach(string peo_temp in people_name)
            {
                if (peo_temp == people_dis)
                {
                    richTextBox2.AppendText(peo_temp + "님이 접속을 해제 하였습니다." + Environment.NewLine);

                    people_name.Remove(peo_temp);
                    foreach (string a in people_name)
                    {
                        PTEMP_name.Add(a);
                    }
                    people_name = new List<string>();

                    foreach (string b in PTEMP_name)
                    {
                        people_name.Add(b);
                    }

                    people--;
                    label2.Text = Convert.ToString(people);
                    return;
                }
            }
            //richTextBox2.AppendText("사용자 연결해제" + Environment.NewLine);

        }

        private void OnReceived(string message, string user_name)
        {
            string displayMessage = "From client : " + user_name + " : " + message;
            DisplayText(displayMessage);
            SendMessageAll(message, user_name, true);
        }

        public void SendMessageAll(string message, string user_name, bool flag)
        {
            foreach (var pair in clientList)
            {
                Trace.WriteLine(string.Format("tcpclient : {0} user_name : {1}", pair.Key, pair.Value));

                TcpClient client = pair.Key as TcpClient;
                NetworkStream stream = client.GetStream();
                byte[] buffer = null;

                if (flag)
                {
                    buffer = Encoding.Unicode.GetBytes(user_name + System.DateTime.Now.ToString("(" + "HH:mm:ss" + ")")+ " : " + message );
                }
                else
                {
                    buffer = Encoding.Unicode.GetBytes(message);
                }

                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
        }

        private void DisplayText(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(text + Environment.NewLine);
                    this.richTextBox1.ScrollToCaret();
                }));
            }
            else
                richTextBox1.AppendText(text + Environment.NewLine);
            this.richTextBox1.ScrollToCaret();

            try
            {
                //MessageBox.Show(text)
                string[] temp = text.Split(':');

                if (temp[3] == "/DIS")
                {
                    people_dis = temp[4];
                    SendMessageAll(temp[2], "", false);
                }
            }
            catch
            {

            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true; // 폼의 표시
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // 최소화를 멈춘다 
            this.Activate(); // 폼을 활성화 시킨다
        }

        private void CH_Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (close == 0)
            {
                e.Cancel = true; // 종료 이벤트를 취소 시킨다
                this.Visible = false; // 폼을 표시하지 않는다;
            }
            else if (close == 1)
            {
                e.Cancel = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            close = 1;
            Application.Exit();
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            close = 1;
            Application.Exit();
        }
    }
}
