using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
#if UNITY_WINRT && !UNITY_EDITOR
using File = UnityEngine.Windows.File;
#else
using File = System.IO.File;
#endif
using System.Runtime.InteropServices;

using UnityEngine;
using Paroxe.PdfRenderer.Internal;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Represents a PDF document. This class is the entry point of all functionalities.
    /// </summary>
    public class PDFDocument : IDisposable
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private byte[] m_DocumentBuffer;
        private bool m_ValidDocument;
        private PDFRenderer m_ConvenienceRenderer;

        public static PDFJS_Promise<PDFDocument> LoadDocumentFromUrlAsync(string url)
        {
            PDFJS_Promise<PDFDocument> documentPromise = new PDFJS_Promise<PDFDocument>();

#if !UNITY_WEBGL || UNITY_EDITOR

            PDFJS_Library.Instance.PreparePromiseCoroutine(LoadDocumentFromWWWCoroutine, documentPromise, url).Start();
#else
            LoadDocumentParameters parameters = new LoadDocumentParameters();
            parameters.url = url;

            PDFJS_Library.Instance.PreparePromiseCoroutine(LoadDocumentCoroutine, documentPromise, parameters).Start();
#endif

            return documentPromise;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private static IEnumerator LoadDocumentFromWWWCoroutine(PDFJS_PromiseCoroutine promiseCoroutine, IPDFJS_Promise promise, object urlString)
        {
            PDFJS_Promise<PDFDocument> documentPromise = promise as PDFJS_Promise<PDFDocument>;

            PDFLibrary.Instance.EnsureInitialized();
            while (!PDFLibrary.Instance.IsInitialized)
                yield return null;

            string url = urlString as string;

            PDFWebRequest www = new PDFWebRequest(url);
            www.SendWebRequest();

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                documentPromise.HasFinished = true;
                documentPromise.HasSucceeded = true;
                documentPromise.HasReceivedJSResponse = true;
                documentPromise.Result = new PDFDocument(www.bytes);

                promiseCoroutine.ExecuteThenAction(true, documentPromise.Result);
            }
            else
            {
                documentPromise.HasFinished = true;
                documentPromise.HasSucceeded = false;

                promiseCoroutine.ExecuteThenAction(false, null);
            }

            www.Dispose();
            www = null;
        }
#endif

        public static PDFJS_Promise<PDFDocument> LoadDocumentFromBytesAsync(byte[] bytes)
        {
            PDFJS_Promise<PDFDocument> documentPromise = new PDFJS_Promise<PDFDocument>();

#if !UNITY_WEBGL || UNITY_EDITOR
            documentPromise.HasFinished = true;
            documentPromise.HasSucceeded = true;
            documentPromise.HasReceivedJSResponse = true;
            documentPromise.Result = new PDFDocument(bytes);
#else
            LoadDocumentParameters parameters = new LoadDocumentParameters();
            parameters.bytes = bytes;

            PDFJS_Library.Instance.PreparePromiseCoroutine(LoadDocumentCoroutine, documentPromise, parameters).Start();
#endif

            return documentPromise;
        }

        public PDFDocument(IntPtr nativePointer)
        {
            PDFLibrary.AddRef("PDFDocument");

            m_NativePointer = nativePointer;
            m_ValidDocument = true;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Open PDF Document with the specified byte array.
        /// </summary>
        /// <param name="buffer"></param>
        public PDFDocument(byte[] buffer)
            : this(buffer, "")
        { }

        /// <summary>
        /// Open PDF Document with the specified byte array.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="password">Can be null or empty</param>
        public PDFDocument(byte[] buffer, string password)
        {
            PDFLibrary.AddRef("PDFDocument");

            CommonInit(buffer, password);
        }

        /// <summary>
        /// Open PDF Document located at the specified path
        /// </summary>
        /// <param name="filePath"></param>
        public PDFDocument(string filePath)
            : this(filePath, "")
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password">Can be null or empty</param>
        public PDFDocument(string filePath, string password)
        {
            PDFLibrary.AddRef("PDFDocument");

#if !UNITY_WEBPLAYER
            CommonInit(File.ReadAllBytes(filePath), password);
#else
            m_ValidDocument = false;
#endif
        }

#endif

        ~PDFDocument()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!m_Disposed)
            {
                lock (PDFLibrary.nativeLock)
                {
                    if (m_NativePointer != IntPtr.Zero)
                    {
#if UNITY_WEBGL && !UNITY_EDITOR
                        PDFJS_CloseDocument(m_NativePointer.ToInt32());
#else
                        FPDF_CloseDocument(m_NativePointer);
#endif
                        m_NativePointer = IntPtr.Zero;
                    }
                }

                PDFLibrary.RemoveRef("PDFDocument");

                m_Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Return a convenience PDFRenderer instance. 
        /// </summary>
        public PDFRenderer Renderer
        {
            get
            {
                if (m_ConvenienceRenderer == null)
                    m_ConvenienceRenderer = new PDFRenderer();
                return m_ConvenienceRenderer;
            }
        }

        /// <summary>
        /// The byte array of the document.
        /// </summary>
        public byte[] DocumentBuffer
        {
            get { return m_DocumentBuffer; }
        }

        /// <summary>
        /// Return if the document is valid. The document can be invalid if the password is invalid or if the
        /// document itseft is corrupted. See PDFLibrary.GetLastError.
        /// </summary>
        public bool IsValid
        {
            get { return m_ValidDocument; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        public int GetPageCount()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return PDFJS_GetPageCount(m_NativePointer.ToInt32());
#else
            return FPDF_GetPageCount(m_NativePointer);
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        public Vector2 GetPageSize(int pageIndex)
        {
            double width;
            double height;

            FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return new Vector2((float)width, (float)height);
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
        public int GetPageWidth(int pageIndex)
        {
            double width;
            double height;

            FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return (int)width;
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
        public int GetPageHeight(int pageIndex)
        {
            double width;
            double height;

            FPDF_GetPageSizeByIndex(m_NativePointer, pageIndex, out width, out height);

            return (int)height;
        }
#endif

#if !UNITY_WEBGL
        /// <summary>
        /// Return the root bookmark of the document.
        /// </summary>
        /// <returns></returns>
        public PDFBookmark GetRootBookmark()
        {
            return GetRootBookmark(null);
        }
#endif

#if !UNITY_WEBGL
        /// <summary>
        /// Return the root bookmark of the document.
        /// </summary>
        /// <param name="device">Pass the device that will receive bookmarks action</param>
        /// <returns></returns>
        public PDFBookmark GetRootBookmark(IPDFDevice device)
        {
            return new PDFBookmark(this, null, IntPtr.Zero, device);
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
        public PDFPage GetPage(int index)
        {
            return new PDFPage(this, index);
        }
#endif

        public PDFJS_Promise<PDFPage> GetPageAsync(int index)
        {
            return PDFPage.LoadPageAsync(this, index);
        }

        private void CommonInit(byte[] buffer, string password)
        {
            m_DocumentBuffer = buffer;

            if (m_DocumentBuffer != null)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                m_NativePointer = FPDF_LoadMemDocument(m_DocumentBuffer, m_DocumentBuffer.Length, password);
#endif

                m_ValidDocument = (m_NativePointer != IntPtr.Zero);
            }
            else
            {
                m_ValidDocument = false;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        class LoadDocumentParameters
        {
            public string url;
            public byte[] bytes;
        }

        private static IEnumerator LoadDocumentCoroutine(PDFJS_PromiseCoroutine promiseCoroutine, IPDFJS_Promise promise, object pars)
        {
            PDFJS_Promise<PDFDocument> documentPromise = promise as PDFJS_Promise<PDFDocument>;

            PDFLibrary.Instance.EnsureInitialized();
            while (!PDFLibrary.Instance.IsInitialized)
                yield return null;

            LoadDocumentParameters parameters = pars as LoadDocumentParameters;

            if (!string.IsNullOrEmpty(parameters.url))
                PDFJS_LoadDocumentFromURL(promise.PromiseHandle, parameters.url);
            else
                PDFJS_LoadDocumentFromBytes(promise.PromiseHandle, Convert.ToBase64String(parameters.bytes));

            while (!promiseCoroutine.Promise.HasReceivedJSResponse)
                yield return null;

            if (documentPromise.HasSucceeded)
            {
                int documentHandle = int.Parse(promiseCoroutine.Promise.JSObjectHandle);
                PDFDocument document = new PDFDocument(new IntPtr(documentHandle));

                documentPromise.Result = document;
                documentPromise.HasFinished = true;

                promiseCoroutine.ExecuteThenAction(true, documentHandle);
            }
            else
            {
                documentPromise.Result = null;
                documentPromise.HasFinished = true;

                promiseCoroutine.ExecuteThenAction(false, null);
            }
        }
#endif

        #region NATIVE

#if !UNITY_WEBGL || UNITY_EDITOR
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDF_CloseDocument(IntPtr document);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern uint FPDF_GetDocPermissions(IntPtr document);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDF_GetPageCount(IntPtr document);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY, CharSet = CharSet.Ansi)]
        private static extern IntPtr FPDF_LoadMemDocument(byte[] data_buf, int size, string password);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);
#else
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_LoadDocumentFromURL(string promiseHandle, string documentUrl);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_LoadDocumentFromBytes(string promiseHandle, string base64);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_CloseDocument(int document);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int PDFJS_GetPageCount(int documentHandle);
#endif

        #endregion
    }
}