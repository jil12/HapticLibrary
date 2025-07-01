using Datafeel;
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
        private static HapticManager _instance;
        private DotManager _dotManager;
        public DotManager DotManager { get { return _dotManager; } }


        private HapticManager() 
        {
            _dotManager = new DotManagerConfiguration().CreateDotManager();

            _dotManager.AddDot(new ManagedDot(_dotManager, 0));
            _dotManager.AddDot(new ManagedDot(_dotManager, 1));
            _dotManager.AddDot(new ManagedDot(_dotManager, 2));
            _dotManager.AddDot(new ManagedDot(_dotManager, 3));
            _dotManager.AddDot(new ManagedDot(_dotManager, 4));

            _dotManager.Start();
        }

        public static HapticManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HapticManager();
            }
            return _instance;
        }
    }
}
