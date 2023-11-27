using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace POSSUM.Api.Utility
{
    public class Dibs
    {
        /**
        * calculateMac
        * Calculates the MAC key from a Dictionary<string, string> and a secret key
        * @param params_dict The Dictionary<string, string> object containing all keys and their values for MAC calculation
        * @param K_hexEnc String containing the hex encoded secret key from DIBS Admin
        * @return String containig the hex encoded MAC key calculated
        **/
        public static string CalculateMac(Dictionary<string, string> params_dict, string K_hexEnc)
        {
            //Create the message for MAC calculation sorted by the key
            var keys = params_dict.Keys.ToList();
            keys.Sort();
            string msg = "";
            foreach (var key in keys)
            {
                if (key != keys[0]) msg += "&";
                msg += key + "=" + params_dict[key];
            }

            //Decoding the secret Hex encoded key and getting the bytes for MAC calculation
            var K_bytes = new byte[K_hexEnc.Length / 2];
            for (int i = 0; i < K_bytes.Length; i++)
            {
                K_bytes[i] = byte.Parse(K_hexEnc.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            //Getting bytes from message
            var encoding = new UTF8Encoding();
            byte[] msg_bytes = encoding.GetBytes(msg);

            //Calculate MAC key
            var hash = new HMACSHA256(K_bytes);
            byte[] mac_bytes = hash.ComputeHash(msg_bytes);
            string mac = BitConverter.ToString(mac_bytes).Replace("-", "").ToLower();

            return mac;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }


    public enum PaymentStatus : int
    {
        PartnerError = -2
        , InternalError = -1
        , Success = 00
        , PaymentError = 1
        , AccountNotFound = 2
        , InsufficientFunds = 3
        , Pending = 4
        , Declined = 5
        , RollbackError = 6
        , CreditError = 7
        , ACCEPTED = 0
    }
    public enum PaymentTypeHistory : int
    {
        Code = 1
            , Dibs = 2
            , WyWallet = 3
            , Cash = 3
    }
}