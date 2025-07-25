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
    public class ReadingBookJson
    {
        public string Name { get; set; } = string.Empty;
        public ReadingPage[] Pages { get; set; } = Array.Empty<ReadingPage>();
    }

    /**
     * Represents a page contents in a haptic reading book.
     */
    public class ReadingPage
    {
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, HapticEffect> HapticTriggers { get; set; } = new Dictionary<string, HapticEffect>();
        public ReadingPage(string text, Dictionary<string, HapticEffect> hapticTriggers)
        {
            this.Text = text;
            this.HapticTriggers = hapticTriggers;
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
        private string _bookName = string.Empty;
        
        public int PageIndex { get { return _pageIndex; } }
        public string BookName { get { return _bookName; } }

        public ReadingBook()
        {
        }
        public void LoadBook(string bookID) //TODO: ID or name? ID.
        {
            //TODO: Get book contents from server
            //string fullPath = Path.Combine("Assets", "HapticReadingBookExample.json");
            string fullPath = Path.Combine(Environment.CurrentDirectory, bookID);
            string jsonString = File.ReadAllText(fullPath);
            // Parse the JSON into a JsonDocument
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            ReadingBookJson readingBookJson = JsonSerializer.Deserialize<ReadingBookJson>(doc);
            pages = new List<ReadingPage>(readingBookJson.Pages);
            _bookName = readingBookJson.Name;
            _pageIndex = 0;
        }

        public string GetText()
        {
            if (pages.Count == 0 || _pageIndex >= pages.Count)
                return "No content available";
            return pages[_pageIndex].Text;
        }

        public void SetText(string text)
        {
            pages[_pageIndex].Text = text;
        }

        public Dictionary<string, HapticEffect> GetHaptics()
        {
            if (pages.Count == 0 || _pageIndex >= pages.Count)
                return new Dictionary<string, HapticEffect>();
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

        public void AddPage()
        {
            ReadingPage newPage = new ReadingPage("", new Dictionary<string, HapticEffect>());
            pages.Insert(_pageIndex+1, newPage);
        }
    }

    public class HapticEffect
    {
        public List<DotPropsJson> Props { get; set; } = new List<DotPropsJson>();
        //TODO: Add support for haptic patterns.
    }
}
