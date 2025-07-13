using Datafeel;
using Datafeel.NET.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.Models
{
    /**
     * Singleton class, stores Datafeel DotManager
     */
    public sealed class HapticManager
    {
        private static HapticManager _instance = new HapticManager();
        private DotManager _dotManager;
        private DatafeelModbusClient _datafeelModbusClient;
        public DotManager DotManager { get { return _dotManager; } }


        private HapticManager() 
        {
            _dotManager = new DotManagerConfiguration()
                .AddDot<Dot_63x_xxx>(1)
                .AddDot<Dot_63x_xxx>(2)
                .AddDot<Dot_63x_xxx>(3)
                .AddDot<Dot_63x_xxx>(4)
                .CreateDotManager();

            _datafeelModbusClient = new DatafeelModbusClientConfiguration()
                .UseWindowsSerialPortTransceiver()
                //.UseSerialPort("COM3") // Uncomment this line to specify the serial port by name
                .CreateClient();
        }

        public static HapticManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HapticManager();
            }
            return _instance;
        }

        public async Task StartManager()
        {
            Console.WriteLine("starting manager");
            await DotManager.Start(_datafeelModbusClient);
            Console.WriteLine("started manager");
        }
    }
}
