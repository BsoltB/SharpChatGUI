using System;
using System.Net;
using SimpleTCP;
namespace SharpChatGUI
{
    public partial class Form1 : Form
    {
        public SimpleTcpClient tcpClient;
        private string server_ip { get; set; }
        private string server_port { get; set; }
        private string nicheng { get; set; }

        public bool connected = false;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            x = this.Width;
            y = this.Height;
            setTag(this);
        }

        private float x = 816;//当前窗体的宽度
        private float y = 489;//当前窗体的高度
        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0)
                {
                    setTag(con);
                }
            }
        }
        private void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    string[] mytag = con.Tag.ToString().Split(new char[] { ';' });
                    //根据窗体缩放的比例确定控件的值
                    con.Width = Convert.ToInt32(System.Convert.ToSingle(mytag[0]) * newx);//宽度
                    con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                    con.Left = Convert.ToInt32(System.Convert.ToSingle(mytag[2]) * newx);//左边距
                    con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                    Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                    con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    if (con.Controls.Count > 0)
                    {
                        setControls(newx, newy, con);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tcpClient = new SimpleTcpClient();
            tcpClient.DataReceived += OnDataReceived;
            panel1.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"imgs\background.png");
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
        }


        public static Image get_Fill_image(string url)
        {
            Image image = null;
            WebRequest webreq = WebRequest.Create(url);
            WebResponse webres = webreq.GetResponse();
            using (Stream stream = webres.GetResponseStream())
            {
                image = Image.FromStream(stream);
            }
            return image;
        }

        public static string GetLocalIp()
        {
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        
        private void OnDataReceived(object? sender, SimpleTCP.Message e)
        {
            string msgstr = e.MessageString;
            if(msgstr.Contains(":image_"))
            {
                string urll = msgstr[msgstr.IndexOf("http")..];
                string nichengg = msgstr[..msgstr.IndexOf(":image_")];
                richTextBox1.AppendText(nichengg + "\n");
                Image image = get_Fill_image(urll);
                Thread t = new Thread((ThreadStart)(() =>
                {
                    // 将出现异常的语句放到这里面
                    Clipboard.SetDataObject(image);
                    if (richTextBox1.CanPaste(DataFormats.GetFormat(DataFormats.Bitmap)))
                    {
                        richTextBox1.Paste();
                        //Clipboard.SetDataObject(String.Empty, false);
                    }
                }));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
            }
            else
            {
                richTextBox1.AppendText(msgstr);
            }
            richTextBox1.AppendText("\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text)) return;
            if (string.IsNullOrEmpty(textBox2.Text)) return;
            if (string.IsNullOrEmpty(textBox3.Text)) return;
            server_ip = textBox1.Text;
            server_port = textBox2.Text;
            nicheng = textBox3.Text;
            try
            {
                tcpClient.Connect(server_ip, int.Parse(server_port));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            label5.Text = "连接状态：在线";
            label5.ForeColor = Color.Green;
            connected = true;
            //告诉服务器自己的昵称
            tcpClient.Write("connect_" + nicheng);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tcpClient.Disconnect();
            label5.Text = "连接状态：离线";
            label5.ForeColor = Color.OrangeRed;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(connected)
            {
                if(!string.IsNullOrEmpty(richTextBox2.Text))
                {
                    string msg = richTextBox2.Text;
                    try
                    {
                        tcpClient.Write(msg);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("你还没有链接服务器！please连接！","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("如果你想换背景，请在根目录下的img文件夹加上你需要的背景（需命名为background.png）！", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            float newx = (this.Width) / x;
            float newy = (this.Height) / y;
            setControls(newx, newy, this);
        }
    }
}