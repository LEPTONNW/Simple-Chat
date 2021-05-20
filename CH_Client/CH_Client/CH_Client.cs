using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CH_Client
{
    public partial class CH_Client : Form
    {
        TcpClient clientSocket = new TcpClient();
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;
        public int color = 0;
        public int close = 0;
        public static string Key_Temp = "";
        public static string TB2 = ""; //TextBox2's Focus Parameter
        public static string CB2 = ""; //combobox2's Focus Parameter
        public static Thread t_handler;
        public int GetCount = 0;
        public int VRC = 0;
        public int VRC2 = 3;
        public int VRC3 = 0;
        public int Kill_C = 1;
        public Icon newIcon;
        

        [DllImport("user32.dll")]
        private static extern int GetFocus();

        public CH_Client()
        {
            InitializeComponent();
            
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.ViRobot.Visible = true;
            ViRobot.ContextMenuStrip = contextMenuStrip1;
        }

        /////////////////////////////////////// 키보드 후킹 시작

        private bool bAltAndNum;//Alt+숫자가 같이 눌린 상태(나중에 1 이외의 숫자도 쓸지 모르니..)
        private bool bAltOrNum;//Alt+숫자 이후 Alt만 남거나 숫자키만 남거나 한 상태
        event KeyboardHooker.HookedKeyboardUserEventHandler HookedKeyboardNofity;

        private long Form1_HookedKeyboardNofity(int iKeyWhatHappened, int vkCode)
        {
            //일단은 기본적으로 키 이벤트를 흘려보내기 위해서 0으로 세팅
            long lResult = 0;


            //if (vkCode == 49 && iKeyWhatHappened == 32) //ALT + 1
            //{
            //    bAltAndNum = true;
            //    bAltOrNum = false;
            //    lResult = 0;
            //    textBox2.Text = "ALT + 1이 눌렸습니다.";
            //}
            //else if (bAltAndNum && iKeyWhatHappened == 128)
            //{
            //    bAltAndNum = false;
            //    bAltOrNum = true;
            //   lResult = 0;
            //    textBox2.Text += Environment.NewLine + "1은 눌려있고 ALT가 떨어졌다.";
            //}
            //else if (bAltAndNum && vkCode == 49)
            //{
            //    bAltAndNum = false;
            //    bAltOrNum = true;
            //    lResult = 0;
            //    textBox2.Text += Environment.NewLine + "ALT는 눌려있고 1이 떨어졌다.";
            //}
            //else if (!bAltAndNum && bAltOrNum && (vkCode == 49 || vkCode == 164))
            //{
            //    bAltOrNum = false;
            //    lResult = 0;
            //    textBox2.Text += Environment.NewLine + "키 조합이 완료되었다.";
            //    timer1.Interval = 50;
            //    timer1.Start();
            //}
            if(vkCode == 27)
            {
                Application.Exit();
            }
            else if (vkCode == 112) 
            {
                bAltAndNum = true;
                bAltOrNum = false;
                this.Visible = true;
                lResult = 0;

                VRC2 = 2;
                this.ViRobot.Visible = true;
            }
            else if (vkCode == 113) 
            {
                this.Visible = false;
                bAltAndNum = true;
                bAltOrNum = false;
                lResult = 0;

                VRC2 = 2;
                this.ViRobot.Visible = false;
            }
            else if(vkCode == 114) //F3
            {
                try
                {
                    KILL_Form();

                }
                catch (Exception e)

                {
                    MessageBox.Show(e.ToString());
                }
                
            }
            else if(vkCode == 115) //F4
            {
                trackBar1.Value = 100;
                this.Opacity = 100;
            }

            else
            {
                //나머지 키들은 얌전히 보내준다.
                bAltAndNum = false;
                bAltOrNum = false;
                lResult = 0;
            }


            return lResult;
        }

        private void KILL_Form()
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm.Name == "KILL")
                {
                    if (openForm.WindowState == FormWindowState.Minimized)
                    {
                        openForm.WindowState = FormWindowState.Normal;
                        openForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
                    }
                    //openForm.Activate();
                    return;
                }
            }
            KILL myForm = new KILL();
            myForm.StartPosition = FormStartPosition.Manual;
            myForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
            myForm.ShowDialog();

            if (Convert.ToInt32(myForm.Passvalue) == 1)
            {
                close = Convert.ToInt32(myForm.Passvalue);
                KILL_Program();


                Process[] processList = Process.GetProcessesByName("CH_Client.exe");

                if (processList.Length > 0)
                {
                    processList[0].Kill();
                }

                Application.ExitThread();
                Application.Exit();
            }
            else
            {
                myForm.Dispose();
            }


        }

        public void KILL_Program()
        {
            string batFilePath = @"Kill_Code.bat";
            if (!File.Exists(batFilePath))
            {
                using (FileStream fs = File.Create(batFilePath))
                {
                    fs.Close();
                }
            }

            using (StreamWriter sw = new StreamWriter(batFilePath))
            {
                sw.WriteLine("%1 mshta vbscript:CreateObject(" + '\u0022' + "Shell.Application" + '\u0022' + ").ShellExecute(" + '\u0022' + "cmd.exe" + '\u0022' + "," + '\u0022' + "/c %~s0 ::" + '\u0022' + "," + '\u0022' + '\u0022' + "," + '\u0022' + "runas" + '\u0022' + ",1)(window.close)&&exit");
                sw.WriteLine("@echo off");
                sw.WriteLine("");
                sw.WriteLine("timeout /t 1");
                sw.WriteLine("echo.");
                sw.WriteLine("echo Kill Code Start");
                sw.WriteLine("");
                sw.WriteLine("del %~dp0CH_Client.exe");
                //sw.WriteLine("pause>nul");

            }
            Process.Start(batFilePath);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //textBox2.Text += Environment.NewLine + "키조합에 의한 특수한 행위를 해보자.";
            timer1.Stop(); //타이머가 반복해서 동작하지 않도록 한다.
        }



        ///////////////////////////////////////

        private void Form1_Load(object sender, EventArgs e)
        {
            //3. 후크 이벤트를 연결한다.
            HookedKeyboardNofity += new KeyboardHooker.HookedKeyboardUserEventHandler(Form1_HookedKeyboardNofity);

            //4. 자동으로 훅을 시작한다. 여기서 훅에 의한 이벤트를 연결시킨다.
            KeyboardHooker.Hook(HookedKeyboardNofity);



            this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            ViRobot.ShowBalloonTip(1000);
            comboBox1.Text = "검정색";
            //comboBox2.Text = "200.200.172.25";
            trackBar1.Value = 100;
            button4.Enabled = false;

            //textBox2.Select();

            this.Activate();
            textBox2.Select();
            TB2 = GetFocus().ToString();
            comboBox2.Select();
            CB2 = GetFocus().ToString();


            /* 실행 시 관리자 권한 상승을 위한 코드 시작 */
            if (/* Main 아래에 정의된 함수 */IsAdministrator() == false)
            {
                try
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo();
                    procInfo.UseShellExecute = true;
                    procInfo.FileName = Application.ExecutablePath;
                    procInfo.WorkingDirectory = Environment.CurrentDirectory;
                    procInfo.Verb = "runas";
                    Process.Start(procInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                close = 1;
                Application.Exit();
                return;
            }
        }

        /* 실행 시 관리자 권한 상승을 위한 함수 시작 */
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }


            return false;
        }
        /* 실행 시 관리자 권한 상승을 위한 함수 끝 */

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox2.Text))
                {
                    return;
                }
                if (comboBox1.Text == "검정색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/black" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "파란색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/blue" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "노란색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/yellow" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "초록색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/green" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "갈색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/brown" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "보라색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/violet" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "빨간색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/red" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
                else if (comboBox1.Text == "회색")
                {
                    byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/gray" + "$");
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                    textBox2.Text = "";
                    this.richTextBox1.ScrollToCaret();
                }
            }
            catch
            {
                MessageBox.Show("서버와 연결이 되지 않았거나" + Environment.NewLine + "허용되지 않은 문자가 입력되었습니다.");
            }
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!base.ProcessCmdKey(ref msg, keyData))
            {
                if (keyData.Equals(Keys.Enter))
                {
                    if (TB2 == GetFocus().ToString())
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(textBox2.Text))
                            {
                                return true;
                            }

                            if (comboBox1.Text == "검정색")

                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/black" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "파란색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/blue" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "노란색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/yellow" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "초록색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/green" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "갈색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/brown" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "보라색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/violet" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "빨간색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/red" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                            else if (comboBox1.Text == "회색")
                            {
                                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox2.Text + "/gray" + "$");
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                                textBox2.Text = "";
                                this.richTextBox1.ScrollToCaret();
                            }
                        }
                        catch
                        {
                            MessageBox.Show("서버와 연결이 되지 않았거나" + Environment.NewLine + "허용되지 않은 문자가 입력되었습니다.");
                        }
                    }
                    else if (CB2 == GetFocus().ToString())
                    {
                        button1.PerformClick();
                    }
                    return true;

                }
                else if (keyData.Equals(Keys.Control | Keys.A))
                {
                    textBox2.SelectAll();
                    return true;
                }
                else if(keyData.Equals(Keys.Escape))
                {
                    Application.Exit();
                    return true;
                }
                else if(keyData.Equals(Keys.F3))
                {
                    MessageBox.Show(GetFocus().ToString());
                    return true;
                }

                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox2.Text) || string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("IP주소와 닉네임을 먼저 설정하세요");
            }
            else
            {
                GetCount = 1;

                clientSocket.Connect(comboBox2.Text, 9999);
                stream = clientSocket.GetStream();

                message = Environment.NewLine+"채팅서버에 접속되었습니다!!!";
                DisplayText(message);

                
                

                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox1.Text + "$");
                stream.Write(buffer, 0, buffer.Length);

                //byte[] buffer2 = Encoding.Unicode.GetBytes("님이서버에 접속되었습니다." + "$");
                //stream.Write(buffer2, 0, buffer2.Length);

                stream.Flush();

                t_handler = new Thread(GetMessage);
                t_handler.IsBackground = true;
                t_handler.Start();

                
                button1.Enabled = false;
                button4.Enabled = true;
            }
        }

        private void GetMessage()
        {
            while (GetCount > 0)
            {
                try
                {
                    stream = clientSocket.GetStream();
                    int BUFFERSIZE = clientSocket.ReceiveBufferSize;
                    byte[] buffer = new byte[BUFFERSIZE];
                    int bytes = stream.Read(buffer, 0, buffer.Length);

                    string message = Encoding.Unicode.GetString(buffer, 0, bytes);

                    string[] MSG = message.Split('/');
                    if (MSG[1] == "black")
                    {   
                        color = 1;
                    }
                    else if (MSG[1] == "blue")
                    {
                        color = 2;
                    }
                    else if (MSG[1] == "yellow")
                    {
                        color = 3;
                    }
                    else if (MSG[1] == "green")
                    {
                        color = 4;
                    }
                    else if (MSG[1] == "brown")
                    {
                        color = 5;
                    }
                    else if (MSG[1] == "violet")
                    {
                        color = 6;
                    }
                    else if (MSG[1] == "red")
                    {
                        color = 7;
                    }
                    else if (MSG[1] == "gray")
                    {
                        color = 8;
                    }
                    DisplayText(MSG[0]);
                }
                catch
                {

                }

            }
        }

        private void DisplayText(string text)
        {


            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    if (color == 1)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.Black;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 2)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.Blue;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 3)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.Yellow;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 4)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.ForestGreen;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 5)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.Brown;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 6)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.DarkViolet;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 7)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.Red;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                    else if (color == 8)
                    {
                        richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                        richTextBox1.SelectionColor = System.Drawing.Color.DarkGray;
                        Thread.Sleep(1);
                        richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                    }
                }));
            }
            else
            {
                if (color == 1)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.Black;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 2)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.Blue;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 3)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.Yellow;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 4)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.ForestGreen;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 5)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.Brown;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 6)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.DarkViolet;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 7)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.Red;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
                else if (color == 8)
                {
                    richTextBox1.SelectionFont = new Font(text, 10, FontStyle.Regular);
                    richTextBox1.SelectionColor = System.Drawing.Color.DarkGray;
                    Thread.Sleep(1);
                    richTextBox1.AppendText(text + Environment.NewLine + Environment.NewLine);
                }
            }
            this.richTextBox1.ScrollToCaret();

            //if(this.Visible == false)
            //{
            
                if (VRC2 == 0)
                {
                    VRC2 = 1;
                    timer2.Interval = 1000;
                    timer2.Start();
                }
            //}
            
            
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            this.Opacity = trackBar1.Value * 0.01;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void 종료ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox1.Text + "님이 접속을 해제 하였습니다./black:" + "/DIS:" + this.textBox1.Text + "$");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                textBox2.Text = "";
                this.richTextBox1.ScrollToCaret();
            }
            catch
            {

            }

            close = 1;
            Application.Exit();
        }

        private void contextMenuStrip1_DoubleClick(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true; // 폼의 표시
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // 최소화를 멈춘다 
            this.Activate(); // 폼을 활성화 시킨다
        }

        private void CH_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(close == 0)
            {
                e.Cancel = true; // 종료 이벤트를 취소 시킨다
                this.Visible = false; // 폼을 표시하지 않는다;
            }
            else if(close == 1)
            {
                e.Cancel = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox1.Text + "님이 접속을 해제 하였습니다./black:" + "/DIS:" + this.textBox1.Text + "$");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                textBox2.Text = "";
                this.richTextBox1.ScrollToCaret();
            }
            catch
            {

            }

            close = 1;

            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = Encoding.Unicode.GetBytes(this.textBox1.Text + "님이 접속을 해제 하였습니다./black:" + "/DIS:" + this.textBox1.Text + "$");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                textBox2.Text = "";
                this.richTextBox1.ScrollToCaret();
            }
            catch
            {

            }
            GetCount = 0;
            clientSocket.Close();
            clientSocket = new TcpClient();
            stream = default(NetworkStream);
            message = string.Empty;
            //t_handler.Abort();
            button1.Enabled = true;
            button4.Enabled = false;
            

            richTextBox1.AppendText("서버와의 연결이 해제되었습니다." + Environment.NewLine );
            this.richTextBox1.ScrollToCaret();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (VRC2 == 1)
            {
                if (VRC == 0)
                {
                    this.ViRobot.Visible = true;

                    VRC = 1;
                }
                else
                {
                    this.ViRobot.Visible = false;
                    VRC = 0;

                }
            }
            else if(VRC2 == 2)
            {
                timer2.Stop();
                this.ViRobot.Visible = true;
            }
        }

        private void CH_Client_Click(object sender, EventArgs e)
        {
            VRC2 = 2;
            this.ViRobot.Visible = true;
        }

        private void CH_Client_Leave(object sender, EventArgs e)
        {

        }

        private void CH_Client_MouseLeave(object sender, EventArgs e)
        {
            VRC2 = 0;
            //MessageBox.Show("VRC 0");
        }

        private void CH_Client_MouseClick(object sender, MouseEventArgs e)
        {
            VRC2 = 2;
            this.ViRobot.Visible = true;
        }

        private void trackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }
    }


    public class KeyboardHooker
    {
        // 후킹된 키보드 이벤트를 처리할 이벤트 핸들러
        private delegate long HookedKeyboardEventHandler(int nCode, int wParam, IntPtr lParam);

        /// <summary>
        /// 유저에게 노출할 이벤트 핸들러
        /// iKeyWhatHappened : 현재 입력이 KeyDown/KeyUp인지 여부 - Key별로 숫자가 다르다.
        /// vkCode : virtual key 값, System.Windows.Forms.Key의 상수 값을 int로 변환해서 대응시키면 된다.
        /// </summary>
        /// <param name="iKeyWhatHappened"></param>
        /// <param name="bAlt"></param>
        /// <param name="bCtrl"></param>
        /// <param name="bShift"></param>
        /// <param name="bWindowKey"></param>
        /// <param name="vkCode"></param>
        /// <returns></returns>
        public delegate long HookedKeyboardUserEventHandler(int iKeyWhatHappened, int vkCode);

        // 후킹된 모듈의 핸들. 후킹이 성공했는지 여부를 식별하기 위해서 사용
        private const int WH_KEYBOARD_LL = 13;		// Intalls a hook procedure that monitors low-level keyboard input events.
        private static long m_hDllKbdHook;
        private static KBDLLHOOKSTRUCT m_KbDllHs = new KBDLLHOOKSTRUCT();
        private static IntPtr m_LastWindowHWnd;
        public static IntPtr m_CurrentWindowHWnd;

        // 후킹한 메시지를 받을 이벤트 핸들러
        private static HookedKeyboardEventHandler m_LlKbEh = new HookedKeyboardEventHandler(HookedKeyboardProc);

        // 콜백해줄 이벤트 핸들러 ; 사용자측에 이벤트를 넘겨주기 위해서 사용
        private static HookedKeyboardUserEventHandler m_fpCallbkProc = null;



        #region KBDLLHOOKSTRUCT Documentation
        /// <summary>
        /// The KBDLLHOOKSTRUCT structure contains information about a low-level keyboard input event. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/hooks_0cxe.htm">KBDLLHOOKSTRUCT</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        /// typedef struct KBDLLHOOKSTRUCT {
        ///     DWORD     vkCode;
        ///     DWORD     scanCode;
        ///     DWORD     flags;
        ///     DWORD     time;
        ///     ULONG_PTR dwExtraInfo;
        ///     ) KBDLLHOOKSTRUCT, *PKBDLLHOOKSTRUCT;
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        private struct KBDLLHOOKSTRUCT
        {
            #region vkCode
            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            /// </summary>
            #endregion
            public int vkCode;
            #region scanCode
            /// <summary>
            /// Specifies a hardware scan code for the key. 
            /// </summary>
            #endregion
            public int scanCode;
            #region flags
            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            /// <remarks>
            /// For valid flag values and additional information, see <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/hooks_0cxe.htm">MSDN Documentation for KBDLLHOOKSTRUCT</a>
            /// </remarks>
            #endregion
            public int flags;
            #region time
            /// <summary>
            /// Specifies the time stamp for this message. 
            /// </summary>
            #endregion
            public int time;
            #region dwExtraInfo
            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            #endregion
            public IntPtr dwExtraInfo;

            #region ToString()
            /// <summary>
            /// Creates a string representing the values of all the variables of an instance of this structure.
            /// </summary>
            /// <returns>A string</returns>
            #endregion
            public override string ToString()
            {
                string temp = "KBDLLHOOKSTRUCT\r\n";
                temp += "vkCode: " + vkCode.ToString() + "\r\n";
                temp += "scanCode: " + scanCode.ToString() + "\r\n";
                temp += "flags: " + flags.ToString() + "\r\n";
                temp += "time: " + time.ToString() + "\r\n";
                return temp;
            }
        }//end of structure

        #region CopyMemory Documentation
        /// <summary>
        /// The CopyMemory function copies a block of memory from one location to another. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/memory/memman_0z95.htm">CopyMemory</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        /// VOID CopyMemory(
        ///		PVOID Destination,   // copy destination
        ///		CONST VOID* Source,  // memory block
        ///		SIZE_T Length        // size of memory block
        ///		);
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto)]
        private static extern void CopyMemory(ref KBDLLHOOKSTRUCT pDest, IntPtr pSource, long cb);

        #region GetForegroundWindow Documentation
        /// <summary>
        /// The GetForegroundWindow function returns a handle to the foreground window (the window with which the user is currently working).
        /// The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/windows_4f5j.htm">GetForegroundWindow</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        /// HWND GetForegroundWindow(VOID);
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();

        #region GetAsyncKeyState
        /// <summary>
        /// The GetAsyncKeyState function determines whether a key is up or down at the time the function is called,
        /// and whether the key was pressed after a previous call to GetAsyncKeyState. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/keybinpt_1x0l.htm">GetAsyncKeyState</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        ///	SHORT GetAsyncKeyState(
        ///		int vKey   // virtual-key code
        ///		);
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetAsyncKeyState(int vKey);

        #region CallNextHookEx Documentation
        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/hooks_57aw.htm">CallNextHookEx</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        /// LRESULT CallNextHookEx(
        ///    HHOOK hhk,      // handle to current hook
        ///    int nCode,      // hook code passed to hook procedure
        ///    WPARAM wParam,  // value passed to hook procedure
        ///    LPARAM lParam   // value passed to hook procedure
        ///    );
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long CallNextHookEx(long hHook, long nCode, long wParam, IntPtr lParam);

        #region SetWindowsHookEx Documentation
        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events.
        /// These events are associated either with a specific thread or with all threads in the same
        /// desktop as the calling thread. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/hooks_7vaw.htm">SetWindowsHookEx</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        ///  HHOOK SetWindowsHookEx(
        ///		int idHook,        // hook type
        ///		HOOKPROC lpfn,     // hook procedure
        ///		HINSTANCE hMod,    // handle to application instance
        ///		DWORD dwThreadId   // thread identifier
        ///		);
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long SetWindowsHookEx(int idHook, HookedKeyboardEventHandler lpfn, long hmod, int dwThreadId);

        #region UnhookWindowsEx Documentation
        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <a href="ms-help://MS.VSCC/MS.MSDNVS/winui/hooks_6fy0.htm">UnhookWindowsHookEx</a><BR/>
        /// </para>
        /// <para>
        /// <code>
        /// [C++]
        /// BOOL UnhookWindowsHookEx(
        ///    HHOOK hhk   // handle to hook procedure
        ///    );
        /// </code>
        /// </para>
        /// </remarks>
        #endregion
        [DllImport(@"user32.dll", CharSet = CharSet.Auto)]
        private static extern long UnhookWindowsHookEx(long hHook);


        // Valid return for nCode parameter of LowLevelKeyboardProc
        private const int HC_ACTION = 0;
        private static long HookedKeyboardProc(int nCode, int wParam, IntPtr lParam)
        {
            long lResult = 0;

            if (nCode == HC_ACTION) //LowLevelKeyboardProc
            {
                //visusl studio 2008 express 버전에서는 빌드 옵션에서 안전하지 않은 코드 허용에 체크
                unsafe
                {
                    //도대체 어디서 뭘 카피해놓는다는건지 이거 원..
                    CopyMemory(ref m_KbDllHs, lParam, sizeof(KBDLLHOOKSTRUCT));
                }

                //전역 후킹을 하기 위해서 현재 활성화 된 윈도우의 핸들값을 찾는다.
                //그래서 이 윈도우에서 발생하는 이벤트를 후킹해야 전역후킹이 가능해진다.
                m_CurrentWindowHWnd = GetForegroundWindow();

                //후킹하려는 윈도우의 핸들을 방금 찾아낸 핸들로 바꾼다.
                if (m_CurrentWindowHWnd != m_LastWindowHWnd)
                    m_LastWindowHWnd = m_CurrentWindowHWnd;

                // 이벤트 발생
                if (m_fpCallbkProc != null)
                {
                    lResult = m_fpCallbkProc(m_KbDllHs.flags, m_KbDllHs.vkCode);
                }

            }
            else if (nCode < 0) //나머지는 그냥 통과시킨다.
            {
                #region MSDN Documentation on return conditions
                // "If nCode is less than zero, the hook procedure must pass the message to the 
                // CallNextHookEx function without further processing and should return the value 
                // returned by CallNextHookEx. "
                // ...
                // "If nCode is greater than or equal to zero, and the hook procedure did not 
                // process the message, it is highly recommended that you call CallNextHookEx 
                // and return the value it returns;"
                #endregion
                return CallNextHookEx(m_hDllKbdHook, nCode, wParam, lParam);
            }

            //
            //lResult 값이 0이면 후킹 후 이벤트를 시스템으로 흘려보내고
            //0이 아니면 후킹도 하고 이벤트도 시스템으로 보내지 않는다.
            return lResult;
        }

        // 후킹 시작
        public static bool Hook(HookedKeyboardUserEventHandler callBackEventHandler)
        {
            bool bResult = true;
            m_hDllKbdHook = SetWindowsHookEx(
                (int)WH_KEYBOARD_LL,
                m_LlKbEh,
                Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(),
                0);

            if (m_hDllKbdHook == 0)
            {
                bResult = false;
            }
            // 외부에서 KeyboardHooker의 이벤트를 받을 수 있도록 이벤트 핸들러를 할당함
            KeyboardHooker.m_fpCallbkProc = callBackEventHandler;

            return bResult;
        }

        // 후킹 중지
        public static void UnHook()
        {
            //프로그램 종료 시점에서 호출해주자.
            UnhookWindowsHookEx(m_hDllKbdHook);
        }

    }//end of class(KeyboardHooker)
}
