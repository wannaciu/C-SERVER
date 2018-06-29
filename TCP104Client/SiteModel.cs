using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TransformerSubstation
{
    public class SiteModel
    {
        public SiteModel()
        {
        }
        public SiteModel(string[] info)
        {
            id = info[0];
            title = info[1];
            phone = info[2];
            address = info[3];
            gprs = info[4];
            type = info[5];
            ip = info[6];

            LinkStatus = false;
            Patrol = true;
            SwitchStatus = false;

            session = null;
            Reset();
        }
        private string log;
        public string Log
        {
            get { return log;}
        }
        private string id;    //站点编号
        private string gprs;   //GPRS ID号：针对101协议
        private string phone;   //手机号码
        private string address;   //地址码
        private string title;    //站点名称
        private string type;   //类型
        private string ip;   //ip地址：针对104协议

        private string messagePhone;    //短信手机

        private bool linkStatus; //连接状态
        private bool patrol; //巡查

                
        #region GroupOne
        private float triphaseCTRatio; //三相CT比率
        private float triphasePTRatio; //三相PT 比率
        private float zeroCTRatio;     //零序CT比率        
        #endregion

        #region GroupTwo        
        float primaryCurrent_A;
        float primaryCurrent_B;
        float primaryCurrent_C;
        float primaryCurrent_ZeroOrder;
        float secondaryVoltage_AB;
        float voltageCapacitor;
        #endregion

        #region GroupThree
        private bool switchStatus; //false：分闸；true：合闸
        #endregion

        #region GroupFour
        public DateTime refreshenTime = new DateTime();
        #endregion

        #region NrSr
        private short _nr;     //接收序号
        private short _sr;     //发送序号
        #endregion

        #region Server
        //重置站点状态与数据
        public void Reset()
        {
            linkStatus = false;
            switchStatus = false;
            patrol = true;
            //GroupOne
            triphaseCTRatio = 0.0f;
            triphasePTRatio = 0.0f;
            zeroCTRatio = 0.0f;
            //GroupTwo
            primaryCurrent_A = 1.0f;
            primaryCurrent_B = 2.0f;
            primaryCurrent_C = 3.0f;
            secondaryVoltage_AB = 4.0f;
            primaryCurrent_ZeroOrder = 5.0f; 
            voltageCapacitor = 6.0f;
            //GroupThree
            switchStatus = false;
            //GroupFour
            refreshenTime = DateTime.Today;
            //NrSr
            _nr = 0;
            _sr = 0;
        }

        //总召唤
        public string Active()
        {

        }
        //收到镜像答复
        public string OnAck(APDUClass apdu)
        {
            return null;
        }
        
        //接受激活确认
        public APDUClass OnActiveConfirm(APDUClass apdu)
        {
            return null;
        }
        //激活终止
        public APDUClass OnStopActive(APDUClass apdu)
        {
            return null;
        }

        //遥测: 接受第一组遥测值
        public APDUClass OnRes001Confirm(APDUClass apdu)
        {
            return null;
        }
        
        //遥测: 取第二组遥测值
        public string Res002()
        {
            return log;
        }

        //接受第二组遥测数据
        public APDUClass OnRes002Confirm(APDUClass apdu)
        {
            return null;
        }
        //遥信 
        public string Res003()
        {
            return log;
        }
        //接受第三组(遥信)
        public APDUClass OnRes003Confirm(APDUClass apdu)
        {
            return null;
        }
        //遥控
        public string Telecontrolling(bool bSwitch)
        {
            return log;
        }
        //突发上传
        public string OnAutoSend(APDUClass apdu)
        {
            return null;
        }
        #endregion
/*************************************************************************************************************************************/
        #region Client
        private TcpCli client = null;         //socket引用

        //注意：客户端没有session
        //重置站点状态与数据
        public void OnReset()
        {
            linkStatus = false;
            switchStatus = false;
            patrol = true;
            //GroupOne
            triphaseCTRatio = 0.0f;
            triphasePTRatio = 0.0f;
            zeroCTRatio = 0.0f;
            //GroupTwo
            primaryCurrent_A = 1.0f;
            primaryCurrent_B = 2.0f;
            primaryCurrent_C = 3.0f;
            secondaryVoltage_AB = 4.0f;
            primaryCurrent_ZeroOrder = 5.0f;
            voltageCapacitor = 6.0f;
            //GroupThree
            switchStatus = false;
            //GroupFour
            refreshenTime = DateTime.Today;
            //NrSr
            _nr = 0;
            _sr = 0;
        }

        //回复激活确认:镜像报文
        public APDUClass Ack(APDUClass apdu)
        {
            //检查全站参数，如果没问题，则回复Active...

            return apdu;
        }
        
        public APDUClass OnActive(APDUClass apdu)
        {
            return apduNew;
        }
        //回复第一组召唤: 不理会，以后再实现???
        public APDUClass OnRes001(APDUClass apdu)
        {
            ASDUClass asduNew = new ASDUClass(1);
            
            return apduNew;
        }
        
        //回复第二组召唤
        public APDUClass OnRes002(APDUClass apdu)
        {            
            ASDUClass asduNew = new ASDUClass(1);
            
            return apduNew;
        }

        //回复第三组召唤：实现
        public APDUClass OnRes003(APDUClass apdu, bool bSwitch)
        {
            ASDUClass asduNew = new ASDUClass(1);
            
            return apduNew;

        }
        //回复激活终止, 暂未实现
        public APDUClass StopActive(APDUClass apdu)
        {
            ASDUClass asduNew = new ASDUClass(1);
            //类型标识、根据命令确定可变结构限定词的值、设置数据
            asduNew.Pack(ASDUClass.TransRes.ActiveStop, ASDUClass.FunType.CalAll, 0, 0x14);

            APCIClassIFormat apciNew = new APCIClassIFormat(_sr++, _nr);

            APDUClass apduNew = new APDUClass(apciNew, asduNew); //apci的长度将在此处被设置
            //byte[] bytes = apduNew.ToArray();
            //log = string.Format("Client received data: {0} From: {1}.\n", apdu.ToString(), Client.ClientSession);
            log = string.Format("Stop Active \n Client sended data: {0} to: {1}.\n", apduNew.ToString(), Client.ClientSession);
            return apduNew;
        }

        //响应遥控
        public APDUClass OnTelecontrolling(APDUClass apdu)
        {
            return null;
        }
        //突发上传
        public string AutoSend(APDUClass apdu)
        {
            ASDUClass asduNew = new ASDUClass(1);
            asduNew.Pack(ASDUClass.TransRes.AutoSend, ASDUClass.FunType.Single_point, 0x6001, (double)Convert.ToByte(SwitchStatus));

            APCIClassIFormat apciNew = new APCIClassIFormat(_sr++, _nr);

            APDUClass apduNew = new APDUClass(apciNew, asduNew); //apci的长度将在此处被设置
            byte[] bytes = apduNew.ToArray();
            return log;
        }
        //
        #endregion
    }
}
