using Datafeel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static HapticLibrary.Models.ReadingBook;

namespace HapticLibrary.Models
{
    public class ReadingBookJson
    {
        public string Name { get; set; }
        public ReadingPage[] Pages { get; set; }
    }

    public class ReadingPage
    {
        public string Text { get; set; }
        public Dictionary<string, HapticEffect> HapticTriggers { get; set; }       //NOTE: Should this store dotProps or dotPropsJson? Shouldn't matter, Props is probably more correct, json is probably easier maybe??
                                                                                   //TODO: This should probably be stored alongside the "word" in the text instead of a dictionary.
        public ReadingPage(string Text, Dictionary<string, HapticEffect> HapticTriggers)
        {
            this.Text = Text;
            this.HapticTriggers = HapticTriggers;
        }
    }

    /**
     * Stores pages in a haptic reading book.
     */
    public class ReadingBook
    {
        

        /**
         * Represents a page contents in a haptic reading book.
         */
        

        private List<ReadingPage> pages = new();
        private int _pageIndex = 0;
        public int PageIndex { get { return _pageIndex; } }

        public ReadingBook()
        {
            pages = null;
        }

        public void LoadBook(string bookName) //TODO: ID or name?
        {
            //TODO: Get book contents from server
            //string fullPath = Path.Combine("Assets", "HapticReadingBookExample.json");
            string fullPath = Path.Combine(Environment.CurrentDirectory, bookName);
            string jsonString = File.ReadAllText(fullPath);
            // Parse the JSON into a JsonDocument
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            ReadingBookJson readingBookJson = JsonSerializer.Deserialize<ReadingBookJson>(doc);
            pages = new List<ReadingPage>(readingBookJson.Pages);
            _pageIndex = 0;
        }

        public string GetText()
        {
            return pages[_pageIndex].Text;
        }

        public void SetText(string text)
        {
            pages[_pageIndex].Text = text;
        }

        public Dictionary<string, HapticEffect> GetHaptics()
        {
            return pages[_pageIndex].HapticTriggers;
        }

        public void SetHaptics(Dictionary<string, HapticEffect> hapticTriggers)
        {
            pages[_pageIndex].HapticTriggers = hapticTriggers;
        }

        public int GetLength()
        {
            return (pages != null) ? pages.Count : 0;
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
