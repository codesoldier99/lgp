using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Modbus.Device;

namespace IndustryDemo.Controllerui
{
    public class STM32Control
    {
        private IModbusSerialMaster master;

        //private bool M1Control, M1ControlScd;
        //private bool M2Control;
        //private bool M3Control;
        //private bool M5Control, M5ControlScd;
        bool beginLocation;

        SerialPort serialPort1;
        int baud;
        string portName1;

        public STM32Control(SerialPort serialPort, int baudRate, string portName)
        {
            serialPort1 = serialPort;
            baud = baudRate;
            portName1 = portName;
        }
        public void Connect()
        {
            try
            {
                serialPort1.PortName = portName1;
                serialPort1.BaudRate = baud;
                serialPort1.Parity = Parity.None;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Open();
                master = ModbusSerialMaster.CreateRtu(serialPort1);
                master.Transport.ReadTimeout = 2000;
            }
            catch
            {
                MessageBox.Show("建立连接失败！");
                return;
            }
        }

        public void DisConnect()
        {
            serialPort1.Close();
        }

        public bool GetValue(ushort addr)
        {
            ushort inputregesters;
            inputregesters = master.ReadHoldingRegisters(1, addr, 1)[0];
            if (inputregesters == 99)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void M1fstBasement_Click()
        {
            ushort[] vs = new ushort[1] { 7 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void M1scdBasement_Click()
        {
            M1Reset_Click();
            ushort[] vs = new ushort[1] { 9 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void fwd6_Click()
        {
            ushort[] vs = new ushort[1] { 2 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void bwd6_Click()
        {
            ushort[] vs = new ushort[1] { 1 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void fwd1_Click()
        {
            ushort[] vs = new ushort[1] { 6 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void bwd1_Click()
        {
            ushort[] vs = new ushort[1] { 5 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void fwd34_Click()
        {
            ushort[] vs = new ushort[1] { 4 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void bwd34_Click()
        {
            ushort[] vs = new ushort[1] { 3 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }

        public void M1Reset_Click()
        {
            ushort[] vs = new ushort[1] { 0 };
            master.WriteMultipleRegisters(1, 40004, vs);
            while (GetValue(40004)) ;
        }


        public void lft6_Click()
        {
            ushort[] vs = new ushort[1] { 1 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void rgt6_Click()
        {
            ushort[] vs = new ushort[1] { 2 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void M2LeftReset_Click()
        {
            ushort[] vs = new ushort[1] { 3 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void M2RightReset_Click()
        {
            ushort[] vs = new ushort[1] { 0 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void M3Basement_Click()
        {
            M3Reset_Click();
            ushort[] vs = new ushort[1] { 5 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void up0m_Click()
        {
            ushort[] vs = new ushort[1] { 3 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void down0m_Click()
        {
            ushort[] vs = new ushort[1] { 1 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void up1m_Click()
        {
            ushort[] vs = new ushort[1] { 4 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void down1m_Click()
        {
            ushort[] vs = new ushort[1] { 2 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void M3Reset_Click()
        {
            ushort[] vs = new ushort[1] { 0 };
            master.WriteMultipleRegisters(1, 40003, vs);
            while (GetValue(40003)) ;
        }

        public void M5ScdBasement_Click()
        {
            M5Reset_Click();
            ushort[] vs = new ushort[1] { 10 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M5Right1m_Click()
        {
            ushort[] vs = new ushort[1] { 1 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M5Left1m_Click()
        {
            ushort[] vs = new ushort[1] { 2 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M5Right32m_Click()
        {
            ushort[] vs = new ushort[1] { 6 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M5Left32m_Click()
        {
            ushort[] vs = new ushort[1] { 5 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M6Up1m_Click()
        {
            ushort[] vs = new ushort[1] { 4 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M6Down1m_Click()
        {
            ushort[] vs = new ushort[1] { 3 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M7fwd_Click()
        {
            ushort[] vs = new ushort[1] { 8 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M7bwd_Click()
        {
            ushort[] vs = new ushort[1] { 7 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void M5Reset_Click()
        {
            ushort[] vs = new ushort[1] { 0 };
            master.WriteMultipleRegisters(1, 40007, vs);
            while (GetValue(40007)) ;
        }

        public void lgtReset_Click()
        {
            ushort[] vs = new ushort[1] { 0 };
            master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
        }


        
        public int trigger_Click()  //等待环光触发
        {
            //ushort[] vs = new ushort[1] { 1 };
            //master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
            return GetDistance();
        }
        public int trigger2_Click()  //等待点光触发
        {
            //ushort[] vs = new ushort[1] { 6 };
            //master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
            return GetDistance();
        }

        public void Std_btn_Click()
        {
            ushort[] vs = new ushort[1] { 1 };
            master.WriteMultipleRegisters(1, 40001, vs);
            while (GetValue(40001)) ;
        }

        public void lft32_Click()
        {
            ushort[] vs = new ushort[1] { 4 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void rgt32_Click()
        {
            ushort[] vs = new ushort[1] { 5 };
            master.WriteMultipleRegisters(1, 40002, vs);
            while (GetValue(40002)) ;
        }

        public void btnBegin_Click()
        {
            M3Basement_Click();
            M1fstBasement_Click();
            M2RightReset_Click();
            beginLocation = true;
        }

        public void btnEnd_Click()
        {
            if (!beginLocation)
            {
                btnBegin_Click();
            }
            beginLocation = false;
            for (int i = 0; i < 10; i++)
            {
                lft32_Click();
            }
                
        }

        public void btnGrabbing_Click()
        {
            ushort[] vs = new ushort[1] { 3 };
            master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
        }


        public void btnRelease_Click()
        {
            ushort[] vs = new ushort[1] { 2 };
            master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
        }

        public void OnlyWritepushRegHoldingBuf(ushort Address, ushort value)
        {
            ushort[] vs = new ushort[1] { value };
            master.WriteMultipleRegisters(1, Address, vs);
        }
        public void WritepushRegHoldingBuf(ushort Address, ushort value)
        {
            ushort[] vs = new ushort[1] { value };
            master.WriteMultipleRegisters(1,Address,vs);
            while (GetValue(Address));
        }

        public int GetpushRegHoldingBuf(ushort Address)
        {
            return master.ReadHoldingRegisters(1, Address, 1)[0];
        }

        public int GetDistance()
        {
            int result;
            result = master.ReadHoldingRegisters(1, 40005, 1)[0];
            int result2;
            result2 = master.ReadHoldingRegisters(1, 40009, 1)[0];
            int final = result * 256 + result2;
            return final;
        }

        public void motor_run(ushort id,double distance)
        {
            //将毫米转为对应电机脉冲数
            uint pulse = 0;
            switch (id)
            {
                case 1:

                    pulse = Convert.ToUInt32(distance / 0.033) / 1;
                    break;
                case 2:
                    pulse = Convert.ToUInt32(distance / 0.01 ) ;
                    break;
                case 34:
                    pulse = Convert.ToUInt32(distance / 0.0025) / 1;
                    break;
                case 5:
                    pulse = Convert.ToUInt32(distance / 0.025) / 1;
                    break;
                case 6:
                    pulse = Convert.ToUInt32(distance / 0.01) / 1;
                    break;
                case 7:

                    pulse = Convert.ToUInt32(distance / 0.07) / 1;
                    break;
                default:
                    break;
            }
            //写距离，单位毫米
            ushort pulse1 = (ushort)(pulse / 256);
            ushort[] vs1 = new ushort[1] { pulse1 };
            master.WriteMultipleRegisters(1, 40010, vs1);

            ushort pulse2 = (ushort)(pulse % 256);
            ushort[] vs2 = new ushort[1] { pulse2 };
            master.WriteMultipleRegisters(1, 40017, vs2);

            
            //电机运动
            switch (id)
            {
                case 1:
                    ushort[] vs3 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40004, vs3);
                    while (GetValue(40004)) ;
                    break;
                case 2:
                    ushort[] vs4 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40002, vs4);
                    while (GetValue(40002)) ;
                    break;
                case 34:
                    ushort[] vs5 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40003, vs5);
                    while (GetValue(40003)) ;
                    break;
                case 5:
                    ushort[] vs6 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40011, vs6);
                    while (GetValue(40011)) ;
                    break;
                case 6:
                    ushort[] vs7 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40012, vs7);
                    while (GetValue(40012)) ;
                    break;
                case 7:
                    ushort[] vs8 = new ushort[1] { 20 };
                    master.WriteMultipleRegisters(1, 40013, vs8);
                    while (GetValue(40013)) ;
                    break;
                default:
                    break;
            }
        }

        public void getRealTimePosition()
        {
            ushort[] vs = new ushort[1] { 7 };
            master.WriteMultipleRegisters(1, 40006, vs);
            while (GetValue(40006)) ;
        }
    }
}
