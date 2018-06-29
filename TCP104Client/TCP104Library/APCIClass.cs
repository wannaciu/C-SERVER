using System;
using System.Collections.Generic;
using System.Text;

namespace TCP104Library
{
    /// <summary>
    /// APCI基类型
    /// </summary>
    public abstract class APCIClass
    {
        public enum UISFormat
        {
            U,
            I,
            S
        }
        #region 数据结构
        /// <summary>
        /// 起始位
        /// </summary>
        protected readonly byte StartByte = 0x68;
        /// <summary>
        /// APDU长度
        /// </summary>
        protected byte apdu_Lenth;
        /// <summary>
        /// 控制域
        /// </summary>
        protected byte[] ControlByte = new byte[4] { 0, 0, 0, 0 };
        private short? _nr;
        public short? Nr
        {
            get { return _nr; }
            set 
            {
                if ((value != null)&&(value < 0))
                {
                    throw new Exception("NR越限");
                }
                else
                {
                    _nr = value;
                }
            }
        }
        private short? _sr;
        public short? Sr
        {
            get { return _sr; }
            set 
            {

                if ((value != null)&&(value < 0))
                {
                    throw new Exception("SR越限");
                }
                else
                {
                    _sr = value;
                }
            }
        }
        private UISFormat uisType;
        public UISFormat UisType
        {
            get { return uisType; }
            set { uisType = value; }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 转换成字符串方法
        /// </summary>
        /// <returns>结果字符串</returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append("0x" + StartByte.ToString("X2") + " ");
            res.Append("0x" + apdu_Lenth.ToString("X2") + " ");
            for (int i = 0; i < 4; i++)
            {
                res.Append("0x" + ControlByte[i].ToString("X2") + " ");
            }
            return res.ToString();
        }
        /// <summary>
        /// 转换成数组的方法
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return new byte[6] 
            { 
                this.StartByte, 
                this.apdu_Lenth, 
                this.ControlByte[0], 
                this.ControlByte[1], 
                this.ControlByte[2], 
                this.ControlByte[3] 
            };
        }
        /// <summary>
        /// 从纯数组获取apci的方法
        /// </summary>
        /// <param name="apcibuffer">含apci的数组</param>
        /// <returns>apci</returns>
        public static APCIClass GetApci(byte[] apcibuffer)
        {
            if (apcibuffer[0] != 0x68)
            {
                throw new Exception("APCI头不对");
            }
            else if (apcibuffer.Length < 0)
            {
                throw new Exception("APCI长度不对");
            }
            else
            {
                switch (apcibuffer[2] & 0x03)
                {
                    case 3:
                        {
                            if (apcibuffer[1] != 4)
                            {
                                throw new Exception("U格式数据长度异常");
                            }
                            APCIClassUFormat.UFormatType utype = (APCIClassUFormat.UFormatType)apcibuffer[2];
                            APCIClassUFormat apci = new APCIClassUFormat(utype);
                            apci.uisType = UISFormat.U;
                            return apci;
                        }
                    case 1:
                        {
                            if (apcibuffer[1] != 4)
                            {
                                throw new Exception("S格式数据长度异常");
                            }
                            short nr = (short)((short)(apcibuffer[4] >> 1) + (short)((short)apcibuffer[5] << 7));
                            APCIClassSFormat apci = new APCIClassSFormat(nr);
                            apci.uisType = UISFormat.S;
                            return apci;
                        }
                    case 2:
                    case 0:
                        {
                            if (apcibuffer[1] < 4)
                            {
                                throw new Exception("I格式数据长度异常");
                            }
                            short ns = (short)((short)(apcibuffer[2] >> 1) + (short)((short)apcibuffer[3] << 7));
                            short nr = (short)((short)(apcibuffer[4] >> 1) + (short)((short)apcibuffer[5] << 7));
                            APCIClassIFormat apci = new APCIClassIFormat(ns, nr);
                            apci.APDULenth = apcibuffer[1];
                            apci.uisType = UISFormat.I;
                            return apci;
                        }
                    default:
                        throw new Exception("格式解析异常");
                }
            }
        }
        #endregion
    }
    /// <summary>
    /// U格式APCI
    /// </summary>
    public class APCIClassUFormat : APCIClass
    {
        public enum UFormatType
        {
            StartSet = 3 + (1 << 2),
            StartConfirm = 3 + (1 << 3),
            StopSet = 3 + (1 << 4),
            StopConfirm = 3 + (1 << 5),
            TestSet = 3 + (1 << 6),
            TestConfirm = 3 + (1 << 7),
        }
        public APCIClassUFormat(UFormatType type)
        {
            this.Nr = null;
            this.Sr = null;
            this.apdu_Lenth = 4;
            this.ControlByte[0] = Convert.ToByte(type);
            this.UisType = UISFormat.U;
        }
    }
    /// <summary>
    /// I格式APCI
    /// </summary>
    public class APCIClassIFormat : APCIClass
    {
        public APCIClassIFormat(short sr, short nr)
        {
            this.Nr = nr;
            this.Sr = sr;
            this.ControlByte[0] = Convert.ToByte((sr << 1) & 0x00fe);
            this.ControlByte[1] = Convert.ToByte((sr >> 7) & 0x00ff);
            this.ControlByte[2] = Convert.ToByte((nr << 1) & 0x00fe);
            this.ControlByte[3] = Convert.ToByte((nr >> 7) & 0x00ff);
            this.UisType = UISFormat.I;
        }

        public byte APDULenth
        {
            set
            {
                this.apdu_Lenth = value;
            }
        }
    }
    /// <summary>
    /// S格式APCI
    /// </summary>
    public class APCIClassSFormat : APCIClass
    {
        public APCIClassSFormat(short nr)
        {
            this.Nr = nr;
            this.Sr = null;
            this.apdu_Lenth = 4;
            this.ControlByte[0] = 1;
            this.ControlByte[2] = Convert.ToByte((nr << 1) & 0x00fe);
            this.ControlByte[3] = Convert.ToByte((nr >> 7) & 0x00ff);
            this.UisType = UISFormat.S;
        }
    }
}
