using Avalonia.Diagnostics;
using Datafeel;
using HapticLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HapticLibrary.Services
{
    /// <summary>
    /// Contains presets that can be used in audio or read aloud
    /// that can be passed into Play with a string of the type.
    /// </summary>
    public class HapticPresets
    {
        private static ManagedDot ChestDot = HapticManager.GetInstance().DotManager.FindDot(1);
        private static ManagedDot WristDot = HapticManager.GetInstance().DotManager.FindDot(2);
        public static void Play(string Type)
        {
            
            switch (Type)
            {
                case "Event1":
                    Event1();
                    break;
                case "Event2":
                    Event2();
                    break;
                case "Event3":
                    Event3();
                    break;
                case "Event4":
                    Event4();
                    break;
                case "Event5":
                    Event5();
                    break;
                case "Event6":
                    Event6();
                    break;
                default:
                    Console.WriteLine("Unkown haptic type");
                    break;
            }
        }

        public static async Task ResetAsync(ManagedDot Dot)
        {
            Dot.ThermalIntensity = 0;
            Dot.LedMode = LedModes.GlobalManual;
            Dot.GlobalLed.Red = 0;
            Dot.GlobalLed.Green = 0;
            Dot.GlobalLed.Blue = 0;
            Dot.VibrationFrequency = 0;
            Dot.VibrationIntensity = 0;
            Dot.VibrationGo = false;
            try
            {
                await Dot.Write();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Reset Dot.");
        }

        /*
         * "Police suggest entire population in the Elm Terrace area do as follows:
         * Everyone in every house in every street open a front or rear door or look from the windows. 
         * The fugitive cannot escape if everyone in the next minute looks from his house. Ready! "
         * Time: 0:00 - 0:16
         */
        private static async Task Event1()
        {
            Console.WriteLine("Event 1");
            Task Delay = Task.Delay(16000);

            Unease(ChestDot);
            AmbulanceSiren(WristDot);

            await Delay;
            ResetAsync(ChestDot);
            ResetAsync(WristDot);
        }

        /// <summary>
        /// Creates unease
        /// </summary>
        /// <param name="ChestDot">Uses ChestDot Vibrations</param>
        /// <returns></returns>
        private static async Task Unease(ManagedDot ChestDot)
        {
            ChestDot.VibrationMode = VibrationModes.Manual;
            ChestDot.VibrationFrequency = 41.2f;
            ChestDot.VibrationIntensity = 1.0f;
            ChestDot.VibrationGo = true;

            try
            {
                await ChestDot.Write();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Alternates wrist from Red to White 30 times.
        /// </summary>
        /// <param name="WristDot">Uses Wrist Dot Lights</param>
        /// <returns></returns>
        private static async Task AmbulanceSiren(ManagedDot WristDot)
        {
            WristDot.LedMode = LedModes.GlobalManual;

            for (int i = 0; i < 30; i++)
            {
                Task Delay = Task.Delay(250);
                WristDot.GlobalLed.Red = 255;
                WristDot.GlobalLed.Blue = 0;
                WristDot.GlobalLed.Green = 0;
                try
                {
                    await WristDot.Write();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Delay;

                Delay = Task.Delay(250);
                WristDot.GlobalLed.Red = 255;
                WristDot.GlobalLed.Green = 255;
                WristDot.GlobalLed.Blue = 255;
                try
                {
                    await WristDot.Write();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                await Delay;
            }
        }


        /*
         * Of course! Why hadn't they done it before! Why, in all the years, hadn't this game been tried! 
         * Everyone up, everyone out! He couldn't be missed! The only man running alone in the night city, 
         * the only man proving his legs! "At the count of ten now! One! Two!
         * Time: 0:16 - 0:32
         */
        private static async Task Event2()
        {
            Task Delay = Task.Delay(15000);
            HeartBeat(ChestDot);
            Hot(WristDot);
            await Delay;
            ResetAsync(ChestDot);
            ResetAsync(WristDot);
        }

        /// <summary>
        /// Creates 78 HeartBeats
        /// </summary>
        /// <param name="ChestDot">Uses ChestDot Vibrationa and Light</param>
        /// <returns></returns>
        private static async Task HeartBeat(ManagedDot ChestDot)
        {
            ChestDot.VibrationGo = true;

            int bpm = 150;
            double beatRate = 60.0 / bpm;
            int singleBeatDelay = 100;
            double betweenBeatOffset = beatRate - .1;

            ChestDot.VibrationMode = VibrationModes.Library;
            ChestDot.VibrationSequence[0].Waveforms = VibrationWaveforms.SoftBumpP100;
            ChestDot.VibrationSequence[1].RestDuration = singleBeatDelay; // Milliseconds
            ChestDot.VibrationSequence[2].Waveforms = VibrationWaveforms.SoftBumpP30;
            ChestDot.VibrationSequence[3].RestDuration = (int)(betweenBeatOffset * 1000); // Milliseconds
            ChestDot.VibrationSequence[4].Waveforms = VibrationWaveforms.EndSequence;

            ChestDot.LedMode = LedModes.GlobalManual;

            for (int i = 0; i < 78; i++)
            {
                Console.WriteLine("Heart Beat!");
                ChestDot.VibrationGo = false;
                for (byte brightness = 241; brightness > 20; brightness -= 20)
                {
                    ChestDot.GlobalLed.Red = brightness;
                    await ChestDot.Write();
                }
                ChestDot.VibrationGo = true;
                await ChestDot.Write();
                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Heats up Wrist for 15 seconds.
        /// </summary>
        /// <param name="WristDot">Uses Wrist Thermals.</param>
        /// <returns></returns>
        private static async Task Hot(ManagedDot WristDot)
        {
            WristDot.ThermalMode = ThermalModes.Manual;
            WristDot.ThermalIntensity = .5f;

            try
            {
                await WristDot.Write();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /*
         * Counting all the numbers.
         * One - 0:30
         * Two - 0:32
         * Three - 0:35
         * Four - 0:40
         * Five - 0:44
         * Six - 0:55
         * Seven - 0:57
         * Eight - 0:59
         * Nine - 1:03
         * Ten - 1:08
         * 
         */
        private static async Task Event3()
        {
            // red flashes and "random" buzzes across different dots

            Console.WriteLine("One!");
            Task Delay = Task.Delay(1700);
            Number();
            await Delay;

            Console.WriteLine("Two!");
            Delay = Task.Delay(2700);
            Number();
            await Delay;

            Console.WriteLine("Three!");
            Delay = Task.Delay(5700);
            Number();
            await Delay;

            Console.WriteLine("Four!");
            Delay = Task.Delay(3000);
            Number();
            await Delay;

            Console.WriteLine("Five!");
            Delay = Task.Delay(11800);
            Number();
            await Delay;

            Console.WriteLine("Six!");
            Delay = Task.Delay(1500);
            Number();
            await Delay;

            Console.WriteLine("Seven!");
            Delay = Task.Delay(1400);
            Number();
            await Delay;

            Console.WriteLine("Eight!");
            Delay = Task.Delay(3800);
            Number();
            await Delay;

            Console.WriteLine("Nine!");
            Delay = Task.Delay(6000);
            Number();
            await Delay;

            Console.WriteLine("Ten!");
            foreach (ManagedDot d in HapticManager.GetInstance().DotManager.Dots)
            {

                // Sets one LED Red
                d.LedMode = LedModes.GlobalManual;
                d.GlobalLed.Red = 255;
                d.VibrationMode = VibrationModes.Library;
                d.VibrationSequence[0].Waveforms = VibrationWaveforms.StrongClick1P100;
                d.VibrationSequence[1].Waveforms = VibrationWaveforms.EndSequence;
                d.VibrationGo = true;
                try
                {
                    // Default timeout is 50ms for both read and write operations
                    // It can be adjusted using DotManager.ReadTimeout and DotManager.WriteTimeout
                    // Alternatively, you can pass in your own CancellationToken.
                    await d.Write();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            // Don't Reset Dots, keep them red until Event 4.
        }

        /// <summary>
        /// Turns red and vibrates when each number is read in the Audio Book
        /// </summary>
        /// <returns></returns>
        private static async Task Number()
        {
            var random = new Random();
            int dotNumber = random.Next(1, 5);
            foreach (ManagedDot d in HapticManager.GetInstance().DotManager.Dots)
            {
                if (d.Address == dotNumber)
                {
                    Console.WriteLine("Dot " + dotNumber + " picked.");
                    var delay = Task.Delay(1000);
                    // Sets one LED Red
                    d.LedMode = LedModes.GlobalManual;
                    d.GlobalLed.Red = 255;
                    d.VibrationMode = VibrationModes.Library;
                    d.VibrationSequence[0].Waveforms = VibrationWaveforms.StrongClick1P100;
                    d.VibrationSequence[1].Waveforms = VibrationWaveforms.EndSequence;
                    d.VibrationGo = true;
                    try
                    {
                        // Default timeout is 50ms for both read and write operations
                        // It can be adjusted using DotManager.ReadTimeout and DotManager.WriteTimeout
                        // Alternatively, you can pass in your own CancellationToken.
                        await d.Write();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    await delay;
                }
            }

            ResetAsync(WristDot);
            ResetAsync(ChestDot);
        }

        /*
         * He imagined thousands on thousands of faces peering into yards, 
         * into alleys, and into the sky, faces hid by curtains, pale, 
         * night-frightened faces, like grey animals peering from electric caves, 
         * faces with grey colourless eyes, grey tongues and grey thoughts looking 
         * out through the numb flesh of the face.
         * Time: 1:12 - 1:31
         */
        private static void Event4()
        {

        }

        /*
         * But he was at the river.
         * He touched it, just to be sure it was real. 
         * He waded in and stripped in darkness to the skin, 
         * splashed his body, arms, legs, and head with raw liquor; 
         * drank it and snuffed some up his nose.
         * Time: 1:31 - 1:47
         */
        private static void Event5()
        {

        }

        /*
         * Then he dressed in Faber's old clothes and shoes. 
         * He tossed his own clothing into the river and watched it swept away. 
         * Then, holding the suitcase, he walked out in the river until 
         * there was no bottom and he was swept away in the dark.
         * Time: 1:54 - 2:00
         */
        private static void Event6()
        {

        }



    }
}
