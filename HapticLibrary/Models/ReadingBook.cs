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
            public ReadingPage[] Pages { get; set; } = Array.Empty<ReadingPage>();
        }

        /**
         * Represents a page contents in a haptic reading book.
         */
        private class ReadingPage
        {
            public string Text { get; set; } = string.Empty;
            public Dictionary<string, HapticEffect> HapticTriggers { get; set; } = new Dictionary<string, HapticEffect>();
            public ReadingPage(string text, Dictionary<string, HapticEffect> hapticTriggers)
            {
                this.Text = text;
                this.HapticTriggers = hapticTriggers;
            }
        }

        private ReadingPage[] pages = Array.Empty<ReadingPage>();
        private int _pageIndex = 0;
        public int PageIndex { get { return _pageIndex; } }

        public ReadingBook()
        {
            pages = Array.Empty<ReadingPage>();
        }

        public void LoadBook(string bookID) //TODO: ID or name?
        {
            try
        {
            //TODO: Get book contents from server
            string fullPath = Path.Combine("Assets", "HapticReadingBookExample.json");
                
                // Check if file exists
                if (!File.Exists(fullPath))
                {
                    // Try to find the file in the current directory or subdirectories
                    string? foundPath = FindFileInDirectory("HapticReadingBookExample.json", Directory.GetCurrentDirectory());
                    if (foundPath != null)
                    {
                        fullPath = foundPath;
                    }
                    else
                    {
                        throw new FileNotFoundException($"Could not find HapticReadingBookExample.json in {fullPath} or subdirectories");
                    }
                }
                
            string jsonString = File.ReadAllText(fullPath);
            // Parse the JSON into a JsonDocument
            using JsonDocument doc = JsonDocument.Parse(jsonString);
                ReadingBookJson? readingBookJson = JsonSerializer.Deserialize<ReadingBookJson>(doc);
                
                if (readingBookJson?.Pages != null)
                {
            pages = readingBookJson.Pages;
                }
                else
                {
                    pages = Array.Empty<ReadingPage>();
                }
                _pageIndex = 0;
            }
            catch (Exception ex)
            {
                // Log the error and set default content
                System.Diagnostics.Debug.WriteLine($"Error loading book: {ex.Message}");
                pages = new ReadingPage[]
                {
                    new ReadingPage("Error loading book content.", new Dictionary<string, HapticEffect>())
                };
            _pageIndex = 0;
            }
        }

        private string? FindFileInDirectory(string fileName, string directory)
        {
            try
            {
                string fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;

                // Search in subdirectories
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    string? found = FindFileInDirectory(fileName, subDir);
                    if (found != null)
                        return found;
                }
            }
            catch
            {
                // Ignore access errors
            }
            return null;
        }

        public string GetText()
        {
            if (pages.Length == 0 || _pageIndex >= pages.Length)
                return "No content available";
            return pages[_pageIndex].Text;
        }

        public Dictionary<string, HapticEffect> GetHaptics()
        {
            if (pages.Length == 0 || _pageIndex >= pages.Length)
                return new Dictionary<string, HapticEffect>();
            return pages[_pageIndex].HapticTriggers;
        }

        public int GetLength()
        {
            return pages.Length;
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
        public List<DotPropsJson> Props { get; set; } = new List<DotPropsJson>();
        //TODO: Add support for haptic patterns.
    }
}
