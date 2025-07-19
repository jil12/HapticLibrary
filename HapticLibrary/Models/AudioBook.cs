using HapticLibrary.ViewModels;
using Avalonia.Controls.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datafeel;

namespace HapticLibrary.Models
{
    public partial class AudioBook : ViewModelBase
    {

        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private int _length;

        [ObservableProperty]
        private string _bookName;

        [ObservableProperty]
        private int _pageIndex;


        public AudioBook()
        {

        }

        public void LoadBook(string BookID)
        {

        }

        public void PreviousPage()
        {

        }

        public void NextPage()
        {

        }
    }



    public class AudioBookJson
    {
        public string Title { get; set; }
        public AudioMetadata Audio { get; set; }
        public List<AudioPage> Pages { get; set; }
        public List<Effect> Effects { get; set; }
    }

    public class AudioMetadata
    {
        public string Src { get; set; }
        public double Duration { get; set; }
    }

    public class AudioPage
    {
        public int PageNumber { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string Text { get; set; }

    }

    public class Effect
    {
        public string Type { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        //public List<DotSensations>? Dots { get; set; }
    }

    //public class DotSensations
    //{
    //    public ManagedDot Dot { get; set; }
    //    public bool IsVibrating { get; set; }
    //    public bool IsThermal { get; set; }
    //    public bool IsLED { get; set; }
    //}
}
