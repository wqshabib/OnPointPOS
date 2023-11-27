// Ccsp.cs
// A simple class for creating and parsing CCSP messages
//
// This code is a simple example of how to process CCSP messages
// NOTE: This is not a reference implementation, there may exist
// a need for extra error handling.
// 
// Copyright (c) 2009 Retail Innovation HTT AB
//
// This code may be used freely. However, if modifications are made
// the code must not be distributed so that recipients get the
// impression that the modified code comes from Retail Innovation HTT AB.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace POSSUM.Integration.ControlUnits.CleanCash
{
    class CcspException : System.ApplicationException
    {
        public CcspException()
        {
        }

        public CcspException(string message)
            : base(message)
        {
        }

        public CcspException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    class CcspNakException : CcspException
    {
        public enum NakError
        {
            Ok = 0,
            InvalidLrc = 1,
            UnknownMessageType = 2,
            InvalidDataOrParameter = 3,
            InvalidSequence = 4,
            NotOperational = 6,
            InvalidPosId = 7,
            InternalError = 8
        }

        private NakError _error;

        public NakError ErrorCode
        {
            get
            {
                return _error;
            }
        }

        public CcspNakException()
        {
        }

        public CcspNakException(NakError error)
            : this(error.ToString())
        {
            _error = error;
        }

        public CcspNakException(string message)
            : base(message)
        {
        }

        public CcspNakException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    class CcspClient
    {
        public enum Mode
        {
            Unknown
            ,
            A
                , B
        }

        public enum IncomingMessageType
        {
            Unknown
            ,
            Ack
                ,
            Nak
                ,
            IdentityResponse
                ,
            SignatureResponse
                , StatusInformationResponse
        }

        public enum OutgoingMessageType
        {
            Unknown
            ,
            IdentityRequest
                ,
            PaymentInformation
                ,
            ReceiptItem
                ,
            ReceiptHeader
                ,
            ReceiptExtraSummary
                ,
            SignatureRequest
                ,
            StartReceipt
                , StatusInformationRequest
        }

        private static readonly Dictionary<string, IncomingMessageType> IN_MESSAGE_TYPES =
            new Dictionary<string, IncomingMessageType> 
            { 
                { "ACK", IncomingMessageType.Ack }
                ,{ "NAK", IncomingMessageType.Nak }
                ,{ "IR", IncomingMessageType.IdentityResponse }
                ,{ "SR", IncomingMessageType.SignatureResponse }
                ,{ "SIR", IncomingMessageType.StatusInformationResponse}
            };

        private static readonly Dictionary<OutgoingMessageType, string> OUT_MESSAGE_TYPES =
            new Dictionary<OutgoingMessageType, string> 
            {
                { OutgoingMessageType.IdentityRequest, "IQ"}
                ,{ OutgoingMessageType.PaymentInformation, "PI" }
                ,{ OutgoingMessageType.ReceiptHeader, "RH" }
                ,{ OutgoingMessageType.ReceiptItem, "RI" }
                ,{ OutgoingMessageType.ReceiptExtraSummary, "RX" }
                ,{ OutgoingMessageType.SignatureRequest, "SQ" }
                ,{ OutgoingMessageType.StartReceipt, "ST"}
                ,{ OutgoingMessageType.StatusInformationRequest, "SIQ" }
            };

        private Mode _messageMode = Mode.Unknown;
        private IncomingMessageType _messageType = IncomingMessageType.Unknown;
        private String _fieldString = "";
        private List<string> _fields = new List<string>();

        public Mode MessageMode
        {
            get { return _messageMode; }
        }

        public IncomingMessageType MessageType
        {
            get { return _messageType; }
        }

        public string FieldString
        {
            get { return _fieldString; }
        }

        public List<string> Fields
        {
            get { return _fields; }
        }

        public CcspClient()
        {
        }

        public byte CalculateLrc(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            byte lrc = 0;

            foreach (char c in message)
            {
                lrc ^= (byte)(c & 0xff);
            }
            return lrc;
        } // CalculateLrc()

        public void ComposeWorker(string typeString, string data,
            StringBuilder msg)
        {
            Trace.Assert(typeString != null);
            // (#[!%]#LEN# = 7) [+ MT + (#DATA)] + (# + LRC + CR = 4) = 11
            int len = 11 + typeString.Length;
            if (data.Length > 0)
            {
                len += data.Length + 1; // Extra '#'
            }

            msg.Append(len.ToString("X3"));
            msg.Append('#');
            msg.Append(typeString);

            int dataPos = msg.Length + 1;

            if (data.Length > 0)
            {
                msg.Append('#');
                msg.Append(data);
            }

            msg.Append('#');

            byte lrc = CalculateLrc(msg.ToString());

            msg.Append(lrc.ToString("X2"));
            msg.Append('\r');
        }

        public String Compose(Mode mode, IncomingMessageType type, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            StringBuilder sb = new StringBuilder();

            sb.Append('#');
            sb.Append(mode == Mode.B ? '%' : '!');
            sb.Append('#');

            string typeString = null;

            foreach (KeyValuePair<string, IncomingMessageType> pair in IN_MESSAGE_TYPES)
            {
                if (pair.Value == type)
                {
                    typeString = pair.Key;
                    break;
                }
            }
            if (typeString == null)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            ComposeWorker(typeString, data, sb);

            return sb.ToString();
        }


        public string Compose(Mode mode, OutgoingMessageType type, string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }

            StringBuilder sb = new StringBuilder();

            sb.Append('#');
            sb.Append(mode == Mode.B ? '%' : '!');
            sb.Append('#');

            string typeString;
            OUT_MESSAGE_TYPES.TryGetValue(type, out typeString);

            ComposeWorker(typeString, data, sb);

            return sb.ToString();
        } // Compose()

        public string Compose(Mode mode, OutgoingMessageType type)
        {
            return Compose(mode, type, "");
        }

        /// <summary>
        /// Parses a CCSP message
        /// </summary>
        /// <param name="message">A CCSP message starting with #!# or #%#</param>
        /// 
        public void Parse(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            Regex messageRe = new Regex(
@"(?x)
\#(?<Identifier>[\!\%])\#
(?<Length>[0-9A-F]{3})\#
(?<MessageType>[^#]{1,3})\#
(?:(?<Fields>[^#].*)\#)?
(?<Lrc>[0-9A-F]{2})\r$");

            Match match = messageRe.Match(message);

            if (!match.Success)
            {
                throw new CcspNakException(CcspNakException.NakError.InvalidDataOrParameter);
            }

            try
            {
                uint len = uint.Parse(match.Groups["Length"].Value,
                                      System.Globalization.NumberStyles.HexNumber);
                byte lrc = byte.Parse(match.Groups["Lrc"].Value,
                                      System.Globalization.NumberStyles.HexNumber);
                if (len != message.Length)
                {
                    throw new CcspNakException(CcspNakException.NakError.InvalidDataOrParameter);
                }

                if (lrc != CalculateLrc(message.Substring(0, message.Length - 3)))
                {
                    throw new CcspNakException(CcspNakException.NakError.InvalidLrc);
                }
            }
            catch (FormatException)
            {
                throw new CcspNakException(CcspNakException.NakError.InvalidDataOrParameter);
            }

            IncomingMessageType mt;
            IN_MESSAGE_TYPES.TryGetValue(match.Groups["MessageType"].Value, out mt);

            if (mt == IncomingMessageType.Unknown)
            {
                throw new CcspNakException(CcspNakException.NakError.UnknownMessageType);
            }

            Mode mode = (match.Groups["Identifier"].Value == "!") ? Mode.A : Mode.B;
            String fieldString = match.Groups["Fields"].Value + "#";

            Regex fieldRe = new Regex(
@"(?x)
([^#](?:\#\#|[^#])*)
");
            MatchCollection matches = fieldRe.Matches(fieldString);

            Trace.Assert(matches.Count >= 0);

            _fields = new List<string>();
            foreach (Match m in matches)
            {
                _fields.Add(m.Value);
            }

            _messageMode = mode;
            _messageType = mt;
            _fieldString = fieldString;
        } // Parse()
    }
}
