using System;
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
    /// Represents the PDF action into a PDF document.
    /// </summary>
    public class PDFAction : IDisposable
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private IDisposable m_Source;
        private PDFDocument m_Document;
        private ActionType m_ActionType = ActionType.Unknown;
        private string m_FilePath;
        private string m_URIPath;

        public PDFAction(PDFLink link, IntPtr nativePointer)
        {
            if (link == null)
                throw new NullReferenceException();
            if (nativePointer == IntPtr.Zero)
                throw new NullReferenceException();

            PDFLibrary.AddRef("PDFAction");

            m_Source = link;
            m_Document = link.Page.Document;

            m_NativePointer = nativePointer;
        }

        public PDFAction(PDFBookmark bookmark, IntPtr nativePointer)
        {
            if (bookmark == null)
                throw new NullReferenceException();
            if (nativePointer == IntPtr.Zero)
                throw new NullReferenceException();

            PDFLibrary.AddRef("PDFAction");

            m_Source = bookmark;
            m_Document = bookmark.Document;

            m_NativePointer = nativePointer;
        }

        ~PDFAction()
        {
            Dispose(false);
        }

        public enum ActionType
        {
            /// <summary>
            /// Unsupported action type.
            /// </summary>
            Unsupported = 0,
            /// <summary>
            /// Go to a destination within current document.
            /// </summary>
            GoTo = 1,
            /// <summary>
            /// Go to a destination within another document.
            /// </summary>
            RemoteGoTo = 2,
            /// <summary>
            /// Universal Resource Identifier, including web pages and other Internet based resources.
            /// </summary>
            Uri = 3,
            /// <summary>
            /// Launch an application or open a file.
            /// </summary>
            Launch = 4,
            Unknown = 133709999
        };

        public IDisposable Source
        {
            get { return m_Source; }
        }

        public PDFDocument Document
        {
            get { return m_Document; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
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

                PDFLibrary.RemoveRef("PDFAction");

                m_Disposed = true;
            }
        }

        /// <summary>
        /// Gets the PDFDest object associated with this action.
        /// </summary>
        /// <returns></returns>
        public PDFDest GetDest()
        {
            IntPtr destPtr = FPDFAction_GetDest(m_Document.NativePointer, m_NativePointer);
            if (destPtr != IntPtr.Zero)
                return new PDFDest(this, destPtr);
            return null;
        }

        public string GetFilePath()
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                byte[] buffer = new byte[4096];

                int filePathLength = (int)FPDFAction_GetFilePath(m_NativePointer, buffer, (uint)buffer.Length);
                if (filePathLength > 0)
                    m_FilePath =
                        Encoding.Unicode.GetString(Encoding.Convert(Encoding.ASCII, Encoding.Unicode, buffer, 0,
                            filePathLength));
            }

            return m_FilePath;
        }

        /// <summary>
        /// Gets type of current action.
        /// </summary>
        /// <returns></returns>
        public ActionType GetActionType()
        {
            if (m_ActionType == ActionType.Unknown)
                m_ActionType = (ActionType)FPDFAction_GetType(m_NativePointer);
            return m_ActionType;
        }

        /// <summary>
        /// Gets URL assigned to the current action.
        /// </summary>
        /// <returns></returns>
        public string GetURIPath()
        {
            if (string.IsNullOrEmpty(m_URIPath))
            {
                byte[] buffer = new byte[4096];

                int uriLength =
                    (int)
                        FPDFAction_GetURIPath(m_Document.NativePointer, m_NativePointer, buffer, (uint)buffer.Length);
                if (uriLength > 0)
                    m_URIPath =
                        Encoding.Unicode.GetString(Encoding.Convert(Encoding.ASCII, Encoding.Unicode, buffer, 0,
                            uriLength));
            }

            return m_URIPath;
        }

        #region NATIVE

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFAction_GetDest(IntPtr document, IntPtr action);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDFAction_GetFilePath(IntPtr action, [In, Out] byte[] buffer, uint buflen);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDFAction_GetType(IntPtr action);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDFAction_GetURIPath(IntPtr document, IntPtr action, [In, Out] byte[] buffer, uint buflen);

        #endregion
    }
#endif
}