using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace NFCManager
{
    public class ConfigData
    {
        private IniData parsedData;
        private FileIniDataParser fileIniData = new FileIniDataParser();
        public ConfigData()
        {
            if (!File.Exists("ConfigIniFile.ini"))
            {
                fileIniData.Parser.Configuration.CommentString = "#";
                IniData initParsedData = InitINIData(new IniData());
                fileIniData.WriteFile("ConfigIniFile.ini", initParsedData, Encoding.UTF8);
                parsedData = initParsedData;
            }
        }
        private int currentPort = 4;
        private int currentBaud = 115200;
        public int CurrentPort{
            get
            {
                if (parsedData!=null)
                {
                    string port = parsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Port").Value;
                    int.TryParse(port, out currentPort);
                }
                return currentPort;
            }
            set
            {
                currentPort = value;
                if (parsedData != null)
                {
                    parsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Port").Value = currentPort.ToString();
                    fileIniData.WriteFile("ConfigIniFile.ini", parsedData, Encoding.UTF8);
                }
            }
        }
        public int CurrentBaud
        {
            get
            {
                if (parsedData != null)
                {
                    string baud = parsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Baud").Value;
                    int.TryParse(baud, out currentBaud);
                }
                return currentBaud;
            }
            set
            {
                currentBaud = value;
                if (parsedData != null)
                {
                    parsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Baud").Value = currentBaud.ToString();
                    fileIniData.WriteFile("ConfigIniFile.ini", parsedData, Encoding.UTF8);
                }
            }
        }
        /// <summary>
        /// 初始化ini文件
        /// </summary>
        /// <param name="initParsedData"></param>
        /// <returns></returns>
        private IniData InitINIData(IniData initParsedData)
        {
            initParsedData["GeneralConfiguration"]["setMaxErrors"] = "10";
            initParsedData.Sections.AddSection("NFCSerialPort");
            initParsedData.Sections.GetSectionData("NFCSerialPort").LeadingComments
                .Add("NFC串口读写设备");
            initParsedData.Sections.GetSectionData("NFCSerialPort").Keys.AddKey("Port", CurrentPort.ToString());
            initParsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Port").Comments
                .Add("默认的串口号");
            initParsedData.Sections.GetSectionData("NFCSerialPort").Keys.AddKey("Baud", CurrentBaud.ToString());
            initParsedData.Sections.GetSectionData("NFCSerialPort").Keys.GetKeyData("Baud").Comments
                .Add("NFC读写设备波特率");

            return initParsedData;
        }
    }
}
