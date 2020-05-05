using System;
using System.Runtime.InteropServices;

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// Represents the annotation link in a PDF page.
    /// </summary>
    public class PDFLink : IDisposable
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private PDFPage m_Page;

        public PDFLink(PDFPage page, IntPtr nativePointer)
        {
            if (page == null)
                throw new NullReferenceException();
            if (nativePointer == IntPtr.Zero)
                throw new NullReferenceException();

            PDFLibrary.AddRef("PDFLink");

            m_Page = page;

            m_NativePointer = nativePointer;
        }

        ~PDFLink()
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

                PDFLibrary.RemoveRef("PDFLink");

                m_Disposed = true;
            }
        }

        public PDFPage Page
        {
            get { return m_Page; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        /// <summary>
        /// Gets the named destination assigned to a link. Return null if there is no destination associated with the link,
        /// in this case the application should try GetAction() instead.
        /// </summary>
        /// <returns></returns>
        public PDFDest GetDest()
        {
            IntPtr destPtr = FPDFLink_GetDest(m_Page.Document.NativePointer, m_NativePointer);
            if (destPtr != IntPtr.Zero)
                return new PDFDest(this, destPtr);
            return null;
        }

        /// <summary>
        /// Gets the PDF action assigned to a link.
        /// </summary>
        /// <returns></returns>
        public PDFAction GetAction()
        {
            IntPtr actionPtr = FPDFLink_GetAction(m_NativePointer);
            if (actionPtr != IntPtr.Zero)
                return new PDFAction(this, actionPtr);
            return null;
        }

        #region NATIVE

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFLink_GetAction(IntPtr link);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFLink_GetDest(IntPtr document, IntPtr link);

        #endregion
    }
#endif
}