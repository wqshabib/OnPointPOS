using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace POSSUM.Sockets
{
    public enum ResultCode
    {
        Ok = 0,
        Error = 1
    }

    public static class XmlUtils
    {
        public static string ToXml(object obj)
        {
            //this avoids xml document declaration
            var settings = new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true
            };

            var stream = new MemoryStream();

            using (XmlWriter xw = XmlWriter.Create(stream, settings))
            {
                var ns = new XmlSerializerNamespaces(
                                   new[] { XmlQualifiedName.Empty });
                var x = new XmlSerializer(obj.GetType(), "");

                x.Serialize(xw, obj, ns);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public class SocketClient
    {
        protected readonly string ip;
        protected readonly int port;
        protected readonly Socket sender;

        protected bool isConnected;
        protected bool isOpen;

        public SocketClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            sender = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);
        }

        public SocketClient()
        {
            sender = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Connect(string ip, int port)
        {
            try
            {
                sender.Connect(ip, port);

                isConnected = true;
            }
            catch (Exception)
            {
                isConnected = false;
            }

            return isConnected;
        }

        public string Reveice()
        {
            var bytes = new byte[1048576];

            sender.ReceiveBufferSize = bytes.Length;

            try
            {
                int bytesRec = sender.Receive(bytes);
            }
            catch (Exception e)
            {
                return null;
            }

            var str = Encoding.Default.GetString(bytes);

            sender.ReceiveBufferSize = 0;

            Debug.WriteLine(str);

            Debug.Flush();

            bytes = new byte[0];

            if (!string.IsNullOrEmpty(str))
            {
                str = Regex.Replace(str, "\0", string.Empty);
                str = Regex.Replace(str, "\r", string.Empty);

                str = Regex.Replace(str, "\n", string.Empty);

                return str.Trim();
            }

            return null;
        }

        public string Send(string message, bool returnResult = true)
        {
            Debug.Write(message);

            Debug.Flush();

            var bytes = new byte[4096];

            var msg = Encoding.Default.GetBytes(message);

            sender.Send(msg);

            return returnResult ? Reveice() : null;
        }
    }
}