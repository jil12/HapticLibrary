using Datafeel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HapticLibrary.Models
{
    /**
     * Stores pages in a haptic reading book.
     */
    public class ReadingBook
    {
        private class ReadingBookJson
        {
            public ReadingPage[] Pages { get; set; }
        }

        /**
         * Represents a page contents in a haptic reading book.
         */
        private class ReadingPage
        {
            public string Text { get; set; }
            public Dictionary<string, HapticEffect> HapticTriggers { get; set; }       //Should this store dotProps or dotPropsJson? Shouldn't matter, Props is probably more correct, json is probably easier maybe??
            public ReadingPage(string text, Dictionary<string, HapticEffect> hapticTriggers)
            {
                this.Text = text;
                this.HapticTriggers = hapticTriggers;
            }
        }

        private ReadingPage[]? pages;
        private int _pageIndex = 0;
        public int PageIndex { get { return _pageIndex; } }

        public ReadingBook()
        {
            pages = null;
        }

        public void LoadBook(string bookID) //TODO: ID or name?
        {
            //TODO: Get book contents from server
            //string fullPath = Path.Combine("Assets", "HapticReadingBookExample.json");
            string fullPath = "C:/Users/Austin/Projects/Datafeel-Storytelling/HapticLibrary/HapticLibrary/Assets/HapticReadingBookExample.json";
            string jsonString = File.ReadAllText(fullPath);
            // Parse the JSON into a JsonDocument
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            ReadingBookJson readingBookJson = JsonSerializer.Deserialize<ReadingBookJson>(doc);
            pages = readingBookJson.Pages;
            _pageIndex = 0;
        }

        public string GetText()
        {
            return pages[_pageIndex].Text;
        }

        public Dictionary<string, HapticEffect> GetHaptics()
        {
            return pages[_pageIndex].HapticTriggers;
        }

        public int GetLength()
        {
            return (pages != null) ? pages.Length : 0;
        }

        public void NextPage()
        {
            if (_pageIndex < GetLength() - 1)
            {
                _pageIndex++;
            }
        }

        public void PreviousPage()
        {
            if (_pageIndex > 0)
            {
                _pageIndex--;
            }
        }
    }

    public class HapticEffect
    {
        public List<DotPropsJson> Props { get; set; }
        //TODO: Add support for haptic patterns.
    }
}
