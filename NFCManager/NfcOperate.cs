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


        // This command will read out the text string by NDEF formatted. 
        // More detail please reference the NTAG213 datasheet or else NDEF files.
        // Read the UID of the NTAG CHIP befor this command!
        public string nfcDataRead(bool bConnectedDevice)
        {
            string result = string.Empty;
            short icdev = 0x0000;
            int status;
            byte npage = 0x00;
            byte txtCode = 0x54;
            int nPageCount = 0;

            if (!bConnectedDevice)
            {
                MessageBox.Show("请连接NFC读写器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            npage = Convert.ToByte(6);// The special code and length data saved in page 6

            IntPtr dataBuffer = Marshal.AllocHGlobal(256);

            int j;
            byte cLen = 0;
            status = Er302Helper.rf_M1_read(icdev, (byte)(npage), dataBuffer, ref cLen); // This command will read out 4 pages=4*4=16(bytes).

            if (status != 0 || cLen != 16)
            {
                MessageBox.Show("rf_M1_read failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Marshal.FreeHGlobal(dataBuffer);
                return "";
            }

            byte[] bytesData = new byte[4];      // 4 bytes for one page
            for (j = 0; j < bytesData.Length; j++)
                bytesData[j] = Marshal.ReadByte(dataBuffer, j);
            if (bytesData[2] != txtCode)
            {
                MessageBox.Show("Ilegal NFC Text!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Marshal.FreeHGlobal(dataBuffer);
                return "";
            }

            byte nLen = 0;
            nLen = bytesData[1]; // Length of the text file including 2 preamble bytes and 1 postamble byte(0xfe)

            nPageCount = nLen / 4;
            j = nLen % 4;
            if (j > 0)
                nPageCount = nPageCount + 1;


            byte[] bytesReceiveData = new byte[256];
            int i;
            for (j = 7; j < (7 + nPageCount); j++)  // read from page 7
            {
                status = Er302Helper.rf_M1_read(icdev, (byte)(j), dataBuffer, ref cLen);
                if (status != 0 || cLen != 16)
                {
                    MessageBox.Show("Read page failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Marshal.FreeHGlobal(dataBuffer);
                    return "";
                }

                for (i = 0; i < 4; i++)
                    bytesReceiveData[4 * (j - 7) + i] = Marshal.ReadByte(dataBuffer, i);
            }

            byte[] bytesAsciiData = new byte[nLen - 3];
            for (i = 2; i < nLen - 1; i++)
                bytesAsciiData[i - 2] = Marshal.ReadByte(bytesReceiveData, i); // get the true text ascii code
            result = System.Text.Encoding.ASCII.GetString(bytesAsciiData).Trim(); // Show the text string saved in the nfc chip.

            Marshal.FreeHGlobal(dataBuffer);
            int delaytime = 50;
            Er302Helper.rf_beep(icdev, (byte)delaytime);
            return result;
        }

        // This command will convert the input text string to Ascii hex array then write into the NTAG chip page by page.
        public void nfcDataWrite(bool bConnectedDevice,string context)
        {
            NfcOperate nfcOperate = new NfcOperate();
            short icdev = 0x0000;
            int status;
            byte npage;
            int i;

            if (!bConnectedDevice)
            {
                MessageBox.Show("请连接NFC读写器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int Len;
            Len = context.Trim().Length;
            Len = Len + 3;  // add Encode bytes(2) and tailer code byte(1)

            IntPtr dataBuffer = Marshal.AllocHGlobal(1024);
            npage = (byte)(Convert.ToByte(4));
            byte[] page4_buf = new byte[] { 0x01, 0x03, 0xa0, 0x10 }; ////Page4_buf[4]={0x01,0x03,0xa0,0x10};//0c
            for (i = 0; i < page4_buf.Length; i++)
                Marshal.WriteByte(dataBuffer, i, page4_buf[i]);
            status = Er302Helper.rf_ul_write(icdev, npage, dataBuffer);

            if (status != 0)
            {
                MessageBox.Show("Write page 4 failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            npage = (byte)(Convert.ToByte(5));
            byte[] page5_buf = new byte[] { 0x44, 0x03, (byte)(Len + 4), 0xd1 };
            for (i = 0; i < page5_buf.Length; i++)
                Marshal.WriteByte(dataBuffer, i, page5_buf[i]);
            status = Er302Helper.rf_ul_write(icdev, npage, dataBuffer);

            if (status != 0)
            {
                MessageBox.Show("Write page 5 failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            npage = (byte)(Convert.ToByte(6));
            byte[] page6_buf = new byte[] { 0x01, (byte)Len, 0x54, 0x02 };
            for (i = 0; i < page6_buf.Length; i++)
                Marshal.WriteByte(dataBuffer, i, page6_buf[i]);
            status = Er302Helper.rf_ul_write(icdev, npage, dataBuffer);

            if (status != 0)
            {
                MessageBox.Show("Write page 6 failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int tPage = 0;   // Total Pages
            tPage = Len / 4;
            int j = Len % 4;
            int k = 0;  // fill zero counter
            if (j > 0)
            {
                tPage = tPage + 1;
                k = 4 - j;
            }
            string sZero = "00";
            string sFillTail = string.Empty;
            if (k > 0)
            {
                for (i = 0; i < k; i++)
                    sFillTail += sZero;
            }

            byte[] bEncode = new byte[] { 0x65, 0x6e };
            string sEncode = string.Empty;
            for (i = 0; i < bEncode.Length; i++)
            {
                sEncode += Convert.ToString(bEncode[i], 16);
            }

            byte[] bTrailer = new byte[] { 0xfe };
            string sTailer = string.Empty;
            for (i = 0; i < bTrailer.Length; i++)
            {
                sTailer += Convert.ToString(bTrailer[i], 16);
            }

            byte[] bytesBlock;
            bytesBlock = Encoding.ASCII.GetBytes(context.Trim());// Convert the string input to ascii code bytes.
            string sInput = string.Empty;
            for (i = 0; i < bytesBlock.Length; i++)
            {
                sInput += Convert.ToString(bytesBlock[i], 16);
            }

            string sComb = string.Empty;
            sComb += sEncode;
            sComb += sInput;
            sComb += sTailer;
            sComb += sFillTail;
            bytesBlock = nfcOperate.ToDigitsBytes(sComb);    // convert to hex byte string  

            int ofs;
            for (j = 7; j < tPage + 7; j++)
            {
                ofs = (j - 7) * 4;
                for (i = 0; i < 4; i++)
                { Marshal.WriteByte(dataBuffer, i, bytesBlock[i + ofs]); } // copy to pointer buffer               
                status = Er302Helper.rf_ul_write(icdev, (byte)j, dataBuffer);
                if (status != 0)
                {
                    MessageBox.Show("NFC写入失败！", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Marshal.FreeHGlobal(dataBuffer);
            int color = 2;// 2:red led light ,1: blue light,0: led off
            Er302Helper.rf_light(icdev, (byte)color);
            int delaytime = 50;
            Er302Helper.rf_beep(icdev, (byte)delaytime);
            color = 0;
            Er302Helper.rf_light(icdev, (byte)color);
            color = 1;
            Er302Helper.rf_light(icdev, (byte)color);
            MessageBox.Show("NFC写入成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        /// <summary>
        /// 将数据写入NFC卡，从0区0块开始写入，直到写完。
        /// </summary>
        /// <param name="mass">区</param>
        /// <param name="submass">块1,2,3</param>
        /// <param name="context">将要写入NFC卡的数据，小于32个字符</param>
        public void WriteBlock(bool bConnectedDevice, int mass, int submass, string context)
        {
            NfcOperate nfcOperate = new NfcOperate();
            short icdev = 0x0000;
            int status;
            byte mode = 0x60;//mode = 0x61; //密钥类型
            string mima = "FFFFFFFFFFFF";
            byte secnr = 0x00;
            byte adr;
            int i;
            if (!bConnectedDevice)
            {
                MessageBox.Show("请连接NFC读写器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            secnr = Convert.ToByte(mass);//cbxMass2这个是区
            adr = (byte)(Convert.ToByte(submass) + secnr * 4);//这个是块cbxSubmass2

            if (submass == 4)
            {
                if (DialogResult.Cancel == MessageBox.Show("不允许写每个区的最后一块", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                    return;
            }

            IntPtr keyBuffer = Marshal.AllocHGlobal(1024);
            //txtInputKey2.Text是每个扇区的密码FFFFFFFFFFFF
            byte[] bytesKey = nfcOperate.ToDigitsBytes(mima);
            for (i = 0; i < bytesKey.Length; i++)
                Marshal.WriteByte(keyBuffer, i, bytesKey[i]);
            status = Er302Helper.rf_M1_authentication2(icdev, mode, (byte)(secnr * 4), keyBuffer);
            Marshal.FreeHGlobal(keyBuffer);
            if (status != 0)
            {
                MessageBox.Show("rf_M1_authentication2 failed!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //
            byte[] bytesBlock;
            if (submass == 1)
            {
                bytesBlock = nfcOperate.ToDigitsBytes(context);
            }
            else if (submass == 2)
            {
                bytesBlock = nfcOperate.ToDigitsBytes(context);
            }
            else if (submass == 3)
            {
                bytesBlock = nfcOperate.ToDigitsBytes(context);
            }
            else
            {
                String strCompont = context;
                bytesBlock = nfcOperate.ToDigitsBytes(strCompont);
            }

            IntPtr dataBuffer = Marshal.AllocHGlobal(1024);

            for (i = 0; i < bytesBlock.Length; i++)
                Marshal.WriteByte(dataBuffer, i, bytesBlock[i]);
            status = Er302Helper.rf_M1_write(icdev, adr, dataBuffer);
            Marshal.FreeHGlobal(dataBuffer);

            if (status != 0)
            {
                MessageBox.Show("NFC写入数据出错！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
