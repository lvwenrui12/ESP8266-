﻿    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ESP8266.HelperClass;

namespace ESP8266
{

    
    public partial class Form1 : Form
    {

        private  ATCommand AT=new ATCommand();

        private string strComRead = "";
        private string strComWQrite = "";
        private byte[] byteComRead;

        //创建一个委托，是为访问TextBox控件服务的。
        public delegate void UpdateTxt(string msg,TextBox txtBox);
        public delegate void UpdateListBox(string msg, ListBox txtBox);
        //定义一个委托变量
        public UpdateTxt updateTxt;

        public UpdateListBox updateListBox;

        private Thread ComReadThread;

        private Socket serverSocket;
        private List<StateObject> ClientLists = new List<StateObject>();

        public Form1()
        {
            InitializeComponent();
            comWorkWay.SelectedIndex = 0;
            comBaudRate.SelectedIndex = 0;
            ScanCom();
            updateTxt = new UpdateTxt(UpdateTxtMethod);
            updateListBox=new UpdateListBox(UpdateListBoxMethod);

        }

        private void UpdateListBoxMethod(string msg, ListBox listbox)
        {
            listbox.Items.Add(msg);
        }

        private void ApendText(string msg, TextBox txtbox)
        {
            
        }

        private void BtnScanCom_Click(object sender, EventArgs e)
        {

            ScanCom();
        }

        #region 串口加载

        private void ScanCom()
        {

            foreach (string com in System.IO.Ports.SerialPort.GetPortNames())
            {
                this.comPortName.Items.Add(com);
                comSetPort.Items.Add(com);
            }

            if (comPortName.Items.Count > 0)
            {
                comPortName.SelectedIndex = 0;
                comSetPort.SelectedIndex = 0;
            }
        }

        #endregion

        private void btnOpenCom_Click(object sender, EventArgs e)
        {

            try
            {
                if (this.serialPort1.IsOpen)
                {
                    this.serialPort1.Close();
                }
                else
                {
                    // 设置端口参数
                    this.serialPort1.BaudRate = int.Parse(this.comBaudRate.Text);
                    this.serialPort1.DataBits = 8;
                    this.serialPort1.StopBits = StopBits.One;
                    this.serialPort1.Parity = Parity.None;
                    this.serialPort1.PortName = this.comPortName.Text;
              
                    //打开端口
                    this.serialPort1.Open();
                }
          
                if (this.serialPort1.IsOpen)
                {
                    this.btnOpenCom.Text = "关闭端口";
                    txtComRead.AppendText("串口已经打开");
                }
                else
                {

                    this.btnOpenCom.Text = "打开端口";
                    txtComRead.AppendText("串口已经关闭");
                    this.txtComRead.SelectionStart = this.txtComRead.Text.Length;
                    this.txtComRead.ScrollToCaret();
                }
            }
            catch (Exception er)
            {
                MessageBox.Show("端口打开失败！" + er.Message, "提示");
            }
        }

        private void btnSetWorkWay_Click(object sender, EventArgs e)
        {

            ComDataTransform(serialPort1, AT.WorkWay(comWorkWay.SelectedIndex + 1));
            if (comWorkWay.SelectedIndex==0)
            {
                tabControl1.SelectedIndex = 2;
            }
            if (comWorkWay.SelectedIndex == 1)
            {
                tabControl1.SelectedIndex =1;
            }
            if (comWorkWay.SelectedIndex == 2)
            {
                tabControl1.SelectedIndex =3;
            }
         
        }

        private void ComDataTransform(SerialPort port,string strWitr)
        {
            if (port.IsOpen)
            {
                txtComWrite.Text += strWitr + Environment.NewLine;
                port.WriteLine(strWitr + Environment.NewLine);
                //port.Write(strWitr);
                this.txtComWrite.SelectionStart = this.txtComWrite.Text.Length;
                this.txtComWrite.ScrollToCaret();

                //Thread.Sleep(200);
                //txtComRead.Text += port.ReadExisting() + Environment.NewLine;
                //this.txtComRead.SelectionStart = this.txtComRead.Text.Length;
                //this.txtComRead.ScrollToCaret();


            }
            else
            {
                MessageBox.Show("请打开串口");
            }
           
        }

        private void tbnClearSend_Click(object sender, EventArgs e)
        {
            txtComWrite.Text = "";
        }

        private void btnClearRece_Click(object sender, EventArgs e)
        {
            txtComRead.Text = "";
        }

        private void btnSetBaudRate_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.BaudRate);
          
        }

        private void BtnScanCom_Click_1(object sender, EventArgs e)
        {
            ScanCom();
        }

        private void btnSetApIP_Click(object sender, EventArgs e)
        {


            SetAp_IP(txtApIP);
        }

        private void SetAp_IP(TextBox txtBox)
        {
            IPAddress ip;
            if (IPAddress.TryParse(txtBox.Text.Trim(), out ip))
            {
                ComDataTransform(serialPort1, AT.Ap_ip(ip.ToString()));
            }
            else
            {
                MessageBox.Show("请输入正确的ip地址");
            }
        }

        private void tbnClearSend_Click_1(object sender, EventArgs e)
        {
            txtComWrite.Text = "";
        }

        private void btnClearRece_Click_1(object sender, EventArgs e)
        {
            txtComRead.Text = "";
        }

        private void btnReBoot_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1,AT.ReBoot);
        }

        private void btnApStartServer_Click(object sender, EventArgs e)
        {
            int port = 0;
            if (int.TryParse(txtApPort.Text,out port))
            {
                ComDataTransform(serialPort1, AT.Ap_StartServer(port));
            }
            else
            {

                MessageBox.Show("请输入数字");
            }
            
        }

        private void btnApCloseServer_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.Ap_StopServer);
        }

        private void btnSetMuil_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.MutiLink);
        }

        private void btnSetStaIp_Click(object sender, EventArgs e)
        {
            SetStationIp(txtStaIp);
        }

        private void SetStationIp(TextBox txtBox)
        {
            IPAddress ip;
            if (IPAddress.TryParse(txtBox.Text.Trim(), out ip))
            {
                ComDataTransform(serialPort1,AT.Sta_ip(ip.ToString()));
            }
            else
            {
                MessageBox.Show("请输入正确IP地址");

            }
        }

        private void btnApTimeOut_Click(object sender, EventArgs e)
        {
            int timeout = 0;
            if (int.TryParse(txtApTimeOut.Text,out timeout))
            {
                ComDataTransform(serialPort1,AT.Ap_ServerTimeOut(timeout));
            }
            else
            {
                MessageBox.Show("请输入数字");
            }
        }

        private void SetApIP_Click(object sender, EventArgs e)
        {
            SetAp_IP(txtAp_Ip);
        }

        private void SetStaIp_Click(object sender, EventArgs e)
        {
            SetStationIp(txtSta_IP);
        }
        private Socket clientSocket;
        private ManualResetEvent connectDone = new ManualResetEvent(false);

        private void btnTcpConnect_Click(object sender, EventArgs e)
        {
           

            #region AT命令连接

            IPAddress ip;
            int port;
            if (IPAddress.TryParse(txtTcpSeverIP.Text,out ip)&&int.TryParse(txtServerPort.Text,out port))
            {
                ComDataTransform(serialPort1, AT.TcpConnect(port,ip));
            }
            else
            {
                MessageBox.Show("请输入正确ip地址或者端口号");
            }

           

            #endregion
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                // 開始非同步接收主機端資料
                if (state.workSocket.Connected)
                {
                    state.workSocket.BeginReceive(state.buffer, 0, state.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {

                    txtTcpRece.BeginInvoke(updateTxt, Environment.NewLine + "连接断开", txtTcpRece);

                }
             
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                if (!client.Connected)
                {
                    txtTcpRece.BeginInvoke(updateTxt, "无法连接服务器", txtTcpRece);
                 
                    return;
                }
                // 完成连接
                client.EndConnect(ar);

                string str = "已连接到服务器: " + client.RemoteEndPoint.ToString();


                if (txtTcpRece.InvokeRequired)
                {
                    txtTcpRece.BeginInvoke(updateTxt,str , txtTcpRece);
                }
                else
                {
                    txtTcpRece.AppendText(str + Environment.NewLine);
                }

                // 狀態設定為未收到訊號
                connectDone.Set();
            }
            catch (Exception e)
            {
                connectDone.Set();
                MessageBox.Show(e.ToString());
            }
        }

    
        public void UpdateTxtMethod(string msg,TextBox txtBox)
        {
            txtBox.Text+= (Environment.NewLine+msg   );

            txtBox.SelectionStart = txtBox.Text.Length;
         
            txtBox.ScrollToCaret();
        }

        private void btnTcpClearSend_Click(object sender, EventArgs e)
        {
            txtSendCmd.Text = "";
        }

        private void btnTcpClearRece_Click(object sender, EventArgs e)
        {
            txtTcpRece.Text = "";
        }

        private void txtTcpSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSendCmd.Text == "")
                {
                    MessageBox.Show("请输入发送信息");
                    return;
                }


                byte[] byteData = Encoding.ASCII.GetBytes(txtSendCmd.Text.Trim());

                if (clientSocket!=null && tabControl3.SelectedIndex==0)
              
                {

                    if ( clientSocket.Connected)
                    {
                     

                        clientSocket.Send(byteData);

                        txtSendCmd.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("与服务器连接断开,请连接");
                    }
                 
                }
                 else if(tabControl3.SelectedIndex == 1)
                 {
                     if (ClientLists.Count>0 && listBoxClient.Items.Count>0)
                     {
                         foreach (var clientList in ClientLists)
                         {

                             if (listBoxClient.SelectedItems.Count>0)
                             {
                                if (clientList.workSocket.LocalEndPoint.ToString() == listBoxClient.SelectedItem.ToString())
                                {
                                    if (clientList.workSocket.Connected)
                                    {
                                        clientList.workSocket.Send(byteData);
                                    }
                                }
                            }
                             
                         }
                     }

                   
                 }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (tabControl1.SelectedIndex == 5)
            //{
            //    tabControl2.SelectedIndex = 1;
            //}
            //else
            //{
            //    tabControl2.SelectedIndex = 0;
            //}
        }

        private void btnUdpConnect_Click(object sender, EventArgs e)
        {
            IPAddress ip;
            int remoPort;
            int locaPort;

            if (IPAddress.TryParse(txtUdpIP.Text,out ip)&& int.TryParse(txtUdpRemotePort.Text,out remoPort )&&int.TryParse(txtUdpLocalPort.Text,out locaPort))
            {
                ComDataTransform(serialPort1,AT.UDPConnect(remoPort,locaPort,ip));
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.CloseConnect(comId.SelectedIndex));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comId.SelectedIndex = 0;
            ComReadThread = new Thread(ComRead);
            ComReadThread.Start();
        }



        #region 监听串口线程调用的方法
        private void ComRead()
        {
            while (true)
            {
                if (serialPort1.IsOpen)
                {

                    if (serialPort1.BytesToRead > 0)
                    {
                        string str = serialPort1.ReadExisting() + Environment.NewLine;

                        if (txtComRead.InvokeRequired)
                        {
                            txtComRead.BeginInvoke(updateTxt, str, txtComRead);
                        }
                        else
                        {
                            txtComRead.AppendText(str);
                            this.txtComRead.SelectionStart = this.txtComRead.Text.Length;
                            this.txtComRead.ScrollToCaret();
                        }
                      
                    }

                }
            }
        } 
        #endregion

        private void btnQueryIp_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1,AT.QuerySelfIp);
        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1,AT.Version);
        }

        private void btnScanWifi_Click(object sender, EventArgs e)
        {
            //AT + CWLAP
            ComDataTransform(serialPort1, AT.ScanWifi);
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.CloseWifi);
        }

        private void btnSingleLinke_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.SingleLink);
        }

        private void btnMultiLink_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.MutiLink);
        }

        private void btnPenetrate_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.Penetrate);
        }

        private void btnNotPenetrate_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.PenetrateNot);
        }

        private void btnQuitPenetrate_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.QuitPenetrate);
        }

        private void btnOpenDog_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.OpenDog);
            
        }

        private void btnCloseDog_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.CloseDog);
          
        }

        private void btnClearDog_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.ClearDog);
        }

        private void btnQueryTimeOut_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.QueryTimeout);
        }

        private void btnQueryTiimeOut_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.QueryWorkWAy);
        }

        private void btnQueryDevice_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.QueryJionDevice);
        }

        private void btnSetWifi_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.SetWifi(txtWifiName.Text,txtWifiPwd.Text, comEncrypt.SelectedIndex));
        }

        private void btnJionWifi_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, AT.JoinWifi(txtJoinWifiName.Text, txtJoinWifiPwd.Text));
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            ComDataTransform(serialPort1, txtSendData.Text);
            ;
        }

        private void btnNetConnect_Click(object sender, EventArgs e)
        {
            #region sokect 连接
            try
            {
                int serverPort = 0;
                IPAddress ip;
                if (int.TryParse(txtNetServerPort.Text, out serverPort) && IPAddress.TryParse(txtNetServerIp.Text, out ip))
                {
                    IPEndPoint remoteEP = new IPEndPoint(ip, serverPort);

                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), clientSocket);


                    connectDone.WaitOne();
                    Receive(clientSocket);


                    tabControl2.SelectedIndex = 1;
                    return;
                }
                else
                {
                    MessageBox.Show("请输入正确的ip地址或者端口号");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            #endregion
        }

        private void btnNetServerStart_Click(object sender, EventArgs e)
        {

            int port = 0;
            if (!int.TryParse(txtLocalServerPort.Text.Trim(),out port))
            {
                MessageBox.Show("请输入正确的端口号");
                return;
            }
         
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Bind(localEP);
                serverSocket.Listen(10);
                txtTcpRece.AppendText("等待客户端连接");
                serverSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), serverSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        
        private void ServerAcceptCallback(IAsyncResult ar)
        {
            Socket handler = ((Socket)ar.AsyncState).EndAccept(ar);
            StateObject user = new StateObject();
            user.workSocket = handler;
            // 添加 User 到 list
            ClientLists.Add(user);
            // 開始接收訊息
            listBoxClient.Invoke(updateListBox, user.workSocket.LocalEndPoint.ToString(), listBoxClient);


            user.workSocket.BeginReceive(user.buffer, 0, user.BufferSize, 0, new AsyncCallback(TcpServerReceiveCallback), user);
            // 持續監聽
            serverSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), serverSocket);
        }

        private void TcpServerReceiveCallback(IAsyncResult ar)
        {
            StateObject user = new StateObject();
            user = (StateObject)ar.AsyncState;
            Socket tmpSocket = user.workSocket;

            // 從客戶端讀取資料
            int bytesRead = user.workSocket.EndReceive(ar);
            if (bytesRead > 0) // 有資料
            {
            
                // 客戶端資料
                String MSG = Encoding.ASCII.GetString(user.buffer);
                txtTcpRece.Invoke(updateTxt, MSG, txtTcpRece);
          
            }
            // 清除舊資料
            user.buffer = new byte[1024];
            // 等待客戶端再次傳送資料
            user.workSocket.BeginReceive(user.buffer, 0, user.BufferSize, 0, new AsyncCallback(ReceiveCallback), user);
        }
    

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = null;
                state = (StateObject)ar.AsyncState;
                Socket server = state.workSocket;
                if (state.workSocket.Connected)
                {
                    // 從主機端讀取資料
                    int bytesRead = server.EndReceive(ar);
                    // 有資料
                    if (bytesRead > 0)
                    {

                        String MSG = Encoding.ASCII.GetString(state.buffer);
                        if (txtTcpRece.InvokeRequired)
                        {
                            txtTcpRece.BeginInvoke(updateTxt, MSG, txtTcpRece);
                        }
                        else
                        {
                            txtTcpRece.AppendText(MSG + Environment.NewLine);
                        }
                    }
                    // 繼續等待主機回傳的資料
                    state.buffer = new byte[state.BufferSize];

                    state.workSocket.BeginReceive(state.buffer, 0, state.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    txtTcpRece.BeginInvoke(updateTxt, Environment.NewLine + "连接断开", txtTcpRece);

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void btnBrodcast_Click(object sender, EventArgs e)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(txtSendCmd.Text);
            // 發佈給所有已連線的客戶端
            foreach (var c in ClientLists)
            {
                if (c.workSocket.Connected)
                {
                    c.workSocket.Send(byteData);
                }
             
            }
           
        }
    }
}

