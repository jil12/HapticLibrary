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
            
            pages = new ReadingPage[3];

            pages[0] = new ReadingPage("This is the first page", new Dictionary<string, IDotProps>());
            pages[1] = new ReadingPage("This is the second page", new Dictionary<string, IDotProps>());
            pages[2] = new ReadingPage("This is the last page", new Dictionary<string, IDotProps>());
            DotProps dotProps = new DotProps(new DotPropsJson());
            TrackPlayer
            _pageIndex = 0;

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "HapticReadingBookExample.json");
            string jsonString = File.ReadAllText(fullPath);

            // Parse the JSON into a JsonDocument
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement root = doc.RootElement;

            // Access data dynamically
            var user = root.GetProperty("user");
            string name = user.GetProperty("name").GetString();
            int id = user.GetProperty("id").GetInt32();

            bool active = root.GetProperty("active").GetBoolean();

            var roles = root.GetProperty("roles");
            foreach (JsonElement role in roles.EnumerateArray())
            {
                Console.WriteLine($"Role: {role.GetString()}");
            }

            // Print results
            Console.WriteLine($"Name: {name}, ID: {id}, Active: {active}");
        }

        public string GetText()
        {
            return pages[_pageIndex].pageText;
        }

        public Dictionary<string, IDotProps> GetHaptics()
        {
            return pages[_pageIndex].wordTriggers;
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

    /**
     * Represents a page contents in a haptic reading book.
     */
    public class ReadingPage
    {
        public string pageText;
        public Dictionary<string, IDotProps> wordTriggers;       //Should this store dotProps or dotPropsJson? Shouldn't matter, Props is probably more correct, json is probably easier maybe??
        public ReadingPage(string PageText, Dictionary<string, IDotProps> PageHaptics) 
        {
            this.pageText = PageText;
            this.wordTriggers = PageHaptics;
        }
    }
}
