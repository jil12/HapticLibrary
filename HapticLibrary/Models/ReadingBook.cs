using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            pages[0] = new ReadingPage("This is the first page", "");
            pages[1] = new ReadingPage("This is the second page", "");
            pages[2] = new ReadingPage("This is the last page", "");

            _pageIndex = 0;
        }

        public string GetText()
        {
            return pages[_pageIndex].pageText;
        }

        public string GetHaptics()
        {
            return pages[_pageIndex].pageHaptics;
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
        public string pageHaptics;  //type HapticKeywords
        public ReadingPage(string PageText, string PageHaptics) 
        {
            this.pageText = PageText;
            this.pageHaptics = PageHaptics;
        }
    }
}
