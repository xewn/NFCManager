using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NFCManager
{
    public class NfcOperate
    {
        public NfcOperate()
        {
            
        }

        public string ReadNfcId(bool bConnectedDevice)
        {
            string nfcId = string.Empty;
            short icdev = 0x0000;
            int status;
            // byte type = (byte)'A';//mifare one type is A 卡询卡方式为A
            byte mode = 0x26;  // Request the card which is not halted.
            ushort TagType = 0;
            byte bcnt = 0x04;//mifare 卡都用4, hold on 4
            IntPtr pSnr;
            byte len = 255;
            sbyte size = 0;


            if (!bConnectedDevice)
            {
                MessageBox.Show("请连接NFC读写设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return nfcId;
            }

            pSnr = Marshal.AllocHGlobal(1024);

            for (int i = 0; i < 2; i++)
            {
                // status = rf_antenna_sta(icdev, 0);//关闭天线 close antenna  
                // if (status != 0)                
                //     continue;

                //  Sleep(20);
                //  status = rf_init_type(icdev, type);
                //  if (status != 0)
                //      continue;

                //  Sleep(20);
                //  status = rf_antenna_sta(icdev, 1);//启动天线 Open antenna
                //  if (status != 0)
                //      continue;

                //  Sleep(50);     // After open the antenna, it needs about 50ms delay before request.
                status = Er302Helper.rf_request(icdev, mode, ref TagType);//搜寻没有休眠的卡，request card  
                if (status != 0)
                    continue;

                status = Er302Helper.rf_anticoll(icdev, bcnt, pSnr, ref len);//防冲突得到返回卡的序列号, anticol--get the card sn
                if (status != 0)
                    continue;

                status = Er302Helper.rf_select(icdev, pSnr, len, ref size);//锁定一张ISO14443-3 TYPE_A 卡, select one card
                if (status != 0)
                    continue;

                byte[] szBytes = new byte[len];

                for (int j = 0; j < len; j++)
                {
                    szBytes[j] = Marshal.ReadByte(pSnr, j);
                }

                String m_cardNo = String.Empty;

                for (int q = 0; q < len; q++)
                {
                    m_cardNo += byteHEX(szBytes[q]);
                }
                nfcId = m_cardNo;

                break;
            }

            Marshal.FreeHGlobal(pSnr);
            return nfcId;
        }
        #region byteHEX
        /// <summary>
        /// 单个字节转字字符.
        /// </summary>
        /// <param name="ib">字节.</param>
        /// <returns>转换好的字符.</returns>
        public String byteHEX(Byte ib)
        {
            String _str = String.Empty;
            try
            {
                char[] Digit = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A',
                    'B', 'C', 'D', 'E', 'F' };
                char[] ob = new char[2];
                ob[0] = Digit[(ib >> 4) & 0X0F];
                ob[1] = Digit[ib & 0X0F];
                _str = new String(ob);
            }
            catch (Exception)
            {
                new Exception("对不起有错。");
            }
            return _str;

        }
        #endregion

        public string ToHexString(byte[] bytes)
        {
            String hexString = String.Empty;
            for (int i = 0; i < bytes.Length; i++)
                hexString += byteHEX(bytes[i]);

            return hexString;
        }



        public byte[] ToDigitsBytes(string theHex)
        {
            byte[] bytes = new byte[theHex.Length / 2 + (((theHex.Length % 2) > 0) ? 1 : 0)];
            for (int i = 0; i < bytes.Length; i++)
            {
                char lowbits = theHex[i * 2];
                char highbits;

                if ((i * 2 + 1) < theHex.Length)
                    highbits = theHex[i * 2 + 1];
                else
                    highbits = '0';

                int a = (int)GetHexBitsValue((byte)lowbits);
                int b = (int)GetHexBitsValue((byte)highbits);
                bytes[i] = (byte)((a << 4) + b);
            }

            return bytes;
        }
        static char[] hexDigits = {
            '0','1','2','3','4','5','6','7',
            '8','9','A','B','C','D','E','F'};

        public byte GetHexBitsValue(byte ch)
        {
            byte sz = 0;
            if (ch <= '9' && ch >= '0')
                sz = (byte)(ch - 0x30);
            if (ch <= 'F' && ch >= 'A')
                sz = (byte)(ch - 0x37);
            if (ch <= 'f' && ch >= 'a')
                sz = (byte)(ch - 0x57);

            return sz;
        }

    }
}
