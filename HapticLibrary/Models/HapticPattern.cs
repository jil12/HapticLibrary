using Datafeel;
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

        public HapticPattern(DotPropsJson propsJson)
        {
            Name = "";
            Color = (propsJson.GlobalLed != null) ? Color.FromArgb(propsJson.GlobalLed.Red, propsJson.GlobalLed.Green, propsJson.GlobalLed.Blue) : Color.Transparent;
            Temperature = (propsJson.ThermalIntensity != null) ? (float) propsJson.ThermalIntensity! : 0.0f;
            Vibration = (propsJson.VibrationIntensity !=null) ? (float)propsJson.VibrationIntensity! : 0.0f;
        } 

        public DotPropsJson ConvertToJson()
        {
            DotPropsJson output = new DotPropsJson();
            output.Address = 15;    //TODO: User should be able to enter address.
            output.LedMode = LedModes.GlobalManual;
            output.GlobalLed = new RgbLed();
            output.GlobalLed.Red = Color.R; //NOTE: RGBLed class has broken constructor.
            output.GlobalLed.Green = Color.G;
            output.GlobalLed.Blue = Color.B;
            output.ThermalMode = ThermalModes.Manual;
            output.ThermalIntensity = Temperature;
            output.VibrationMode = VibrationModes.Manual;
            output.VibrationIntensity = Vibration;
            return output;
        }
    }
}
