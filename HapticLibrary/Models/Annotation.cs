using System;

namespace HapticLibrary.Models
{
    public class Annotation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BookId { get; set; } = "";
        public string SelectedText { get; set; } = "";
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string HapticPreset { get; set; } = "";
    }
} 