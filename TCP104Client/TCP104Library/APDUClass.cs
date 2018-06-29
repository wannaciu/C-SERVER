using System;
using System.Collections.Generic;
using System.Text;

namespace TCP104Library
{
    public class APDUClass
    {
        #region 构造函数
        /// <summary>
        /// 屏蔽无参数构造函数
        /// </summary>
        private APDUClass()
        {
            throw new Exception("禁止使用此构造函数");
        }
        /// <summary>
        /// 以APCI+数据构造APDU
        /// </summary>
        /// <param name="apci"></param>
        /// <param name="data"></param>
        public APDUClass(APCIClass apci, ASDUClass asdu)
        {
            this.SetApci(apci);
            this.SetAsdu(asdu);
        }
        /// <summary>
        /// 以纯数组构造APDU
        /// </summary>
        /// <param name="buffer"></param>
        public APDUClass(byte[] buffer)
        {
            this.SetApci(APCIClass.GetApci(buffer));
            if (buffer.Length > 6)
            {
                byte[] data = new byte[buffer.Length - 6];
                if (data.Length < buffer.Length - 6)
                {
                    throw new Exception("初始化数据长度出错");
                }
                try
                {
                    Array.Copy(buffer, 6, data, 0, data.Length);
                    ASDUClass temp = new ASDUClass();
                    _res = temp.UnPack(buffer, 6, buffer.Length - 6);
                    this.SetAsdu(temp);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
        #endregion

        #region 数据结构
        /// <summary>
        /// 传送原因
        /// </summary>
        TCP104Library.ASDUClass.TransRes _res;
        /// <summary>
        /// 传送原因
        /// </summary>
        public TCP104Library.ASDUClass.TransRes Res
        {
            get { return _res; }
            set { _res = value; }
        }
        /// <summary>
        /// apci部分数据
        /// </summary>
        private APCIClass apci;//= new APCIClass();
        /// <summary>
        /// asdu部分数据
        /// </summary>
        private ASDUClass _asdu = null;
        #endregion

        #region 数据使用方法
        /// <summary>
        /// 转换成数组
        /// </summary>
        /// <returns>结果数组</returns>
        public byte[] ToArray()
        {
            byte[] res = new byte[6 + (_asdu == null? 0:_asdu.ToArray().Length)];
            apci.ToArray().CopyTo(res, 0);
            if (_asdu != null)
            {
                _asdu.ToArray().CopyTo(res, 6);
            }
            return res;
        }
        /// <summary>
        /// 设置Apci部分数据
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool SetApci(APCIClass control)
        {
            this.apci = control;
            return true;
        }
        /// <summary>
        /// 设置Asdu部分数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetAsdu(ASDUClass data)
        {
            if (data != null)
            {
                if (this.apci.UisType == APCIClass.UISFormat.I)
                {
                    byte[] temp = data.ToArray();
                    try
                    {
                        if (temp.Length > 250)
                        {
                            (this.apci as APCIClassIFormat).APDULenth = 0x00;
                        }
                        else
                        {
                            (this.apci as APCIClassIFormat).APDULenth = Convert.ToByte(temp.Length + 4);
                        }
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(ex.ToString() + "   " + temp.Length);
                    }
                    this._asdu = data;
                }
                return true;
            }
            else
            {
                _asdu = null;
                if (this.apci.UisType == APCIClass.UISFormat.I)
                {
                    (this.apci as APCIClassIFormat).APDULenth = 4;
                }
                return false;
            }
        }
        /// <summary>
        /// 获取APCI类型
        /// </summary>
        /// <returns>APCI类型</returns>
        public APCIClass.UISFormat GetApciType()
        {
            return apci.UisType;
        }
        /// <summary>
        /// 获取NR值
        /// </summary>
        /// <returns>NR值</returns>
        public short? GetNR()
        {
            return apci.Nr;
        }
        /// <summary>
        /// 获取SR值
        /// </summary>
        /// <returns>SR值</returns>
        public short? GetSR()
        {
            return apci.Sr;
        }
        /// <summary>
        /// 获得数据集和
        /// </summary>
        /// <returns></returns>
        public List<DataStruct> GetData()
        {
            return this._asdu.Data;
        }
        #endregion

        #region 转换成字符串方法
        /// <summary>
        /// 转换成字符串方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.apci.ToString() + (_asdu == null ? null : _asdu.ToString());
        }
        /// <summary>
        /// asdu数据部分转换成字符串
        /// </summary>
        /// <returns></returns>
        public string AsduToString()
        {
            return _asdu == null?null:_asdu.ToString();
        }
        /// <summary>
        /// apci部分转换成字符串
        /// </summary>
        /// <returns></returns>
        public string ApciToString()
        {
            return this.apci.ToString();
        }

        public ASDUClass.FunType GetAsduType()
        {
            return this._asdu.Type;
        }
        #endregion
    }
}
