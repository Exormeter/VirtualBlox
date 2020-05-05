using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if NETFX_CORE && !UNITY_WSA_10_0
using WinRTLegacy.Text;
#else
using System.Text;
#endif

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// This class allow to access the text of a page.
    /// </summary>
    public class PDFTextPage : IDisposable, IEquatable<PDFTextPage>
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private PDFPage m_Page;
        private static Dictionary<IntPtr, int> s_InstanceMap = new Dictionary<IntPtr, int>();

        public PDFTextPage(PDFPage page)
        {
            if (page == null)
                throw new NullReferenceException();

            PDFLibrary.AddRef("PDFTextPage");

            m_Page = page;
            m_NativePointer = FPDFText_LoadPage(m_Page.NativePointer);

            if (m_NativePointer != IntPtr.Zero)
            {
                if (s_InstanceMap.ContainsKey(m_NativePointer))
                {
                    s_InstanceMap[m_NativePointer] = s_InstanceMap[m_NativePointer] + 1;
                }
                else
                    s_InstanceMap[m_NativePointer] = 1;
            }
        }

        ~PDFTextPage()
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
                        s_InstanceMap[m_NativePointer] = s_InstanceMap[m_NativePointer] - 1;

                        if (s_InstanceMap[m_NativePointer] == 0)
                        {
                            if (m_Page.NativePointer != IntPtr.Zero && m_Page.Document.NativePointer != IntPtr.Zero)
                                FPDFText_ClosePage(m_NativePointer);
                            s_InstanceMap.Remove(m_NativePointer);
                            m_NativePointer = IntPtr.Zero;
                        }
                    }
                }

                PDFLibrary.RemoveRef("PDFTextPage");

                m_Disposed = true;
            }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        /// <summary>
        /// Return a refence to the page.
        /// </summary>
        public PDFPage Page
        {
            get { return m_Page; }
        }

        /// <summary>
        /// Return the page index of the text.
        /// </summary>
        public int PageIndex
        {
            get { return m_Page.PageIndex; }
        }

        /// <summary>
        /// Return the number of character in the page.
        /// </summary>
        /// <returns></returns>
        public int CountChars()
        {
            return FPDFText_CountChars(m_NativePointer);
        }

        /// <summary>
        /// Count number of rectangular areas occupied by a segment of texts.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int CountRects(int startIndex, int count)
        {
            return FPDFText_CountRects(m_NativePointer, startIndex, count);
        }

        /// <summary>
        /// Extract text within a rectangular boundary on the page.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public string GetBoundedText(float left, float top, float right, float bottom, int charCount)
        {
            byte[] textBuffer = new byte[(charCount + 1) * 2];
            FPDFText_GetBoundedText(m_NativePointer, left, top, right, bottom, textBuffer, textBuffer.Length);
            return Encoding.Unicode.GetString(textBuffer);
        }

        /// <summary>
        /// Get bounding box of a particular character.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public Rect GetCharBox(int charIndex)
        {
            double left;
            double right;
            double bottom;
            double top;

            FPDFText_GetCharBox(m_NativePointer, charIndex, out left, out right, out bottom, out top);

            return new Rect((float)left, (float)top, Mathf.Abs((float)right - (float)left),
                Mathf.Abs((float)bottom - (float)top));
        }

        /// <summary>
        /// Get the index of a character at or nearby a certain position on the page.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public int GetCharIndexAtPos(Vector2 pos, Vector2 tolerance)
        {
            /*Vector2 pagePos = m_Page.ConvertUnityUIDevicePositionToPagePosition(pos,
                m_Page.Document.GetPageSize(m_Page.PageIndex));*/

            return FPDFText_GetCharIndexAtPos(m_NativePointer, pos.x, pos.y, tolerance.x, tolerance.y);
        }

        /// <summary>
        /// Get the font size of a particular character.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public double GetFontSize(int charIndex)
        {
            return FPDFText_GetFontSize(m_NativePointer, charIndex);
        }

        /// <summary>
        /// Get a rectangular area from the result generated by CountRects
        /// </summary>
        /// <param name="rectIndex"></param>
        /// <returns></returns>
        public Rect GetRect(int rectIndex)
        {
            double left;
            double right;
            double bottom;
            double top;

            FPDFText_GetRect(m_NativePointer, rectIndex, out left, out top, out right, out bottom);

            return new Rect((float)left, (float)top, Mathf.Abs((float)right - (float)left),
                Mathf.Abs((float)bottom - (float)top));
        }

        /// <summary>
        /// Extract text string from the page.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string GetText(int startIndex, int count)
        {
            byte[] textBuffer = new byte[(count + 1) * 2];
            FPDFText_GetText(m_NativePointer, startIndex, count, textBuffer);
            return Encoding.Unicode.GetString(textBuffer);
        }

        /// <summary>
        /// Get a character in the page.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public string GetChar(int charIndex)
        {
            return char.ConvertFromUtf32((int)FPDFText_GetUnicode(m_NativePointer, charIndex));
        }

        public IList<PDFSearchResult> Search(string findWhat,
            PDFSearchHandle.MatchOption flags = PDFSearchHandle.MatchOption.NONE, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(findWhat.Trim()))
                return new List<PDFSearchResult>();

            return Search(Encoding.Unicode.GetBytes(findWhat.Trim() + "\0"), flags, startIndex);
        }

        public IList<PDFSearchResult> Search(byte[] findWhatUnicode,
            PDFSearchHandle.MatchOption flags = PDFSearchHandle.MatchOption.NONE, int startIndex = 0)
        {
            List<PDFSearchResult> searchResults = new List<PDFSearchResult>();

            if (findWhatUnicode == null)
                return searchResults;

            using (PDFSearchHandle searchHandle = new PDFSearchHandle(this, findWhatUnicode, startIndex, flags))
            {
                foreach (PDFSearchResult result in searchHandle.EnumerateSearchResults())
                {
                    searchResults.Add(result);
                }
            }

            return searchResults;
        }

        public bool Equals(PDFTextPage other)
        {
            return (m_NativePointer != IntPtr.Zero && m_NativePointer == other.m_NativePointer);
        }

        #region NATIVE

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFText_LoadPage(IntPtr page);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFText_ClosePage(IntPtr text_page);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_CountChars(IntPtr text_page);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_CountRects(IntPtr text_page, int start_index, int count);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_GetBoundedText(IntPtr text_page, double left, double top, double right,
            double bottom, [In, Out] byte[] buffer, int buflen);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFText_GetCharBox(IntPtr text_page, int index, out double left, out double right,
            out double bottom, out double top);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_GetCharIndexAtPos(IntPtr text_page, double x, double y, double xTolerance,
            double yTolerance);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern double FPDFText_GetFontSize(IntPtr text_page, int index);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFText_GetRect(IntPtr text_page, int rect_index, out double left, out double top,
            out double right, out double bottom);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFText_GetText(IntPtr text_page, int start_index, int count, [In, Out] byte[] buffer);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDFText_GetUnicode(IntPtr text_page, int index);

        #endregion
    }
#endif
}