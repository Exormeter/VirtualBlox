using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if NETFX_CORE && !UNITY_WSA_10_0
using WinRTLegacy.Text;
#else
using System.Text;
#endif

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// Represents the bookmark into a PDF document.
    /// </summary>
    public class PDFBookmark : IDisposable
    {
        private bool m_Disposed;
        private List<PDFBookmark> m_Bookmarks = new List<PDFBookmark>();
        private PDFDocument m_Document;
        private IntPtr m_NativePointer;
        private PDFBookmark m_ParentBookmark;
        private IPDFDevice m_Device;
        private string m_Title;

        public PDFBookmark(PDFDocument document, PDFBookmark parentBookmark, IntPtr nativePointer)
            : this(document, parentBookmark, nativePointer, null)
        {
            if (document == null)
                throw new NullReferenceException();
        }

        public PDFBookmark(PDFDocument document, PDFBookmark parentBookmark, IntPtr nativePointer, IPDFDevice device)
        {
            if (document == null)
                throw new NullReferenceException();

            m_ParentBookmark = parentBookmark;
            m_NativePointer = nativePointer;
            m_Document = document;
            m_Device = device;

            if (m_NativePointer == IntPtr.Zero)
                m_Title = "ROOT";

            PDFLibrary.AddRef("PDFBookmark");

            PDFBookmark firstChild = GetFirstChild();

            if (firstChild != null)
            {
                PDFBookmark previousSibling = firstChild;

                while (previousSibling != null)
                {
                    m_Bookmarks.Add(previousSibling);
                    previousSibling = previousSibling.GetNextSibling();
                }
            }
        }

        ~PDFBookmark()
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
                m_NativePointer = IntPtr.Zero;

                PDFLibrary.RemoveRef("PDFBookmark");

                m_Disposed = true;
            }
        }

        public PDFDocument Document
        {
            get { return m_Document; }
        }

        public int ChildCount
        {
            get { return m_Bookmarks.Count; }
        }

        public bool IsTopLevelBookmark
        {
            get { return (m_ParentBookmark == null || m_ParentBookmark.NativePointer == IntPtr.Zero); }
        }

        public PDFBookmark Parent
        {
            get { return m_ParentBookmark; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        public IList<PDFBookmark> GetChildrenBookmarks()
        {
            return m_Bookmarks;
        }

        public IEnumerable<PDFBookmark> EnumerateChildrenBookmarks()
        {
            foreach (PDFBookmark child in m_Bookmarks)
            {
                yield return child;
            }
        }

        public void ExecuteBookmarkAction()
        {
            if (m_Device != null)
                PDFActionHandlerHelper.ExecuteBookmarkAction(m_Device, this);
        }

        public PDFBookmark GetChild(int index)
        {
            return m_Bookmarks[index];
        }

        public string GetTitle()
        {
            if (string.IsNullOrEmpty(m_Title))
            {
                byte[] buffer = new byte[4096];

                int length = (int)FPDFBookmark_GetTitle(m_NativePointer, buffer, (uint)buffer.Length);
                if (length > 0)
                    m_Title = Encoding.Unicode.GetString(buffer);
            }
            return m_Title;
        }

        public PDFAction GetAction()
        {
            IntPtr actionPtr = FPDFBookmark_GetAction(m_NativePointer);
            if (actionPtr != IntPtr.Zero)
                return new PDFAction(this, actionPtr);
            return null;
        }

        public PDFDest GetDest()
        {
            IntPtr destPtr = FPDFBookmark_GetDest(m_Document.NativePointer, m_NativePointer);
            if (destPtr != IntPtr.Zero)
                return new PDFDest(this, destPtr);
            return null;
        }

        public PDFBookmark GetFirstChild()
        {
            IntPtr childPtr = FPDFBookmark_GetFirstChild(m_Document.NativePointer, m_NativePointer);
            if (childPtr != IntPtr.Zero)
                return new PDFBookmark(m_Document, this, childPtr, m_Device);
            return null;
        }

        public PDFBookmark GetNextSibling()
        {
            IntPtr nextPtr = FPDFBookmark_GetNextSibling(m_Document.NativePointer, m_NativePointer);
            if (nextPtr != IntPtr.Zero)
                return new PDFBookmark(m_Document, m_ParentBookmark, nextPtr, m_Device);
            return null;
        }

        #region NATIVE


        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBookmark_GetAction(IntPtr bookmark);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBookmark_GetDest(IntPtr document, IntPtr bookmark);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBookmark_GetFirstChild(IntPtr document, IntPtr bookmark);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBookmark_GetNextSibling(IntPtr document, IntPtr bookmark);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDFBookmark_GetTitle(IntPtr bookmark, [In, Out] byte[] buffer, uint buflen);

        #endregion
    }
#endif
}