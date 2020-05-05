
namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Reprensent a search result. To result location is describe with index of the first char and the length of the result.
    /// </summary>
    public struct PDFSearchResult
    {
        private readonly int m_PageIndex;
        private readonly int m_StartIndex; // index of the first character
        private readonly int m_Count; // number of characters

        public PDFSearchResult(int pageIndex, int startIndex, int count)
        {
            m_PageIndex = pageIndex;
            m_StartIndex = startIndex;
            m_Count = count;
        }

        /// <summary>
        /// Indicate whether the result is valid or invalid.
        /// </summary>
        public bool IsValid
        {
            get { return m_PageIndex != -1; }
        }

        /// <summary>
        /// The pageIndex of the result.
        /// </summary>
        public int PageIndex
        {
            get { return m_PageIndex; }
        }

        /// <summary>
        /// The index of the first character of the result within the page.
        /// </summary>
        public int StartIndex
        {
            get { return m_StartIndex; }
        }

        /// <summary>
        /// The length of the result within the page.
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }
    }
}