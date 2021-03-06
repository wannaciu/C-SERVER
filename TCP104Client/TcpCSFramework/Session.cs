using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace TcpCSFramework
{
    /// <summary>
    /// 客户端与服务器之间的会话类
    /// </summary>
    public class Session : ICloneable
    {
        #region 字段

        /// <summary>
        /// 会话ID
        /// </summary>
        private SessionId _id;

        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private byte[] _recvDataBuffer;
        private int _received;

        /// <summary>
        /// 客户端发送到服务器的报文
        /// 注意:在有些情况下报文可能只是报文的片断而不完整
        /// </summary>
        private string _datagram;

        /// <summary>
        /// 客户端的Socket
        /// </summary>
        private Socket _cliSock;

        /// <summary>
        /// 客户端的退出类型
        /// </summary>
        private ExitType _exitType;

        /// <summary>
        /// 会话通道号
        /// </summary>
        private string _channel = "";

        /// <summary>
        /// 退出类型枚举
        /// </summary>
        public enum ExitType
        {
            NormalExit,
            ExceptionExit
        };

        #endregion

        #region 属性

        /// <summary>
        /// 返回会话的ID
        /// </summary>
        public SessionId ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// 接收数据缓冲区 
        /// </summary>
        public byte[] RecvDataBuffer
        {
            get
            {
                return _recvDataBuffer;
            }
            set
            {
                _recvDataBuffer = value;
            }
        }
        public int Received
        {
            get { return _received; }
            set { _received = value; }
        }
        /// <summary>
        /// 存取会话的报文
        /// </summary>
        public string Datagram
        {
            get
            {
                return _datagram;
            }
            set
            {
                _datagram = value;
            }
        }

        /// <summary>
        /// 获得与客户端会话关联的Socket对象
        /// </summary>
        public Socket ClientSocket
        {
            get
            {
                return _cliSock;
                
            }
        }


        /// <summary>
        /// 存取客户端的退出方式
        /// </summary>
        public ExitType TypeOfExit
        {
            get
            {
                return _exitType;
            }

            set
            {
                _exitType = value;
            }
        }

        /// <summary>
        /// 会话执行的通道号
        /// </summary>
        public string Channel
        {
            get
            {
                return _channel;
            }

            set
            {
                _channel = value;
            }
        }
        
        #endregion

        #region 方法

        /// <summary>
        /// 使用Socket对象的Handle值作为HashCode,它具有良好的线性特征.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)_cliSock.Handle;
        }

        /// <summary>
        /// 返回两个Session是否代表同一个客户端
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Session rightObj = (Session)obj;

            return (int)_cliSock.Handle == (int)rightObj.ClientSocket.Handle;
        }

        /// <summary>
        /// 重载ToString()方法,返回Session对象的特征
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = string.Format("Session:{0},IP:{1}",
             _id, _cliSock.RemoteEndPoint.ToString());

            //result.C
            return result;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cliSock">会话使用的Socket连接</param>
        public Session(Socket cliSock)
        {
            Debug.Assert(cliSock != null);

            _cliSock = cliSock;

            _id = new SessionId((int)cliSock.Handle);
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        public void Close()
        {
            Debug.Assert(_cliSock != null);

            //关闭数据的接受和发送
            _cliSock.Shutdown(SocketShutdown.Both);

            //清理资源
            _cliSock.Close();
        }

        #endregion

        #region ICloneable 成员

        object System.ICloneable.Clone()
        {
            Session newSession = new Session(_cliSock);
            newSession.Datagram = _datagram;
            newSession.TypeOfExit = _exitType;

            return newSession;
        }

        #endregion
    }
}
