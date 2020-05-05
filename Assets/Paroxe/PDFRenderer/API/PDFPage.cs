using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Represents a PDF page inside document.
    /// </summary>
    public class PDFPage : IDisposable, IEquatable<PDFPage>
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private PDFDocument m_Document;
        private int m_PageIndex;
#if !UNITY_WEBGL || UNITY_EDITOR
        private static Dictionary<IntPtr, int> s_InstanceMap = new Dictionary<IntPtr, int>();
#endif

        /// <summary>
        /// Rotations are clockwise
        /// </summary>
        public enum PageRotation
        {
            Normal = 0,
            Rotate90 = 1,
            Rotate180 = 2,
            Rotate270 = 3
        };

        public static PDFJS_Promise<PDFPage> LoadPageAsync(PDFDocument document, int pageIndex)
        {
            PDFJS_Promise<PDFPage> pagePromise = new PDFJS_Promise<PDFPage>();
#if UNITY_WEBGL && !UNITY_EDITOR


            LoadPageParameters parameters = new LoadPageParameters(document, pageIndex);

            PDFJS_Library.Instance.PreparePromiseCoroutine(LoadPageCoroutine, pagePromise, parameters).Start();


#else
            pagePromise.HasFinished = true;
            pagePromise.HasSucceeded = true;
            pagePromise.HasReceivedJSResponse = true;
            pagePromise.Result = document.GetPage(pageIndex);
#endif

            return pagePromise;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        public PDFPage(PDFDocument document, IntPtr pageHandle, int pageIndex)
        {
            PDFLibrary.AddRef("PDFPage");

            m_Document = document;
            m_PageIndex = pageIndex;

            m_NativePointer = pageHandle;
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
        public PDFPage(PDFDocument document, int pageIndex)
        {
            if (document == null)
                throw new NullReferenceException();
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException();

            PDFLibrary.AddRef("PDFPage");

            m_Document = document;
            m_PageIndex = pageIndex;

            m_NativePointer = FPDF_LoadPage(document.NativePointer, m_PageIndex);

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
#endif

        ~PDFPage()
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
                if (m_NativePointer != IntPtr.Zero)
                {
#if !UNITY_WEBGL || UNITY_EDITOR

                    lock (PDFLibrary.nativeLock)
                    {
                        s_InstanceMap[m_NativePointer] = s_InstanceMap[m_NativePointer] - 1;

                        if (s_InstanceMap[m_NativePointer] == 0)
                        {
                            if (m_Document.NativePointer != IntPtr.Zero)
                                FPDF_ClosePage(m_NativePointer);
                            s_InstanceMap.Remove(m_NativePointer);
                            m_NativePointer = IntPtr.Zero;
                        }
                    }

#else
                    PDFJS_ClosePage(m_NativePointer.ToInt32());
                    m_NativePointer = IntPtr.Zero;
#endif
                }

                PDFLibrary.RemoveRef("PDFPage");

                m_Disposed = true;
            }

        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        public PDFDocument Document
        {
            get { return m_Document; }
        }

        public int PageIndex
        {
            get { return m_PageIndex; }
        }

        public Vector2 GetPageSize(float scale = 1.0f)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            return m_Document.GetPageSize(m_PageIndex) * scale;
#else
            return new Vector2(
                PDFJS_GetPageWidth(m_NativePointer.ToInt32(), scale),
                PDFJS_GetPageHeight(m_NativePointer.ToInt32(), scale));
#endif

        }


#if UNITY_WEBGL && !UNITY_EDITOR
        internal static Vector2 GetPageSize(IntPtr pageHandle, float scale = 1.0f)
        {

            return new Vector2(
                PDFJS_GetPageWidth(pageHandle.ToInt32(), scale),
                PDFJS_GetPageHeight(pageHandle.ToInt32(), scale));

        }
#endif




#if !UNITY_WEBGL
        /// <summary>
        /// Return an instance of PDFTextPage that give access the the current page text content
        /// </summary>
        /// <returns></returns>
        public PDFTextPage GetTextPage()
        {
            return new PDFTextPage(this);
        }

        public PDFLink GetLinkAtPoint(Vector2 pagePoint)
        {
            IntPtr linkPtr = FPDFLink_GetLinkAtPoint(m_NativePointer, pagePoint.x, pagePoint.y);
            if (linkPtr != IntPtr.Zero)
                return new PDFLink(this, linkPtr);
            return null;
        }

        public PDFLink GetLinkAtPoint(double pageX, double pageY)
        {
            IntPtr linkPtr = FPDFLink_GetLinkAtPoint(m_NativePointer, pageX, pageY);
            if (linkPtr != IntPtr.Zero)
                return new PDFLink(this, linkPtr);
            return null;
        }

        public Vector2 DeviceToPage(int startX, int startY, int sizeX, int sizeY, PageRotation rotation, int deviceX, int deviceY)
        {
            double pageX;
            double pageY;
            FPDF_DeviceToPage(m_NativePointer, startX, startY, sizeX, sizeY, (int)rotation, deviceX, deviceY, out pageX,
                out pageY);

            return new Vector2((float)pageX, (float)pageY);
        }

        public Vector2 PageToDevice(int startX, int startY, int sizeX, int sizeY, PageRotation rotation, int pageX, int pageY)
        {
            int deviceX;
            int deviceY;
            FPDF_PageToDevice(m_NativePointer, startX, startY, sizeX, sizeY, (int)rotation, pageX, pageY, out deviceX,
                out deviceY);

            return new Vector2(deviceX, deviceY);
        }

        public Vector2 ConvertPagePositionToUnityUIDevicePosition(Vector2 pagePoint, Vector2 devicePageSize)
        {
            pagePoint = pagePoint.x / (devicePageSize.y / devicePageSize.x) * Vector2.right + pagePoint.y * Vector2.up;

            int device_x;
            int device_y;

            FPDF_PageToDevice(m_NativePointer, 0, 0, (int)devicePageSize.y, (int)devicePageSize.y, 0, pagePoint.x,
                pagePoint.y, out device_x, out device_y);

            return new Vector2(device_x, device_y);
        }

        public Rect ConvertPageRectToDeviceRect(Rect pageRect, Vector2 devicePageSize)
        {
            Vector2 min = ConvertPagePositionToUnityUIDevicePosition(pageRect.min, devicePageSize);
            float mx = pageRect.max.x;
            float my = (pageRect.min - (pageRect.max - pageRect.min)).y;
            Vector2 max = ConvertPagePositionToUnityUIDevicePosition(new Vector2(mx, my), devicePageSize);
            Rect rect = new Rect();
            rect.min = min;
            rect.max = max;
            return rect;
        }

        public Vector2 ConvertUnityUIDevicePositionToPagePosition(Vector2 devicePoint, Vector2 devicePageSize)
        {
            devicePoint = devicePoint.x * (devicePageSize.y / devicePageSize.x) * Vector2.right + devicePoint.y * Vector2.up;

            double page_x;
            double page_y;

            FPDF_DeviceToPage(m_NativePointer, 0, 0, (int)devicePageSize.y, (int)devicePageSize.y, 0,
                (int)devicePoint.x, (int)devicePoint.y, out page_x, out page_y);

            return new Vector2((float)page_x, (float)page_y);
        }

#endif
        public bool Equals(PDFPage other)
        {
            return (m_NativePointer != IntPtr.Zero && m_NativePointer == other.m_NativePointer);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private class LoadPageParameters
        {
            public PDFDocument document;
            public int pageIndex;

            public LoadPageParameters(PDFDocument document, int pageIndex)
            {
                this.document = document;
                this.pageIndex = pageIndex;
            }
        }

        private static IEnumerator LoadPageCoroutine(PDFJS_PromiseCoroutine promiseCoroutine, IPDFJS_Promise promise, object par)
        {
            PDFLibrary.Instance.EnsureInitialized();
            while (!PDFLibrary.Instance.IsInitialized)
                yield return null;

            PDFJS_Promise<PDFPage> pagePromise = promise as PDFJS_Promise<PDFPage>;

            LoadPageParameters parameters = par as LoadPageParameters;

            PDFJS_LoadPage(promise.PromiseHandle, parameters.document.NativePointer.ToInt32(), parameters.pageIndex + 1);

            while (!pagePromise.HasReceivedJSResponse)
                yield return null;

            if (pagePromise.HasSucceeded)
            {
                int pageHandle = int.Parse(pagePromise.JSObjectHandle);
                PDFPage page = new PDFPage(parameters.document, new IntPtr(pageHandle), parameters.pageIndex);

                pagePromise.Result = page;
                pagePromise.HasFinished = true;

                promiseCoroutine.ExecuteThenAction(true, page);
            }
            else
            {
                pagePromise.Result = null;
                pagePromise.HasFinished = true;

                promiseCoroutine.ExecuteThenAction(false, null);
            }
        }
#endif

        #region NATIVE
#if !UNITY_WEBGL || UNITY_EDITOR
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDF_ClosePage(IntPtr page);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFLink_GetLinkAtPoint(IntPtr page, double x, double y);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDF_DeviceToPage(IntPtr page, int start_x, int start_y, int size_x, int size_y,
        int rotate, int device_x, int device_y, out double page_x, out double page_y);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDF_PageToDevice(IntPtr page, int start_x, int start_y, int size_x, int size_y,
        int rotate, double page_x, double page_y, out int device_x, out int device_y);
#else
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_LoadPage(string promiseHandle, int documentHandle, int pageIndex);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_ClosePage(int pageHandle);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int PDFJS_GetPageWidth(int pageHandle, float scale);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int PDFJS_GetPageHeight(int pageHandle, float scale);
#endif
        #endregion
    }
}