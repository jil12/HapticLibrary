using CommunityToolkit.Mvvm.Input;
using Datafeel;
using HapticLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HapticLibrary.ViewModels
{
    public partial class SettingsPageViewModel : ViewModelBase, IPageViewModel
    {

        [RelayCommand]
        public void TempButton()
        {
            ReadingBookJson output = new ReadingBookJson();
            output.Name = "Austin";
            output.Pages = new ReadingPage[5];
            Dictionary<string, HapticEffect> page1 = new Dictionary<string, HapticEffect>();
            page1.Add("first", new HapticEffect {
                Props = [new DotPropsJson {
                    Address = 1,
                    LedMode = LedModes.GlobalManual,
                    GlobalLed = new RgbLed(50, 0, 0)
                }]
            });
            output.Pages[0] = new ReadingPage("This is the first page.", page1);

            SerializeDictionaryToFileAsync(output, Path.Combine("C:/Users/Austin/Projects/Datafeel-Storytelling/HapticLibrary/HapticLibrary/Assets", "output.json"));
        }

        public async Task SerializeDictionaryToFileAsync(ReadingBookJson output, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Makes the output pretty-printed
            };

            using FileStream createStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(createStream, output, options);
        }
    }
}
