using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NFCManager
{
    public static class Er302Helper
    {
        [DllImport(@"Lib\kernel32.dll")]
        public static extern void Sleep(int dwMilliseconds);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int lib_ver(ref uint pVer);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_init_com(int port, int baud);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_ClosePort();

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_beep(short icdev, byte delay);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_light(short icdev, byte Ledcolor);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_antenna_sta(short icdev, byte mode);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_init_type(short icdev, byte type);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_request(short icdev, byte mode, ref ushort pTagType);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_anticoll(short icdev, byte bcnt, IntPtr pSnr, ref byte pRLength);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_select(short icdev, IntPtr pSnr, byte srcLen, ref sbyte Size);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_halt(short icdev);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_authentication2(short icdev, byte mode, byte secnr, IntPtr key);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_initval(short icdev, byte adr, Int32 value);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_increment(short icdev, byte adr, Int32 value);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_decrement(short icdev, byte adr, Int32 value);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_readval(short icdev, byte adr, ref Int32 pValue);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_read(short icdev, byte adr, IntPtr pData, ref byte pLen);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_M1_write(short icdev, byte adr, IntPtr pData);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_ul_select(short icdev, IntPtr pSnr, ref byte pRLength);

        [DllImport(@"Lib\MasterRD.dll")]
        public static extern int rf_ul_write(short icdev, byte page, IntPtr pData);
    }
}
