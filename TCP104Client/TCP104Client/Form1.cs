using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TcpCSFramework;
using TCP104Library;
using System.Timers;

namespace TCP104Client
{
    public partial class Form1 : Form
    {
        //byte[] remainderBuffer = new byte[] { };//大小可变的缓存器, 处理粘包、半包问题         
        static BindingList<string> msgList = new BindingList<string>();
        int length = 0;
        TcpCli cli1 = null;
        string[] recvarray;    
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            length = 0;

            cli1 = new TcpCli(new Coder(Coder.EncodingMethod.ASCII));
            //cli1.Resovlver = new DatagramResolver("16");//这里的设置没啥用，因为我不准备使用它的解析器。
            cli1.ReceivedDatagram += new NetEvent(RecvData);
            cli1.DisConnectedServer += new NetEvent(ClientClose);
            cli1.ConnectedServer += new NetEvent(ClientConn);
        }
       

        //连接
        private void ClientConn(object sender, NetEventArgs e)
        {
            string info = string.Format("A Client:{0} connect server :{1}", e.Client,
            e.Client.ClientSocket.RemoteEndPoint.ToString());

            Console.WriteLine(info);
            Console.Write(">");
        }
        //断开
        private void ClientClose(object sender, NetEventArgs e)
        {
            string info;

            if (e.Client.TypeOfExit == Session.ExitType.ExceptionExit)
            {
                info = string.Format("A Client Session:{0} Exception Closed.",
                 e.Client.ID);
            }
            else
            {
                info = string.Format("A Client Session:{0} Normal Closed.",
                 e.Client.ID);
            }

            Console.WriteLine(info);
            Console.Write(">");
        }
        //收
        private void RecvData(object sender, NetEventArgs e)
        {
            //显示接收到的数据

            //其实应该在这里做解析和反馈

            //记录信息

            //反馈
            //显示接收到的数据
            int count = e.Client.RecvDataBuffer.Length;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < e.Client.RecvDataBuffer.Length; i++)
            {
                if (i == 1)
                {
                    count = e.Client.RecvDataBuffer[i] + 1;

                }
                if (i <= count)
                {
                    sb.Append(e.Client.RecvDataBuffer[i].ToString("X2") + " ");
                }
            }
            byte[] temp = temp = new byte[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                temp[i] = e.Client.RecvDataBuffer[i];
            }
            //控制遥测
            if (temp[6].Equals(16)&&temp[9].Equals(33))
            {
                byte[] temp1 = temp1 = new byte[4];
                byte[] temp2 = temp2 = new byte[4];
                byte[] temp3 = temp3 = new byte[4];
                byte[] temp4 = temp4 = new byte[4];
                byte[] temp5 = temp5 = new byte[4];
                byte[] temp6 = temp6 = new byte[4];

                temp1 = setByte(16, temp);
                temp2 = setByte(20, temp);
                temp3 = setByte(24, temp);
                temp4 = setByte(28, temp);
                temp5 = setByte(32, temp);
                temp6 = setByte(36, temp);
                a1TextBox.Text = BitConverter.ToSingle(temp1, 0).ToString();
                zeroTextBox.Text = BitConverter.ToSingle(temp2, 0).ToString();
                b1TextBox.Text = BitConverter.ToSingle(temp3, 0).ToString();
                abTextBox.Text = BitConverter.ToSingle(temp4, 0).ToString();
                c1TextBox.Text = BitConverter.ToSingle(temp5, 0).ToString();
                volTextBox.Text = BitConverter.ToSingle(temp6, 0).ToString();
            }
            string info = string.Format("omegaC Received data:{0} From:{1}.", sb.ToString(), e.Client);
            string recv = sb.ToString();
            recvarray = recv.Split(' ');
            Console.Write(">");
            Console.WriteLine(info);
            listBox1.Items.Add(info);
            listBox1.Items.Add(recvarray[0]);
            UnPack();
        }
        //
        private byte[] setByte(int k, byte[] temp)
        {
            byte[] temp2 = new byte[4];
            for (int i = k; i < k + 4; i++)
            {
                temp2[i - k] = temp[i];
            }
            return temp2;
        }

        //解包
        public void UnPack()
        {
            if (recvarray[0].CompareTo("68") == 0)
            {
                switch (recvarray[6])
                {
                    case "00"://无
                        {
                            switch (recvarray[9])
                            {
                                case "25"://断开连接  
                                    {
                                        InternetStatus.Text = "断开";
                                        MessageBox.Show("控制端已断开连接");
                                    }
                                    break;
                                case "27"://请求连接  
                                    {
                                        InternetStatus.Text = "连接";
                                    }
                                    break;
                            }
                        }
                        break;
                    case "01"://测
                        {
                            switch (recvarray[9])
                            {
                                case "21"://遥测  
                                    {
                                        Teltest();
                                    }
                                    break;
                                case "22"://遥信
                                    {
                                        AsyncSndMsg();
                                    }
                                    break;
                                case "24"://延时
                                    {
                                        Acetime();
                                    }
                                    break;
                            }
                        }
                        break;
                    case "10"://控
                        {
                            switch (recvarray[9])
                            {
                                case "23"://遥控控制
                                    {
                                        switch (recvarray[15])
                                        {
                                            case "00"://分闸
                                                {
                                                    switchCheckBox.CheckState = CheckState.Unchecked;
                                                }
                                                break;

                                            case "01"://合闸
                                                {
                                                    switchCheckBox.CheckState = CheckState.Checked;
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        //遥信
        public void AsyncSndMsg()
        { 
            //发送遥信命令
            //Session client = (Session)svr.SessionTable[new SessionId(port)];
            List<byte> byteSource2 = new List<byte>();
            byteSource2.Add(0x68);//启动

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00);//控制域

            byteSource2.Add(0x00);//类型标识,00无返回，01测，10控
            byteSource2.Add(0x01);//可变结构限定
            byteSource2.Add(0x00); byteSource2.Add(0x22);//传送原因，21遥测，22遥信，23遥控,24延时
            byteSource2.Add(0x00); byteSource2.Add(0x01);//公共地址

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x10);//信息对象地址
            if (switchCheckBox.CheckState == CheckState.Unchecked)
            {
                byteSource2.Add(0x00); //信息元素集,遥控
            }
            else
            {
                byteSource2.Add(0x01);
            }
            byteSource2.Add(0x00000000); byteSource2.Add(0x00000000); byteSource2.Add(0x00000000); byteSource2.Add(0x00000000); byteSource2.Add(0x00000000); byteSource2.Add(0x00000000);//遥测
            byteSource2.Insert(1, (byte)(byteSource2.Count() - 1));//长度 

            byte[] data = byteSource2.ToArray();

            StringBuilder sb = GetSb(data);
            string info = string.Format("omegaC Send data:{0}.", sb.ToString());
           // string info = "发送成功";
            if (info == null)
                MessageBox.Show("请确保该服务端已正确连接！");
            else
            {
                listBox1.Items.Add(info);
                cli1.Send(data);

            }   
        }
        //遥测
        public void Teltest()
        {
            byte[] b1, b2, b3, b4, b5, b6;
            try
            {
                float a1 = Convert.ToSingle(a1TextBox.Text.ToString());
                b1 = BitConverter.GetBytes(a1);

                float a2 = Convert.ToSingle(zeroTextBox.Text.ToString());
                b2 = BitConverter.GetBytes(a2);

                float a3 = Convert.ToSingle(b1TextBox.Text.ToString());
                b3 = BitConverter.GetBytes(a3);

                float a4 = Convert.ToSingle(abTextBox.Text.ToString());
                b4 = BitConverter.GetBytes(a4);

                float a5 = Convert.ToSingle(c1TextBox.Text.ToString());
                b5 = BitConverter.GetBytes(a5);

                float a6 = Convert.ToSingle(volTextBox.Text.ToString());
                b6 = BitConverter.GetBytes(a6);
            }
            catch
            {
                MessageBox.Show("请确保该数据有效！");
                return;           
            }
            //发送遥测信息
            List<byte> byteSource2 = new List<byte>();
            byteSource2.Add(0x68);//启动

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00);//控制域

            byteSource2.Add(0x00);//类型标识,00无返回，01测，10控
            byteSource2.Add(0x01);//可变结构限定
            byteSource2.Add(0x00); byteSource2.Add(0x21);//传送原因，21遥测，22遥信，23遥控,24延时，25断开连接，26发信息,27请求连接
            byteSource2.Add(0x00); byteSource2.Add(0x00);//公共地址，00 为被控制端，01为控制端

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x10);//信息对象地址，固定
             byteSource2.Add(0x00); //信息元素集,遥控
             AddByte(AddByte(AddByte(AddByte(AddByte(AddByte(byteSource2, b1), b2), b3), b4), b5), b6);//遥信
            byteSource2.Insert(1, (byte)(byteSource2.Count() - 1));//长度 

            byte[] data = byteSource2.ToArray();
            //string info = "发送成功";
            StringBuilder sb = GetSb(data);
            string info = string.Format("omegaC Send data:{0}.", sb.ToString());
            if (info == null)
                MessageBox.Show("请确保该服务端已正确连接！");
            else
            {
                listBox1.Items.Add(info);
                cli1.Send(data);
            }        
        }
        //拼接
        private List<byte> AddByte(List<byte> byteSource, Byte[] a)
        {
               for (int i = 0; i < a.Length; i++)
               {
                   byteSource.Add(a[i]);
               }
               return byteSource;
        }

        //延时
        public void Acetime()
        {
            List<byte> byteSource2 = new List<byte>();
            byteSource2.Add(0x68);//启动

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00);//控制域

            byteSource2.Add(0x00);//类型标识,01下行，10上行
            byteSource2.Add(0x01);//可变结构限定
            byteSource2.Add(0x00); byteSource2.Add(0x24);//传送原因，21遥测，22遥信，23遥控，24延时，25断开连接，26发信息
            byteSource2.Add(0x00); byteSource2.Add(0x01);//公共地址

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x10);//信息对象地址
            byteSource2.Add(0x00); //信息元素集,遥控
            //AddByte(AddByte(AddByte(AddByte(AddByte(AddByte(byteSource2, b1), b2), b3), b4), b5), b6);//遥信
            byteSource2.Insert(1, (byte)(byteSource2.Count() - 1));//长度 

            byte[] data = byteSource2.ToArray();
            StringBuilder sb = GetSb(data);
            string info = string.Format("omegaC Send data:{0}.", sb.ToString());
            //string info = "发送成功";
            if (info == null)
                MessageBox.Show("请确保该服务端已正确连接！");
            else
            {
                listBox1.Items.Add(info);
                cli1.Send(data);
            }        
        }
        public StringBuilder GetSb(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            int count = data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 1)
                {
                    count = data[i] + 1;

                }
                if (i <= count)
                {
                    sb.Append(data[i].ToString() + " ");
                }
            }
            return sb;
        }
        //关闭连接
        public void Closecon()
        {
            //发送关闭信息
            List<byte> byteSource2 = new List<byte>();
            byteSource2.Add(0x68);//启动

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00);//控制域

            byteSource2.Add(0x00);//类型标识,01下行，10上行
            byteSource2.Add(0x01);//可变结构限定
            byteSource2.Add(0x00); byteSource2.Add(0x25);//传送原因，21遥测，22遥信，23遥控,24延时,25断开连接
            byteSource2.Add(0x00); byteSource2.Add(0x01);//公共地址

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x10);//信息对象地址
            byteSource2.Add(0x00); //信息元素集,遥控
            //AddByte(AddByte(AddByte(AddByte(AddByte(AddByte(byteSource2, b1), b2), b3), b4), b5), b6);//遥信
            byteSource2.Insert(1, (byte)(byteSource2.Count() - 1));//长度 

            byte[] data = byteSource2.ToArray();
            StringBuilder sb = GetSb(data);//显示发送的消息
            string info = string.Format("omegaC Send data:{0}.", sb.ToString());
            //string info = "发送成功";
            if (info == null)
                MessageBox.Show("请确保该服务端已正确连接！");
            else
            {
                listBox1.Items.Add(info);
                cli1.Send(data);
            }       
        }
       //private APDUClass OnReceive(APDUClass apdu)
        //{
        //    APDUClass feedback = null;
        //    switch (apdu.GetApciType())
        //    {
        //        case APCIClass.UISFormat.U:
        //            OnUMessage(apdu);
        //            break;
        //        case APCIClass.UISFormat.I:
        //            OnIMessage(apdu);
        //            break;
        //        case APCIClass.UISFormat.S:
        //            OnSMessage(apdu);
        //            break;
        //        default:
        //            break;
        //    }
        //    return feedback;
        //}
        //private void OnUMessage(APDUClass apdu)
        //{ }
        //private void OnIMessage(APDUClass apdu)
        //{
        //    //回复镜像报文

        //    switch (apdu.Res)
        //    {
        //        case ASDUClass.TransRes.UnDef:
        //            break;
        //        case ASDUClass.TransRes.Rate:
        //            break;
        //        case ASDUClass.TransRes.BackGroundScan:
        //            break;
        //        //case ASDUClass.TransRes.AutoSend:
        //        //    Send(siteModel.AutoSend(apdu));
        //        //    break;
        //        case ASDUClass.TransRes.Init:
        //            break;
        //        case ASDUClass.TransRes.Request:
        //            break;
        //        case ASDUClass.TransRes.Active:
        //            //回复激活确认、回复第一组召唤、回复第二组召唤、回复第三组召唤、回复激活终止
        //            Send(siteModel.OnActive(apdu));
        //            AddMessage(siteModel.Log);
        //            Send(siteModel.OnRes001(apdu));
        //            AddMessage(siteModel.Log);
        //            Send(siteModel.OnRes002(apdu));
        //            AddMessage(siteModel.Log);
        //            Send(siteModel.OnRes003(apdu, siteModel.SwitchStatus));
        //            AddMessage(siteModel.Log);
        //            Send(siteModel.StopActive(apdu));
        //            AddMessage(siteModel.Log);
        //            break;
        //        case ASDUClass.TransRes.ActiveConfirm:
        //            break;
        //        case ASDUClass.TransRes.ActiveStop:
        //            break;
        //        case ASDUClass.TransRes.ActiveStopConfirm:
        //            break;
        //        case ASDUClass.TransRes.ActiveEnd:
        //            break;
        //        case ASDUClass.TransRes.ResAll:
        //            break;
        //        case ASDUClass.TransRes.Res002:
        //            //处理报文
        //            Send(siteModel.OnRes002(apdu));
        //            AddMessage(siteModel.Log);
        //            break;
        //        case ASDUClass.TransRes.Res003:
        //            //处理报文
        //            Send(siteModel.OnRes003(apdu, siteModel.SwitchStatus));
        //            AddMessage(siteModel.Log);
        //            break;
        //        case ASDUClass.TransRes.Telecontrolling:
        //            //处理报文
        //            Send(siteModel.OnTelecontrolling(apdu));
        //            AddMessage(siteModel.Log);
        //            //修改完成后上传第三组数据
        //            Send(siteModel.OnRes003(apdu, siteModel.SwitchStatus));
        //            AddMessage(siteModel.Log);
        //            break;
        //        default:
        //            break;
        //    }
        //}
        //private void OnSMessage(APDUClass apdu)
        //{ }

        //void Send(APDUClass apdu)
        //{

        //}

        #region 记录客户端事件
        public static void AddMessage(string str)
        {
            if (str == null || str == string.Empty)
            {
                return;
            }
            msgList.Add(str);
        }
        #endregion
        //连接服务端
        private void button1_Click(object sender, EventArgs e)
        {
            string conn = textBox1.Text;
            ushort port = ushort.Parse(textBox2.Text);
            cli1.Connect(conn, port);
            //InternetStatus.Text = "已连接";
            panel1.Visible = true;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("连接已经关闭！！！", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            try
            {
                Closecon();
                cli1.Close();
                InternetStatus.Text = "断开";
            }
            catch
            {}
        }
       //发送信息
        private void button2_Click(object sender, EventArgs e)
        {
            byte[] data1 = Encoding.UTF8.GetBytes(contenttextBox.Text.ToString());
            List<byte> byteSource2 = new List<byte>();
            byteSource2.Add(0x68);//启动

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x00);//控制域

            byteSource2.Add(0x00);//类型标识,01下行，10上行
            byteSource2.Add(0x01);//可变结构限定
            byteSource2.Add(0x00); byteSource2.Add(0x26);//传送原因，21遥测，22遥信，23遥控，24延时，25断开连接，26发信息
            byteSource2.Add(0x00); byteSource2.Add(0x01);//公共地址

            byteSource2.Add(0x00); byteSource2.Add(0x00); byteSource2.Add(0x10);//信息对象地址
            byteSource2.Add(0x00); //信息元素集,遥控
            AddByte(byteSource2, data1);//发送信息
            byteSource2.Insert(1, (byte)(byteSource2.Count() - 1));//长度 

            byte[] data = byteSource2.ToArray();
            cli1.Send(data);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //InternetStatus.BackColor = System.Drawing.Color.Transparent;
        }

        private void Closebutton_Click(object sender, EventArgs e)
        {
            try
            {
                Closecon();
                cli1.Close();
                InternetStatus.Text = "断开";
            }
            catch 
            {
                MessageBox.Show("还未建立连接！！！");
            
            }
        }
    }
}
