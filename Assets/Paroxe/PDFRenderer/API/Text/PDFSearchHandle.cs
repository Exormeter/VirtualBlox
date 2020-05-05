using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// Represent a search session within a specific page. To search in entire document use PDFTextPage.Search
    /// </summary>
    public class PDFSearchHandle : IDisposable
    {
        public enum MatchOption
        {
            NONE = 0x00000000,
            MATCH_CASE = 0x00000001,
            MATCH_WHOLE_WORD = 0x00000002,
            MATCH_CASE_AND_WHOLE_WORD = 0x00000003
        }

        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private PDFTextPage m_TextPage;

        public PDFSearchHandle(PDFTextPage textPage, byte[] findWhatUnicode, int startIndex,
            MatchOption flags = MatchOption.NONE)
        {
            if (textPage == null)
                throw new NullReferenceException();
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException();

            PDFLibrary.AddRef("PDFSearchHandle");

            m_TextPage = textPage;

            IntPtr unmanagedPointer = Marshal.AllocHGlobal(findWhatUnicode.Length);
            Marshal.Copy(findWhatUnicode, 0, unmanagedPointer, findWhatUnicode.Length);

            m_NativePointer = FPDFText_FindStart(textPage.NativePointer, unmanagedPointer, (int)flags, startIndex);

            Marshal.FreeHGlobal(unmanagedPointer);
        }

        ~PDFSearchHandle()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                lock (PDFLibrary.nativeLock)
                {
                    if (m_NativePointer != IntPtr.Zero)
                    {
                        FPDFText_FindClose(m_NativePointer);
                        m_NativePointer = IntPtr.Zero;
                    }
                }

                PDFLibrary.RemoveRef("PDFSearchHandle");

                m_Disposed = true;
            }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        /// <summary>
        /// Return an array containing all the searchResults. If there is no result, this function return null.
        /// </summary>
        /// <returns></returns>
        public IList<PDFSearchResult> GetResults()
        {
            List<PDFSearchResult> results = new List<PDFSearchResult>();

            foreach (PDFSearchResult result in EnumerateSearchResults())
            {
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Enumerate search results.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PDFSearchResult> EnumerateSearchResults()
        {
            if (m_NativePointer != IntPtr.Zero)
            {
                while (FPDFText_FindNext(m_NativePointer))
                    yield return new PDFSearchResult(
                        m_TextPage.PageIndex,
                        FPDFText_GetSchResultIndex(m_NativePointer),
                        FPDFText_GetSchCount(m_NativePointer));
            }
        }

        /// <summary>
        /// Get the next search result. If there is no other result, the function returns an invalid searchResult (validate it with PDFSearchResult.IsValid)
        /// </summary>
        /// <returns></returns>
        public PDFSearchResult FindNext()
        {
            if (FPDFText_FindNext(m_NativePointer))
                return new PDFSearchResult(
                    m_TextPage.PageIndex,
                    FPDFText_GetSchResultIndex(m_NativePointer),
                    FPDFText_GetSchCount(m_NativePointer));
            return new PDFSearchResult(-1, -1, -1);
        }

        /// <summary>
        /// Get the previous search result. If there is no other result, the function returns an invalid searchResult (validate it with PDFSearchResult.IsValid)
        /// </summary>
        /// <returns></returns>
        public PDFSearchResult FindPrevious()
        {
            if (FPDFText_FindPrev(m_NativePointer))
                return new PDFSearchResult(
                    m_TextPage.PageIndex,
                    FPDFText_GetSchResultIndex(m_NativePointer),
                    FPDFText_GetSchCount(m_NativePointer));
            return new PDFSearchResult(-1, -1, -1);
        }

        #region NATIVE


        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFText_FindStart(IntPtr text_page, IntPtr buffer, int flags, int start_index);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFText_FindClose(IntPtr handle);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern bool FPDFText_FindNext(IntPtr handle);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern bool FPDFText_FindPrev(IntPtr handle);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_GetSchCount(IntPtr handle);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_GetSchResultIndex(IntPtr handle);

        #endregion
    }

#endif
}