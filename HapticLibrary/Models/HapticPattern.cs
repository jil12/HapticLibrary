using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.Models
{
    public class HapticPattern
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public float Temperature { get; set; }
        public float Vibration { get; set; }
        public HapticPattern(string name, Color color, float temperature, float vibration) {
            Name = name;
            Color = color;
            Temperature = temperature;
            Vibration = vibration;
        }
    }
}
