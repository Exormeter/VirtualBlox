using System;
using System.Runtime.InteropServices;

namespace Paroxe.PdfRenderer.Internal
{
    public class PDFBitmap : IDisposable
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;
        private readonly int m_Width;
        private readonly int m_Height;
        private readonly bool m_UseAlphaChannel;

        public PDFBitmap(int width, int height, bool useAlphaChannel)
        {
            PDFLibrary.AddRef("PDFBitmap");

            m_Width = width;
            m_Height = height;
            m_UseAlphaChannel = useAlphaChannel;

            m_NativePointer = FPDFBitmap_Create(m_Width, m_Height, useAlphaChannel);
        }

        ~PDFBitmap()
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
                        FPDFBitmap_Destroy(m_NativePointer);
                    m_NativePointer = IntPtr.Zero;
                }


                PDFLibrary.RemoveRef("PDFBitmap");

                m_Disposed = true;
            }
        }

        public int Width
        {
            get { return m_Width; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public bool UseAlphaChannel
        {
            get { return m_UseAlphaChannel; }
        }

        public IntPtr NativePointer
        {
            get { return m_NativePointer; }
        }

        public bool HasSameSize(PDFBitmap other)
        {
            return (m_Width == other.m_Width && m_Height == other.m_Height);
        }

        public bool HasSameSize(int width, int height)
        {
            return (m_Width == width && m_Height == height);
        }

        public void FillRect(int left, int top, int width, int height, int color)
        {
            FPDFBitmap_FillRect(m_NativePointer, left, top, width, height, color);
        }

        public IntPtr GetBuffer()
        {
            return FPDFBitmap_GetBuffer(m_NativePointer);
        }

        public int GetStride()
        {
            return FPDFBitmap_GetStride(m_NativePointer);
        }

        #region NATIVE

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBitmap_Create(int width, int height, bool alpha);

        //[DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        //private static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr firstScan, int stride);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFBitmap_Destroy(IntPtr bitmap);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDFBitmap_FillRect(IntPtr bitmap, int left, int top, int width, int height, int color);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern IntPtr FPDFBitmap_GetBuffer(IntPtr bitmap);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern int FPDFBitmap_GetStride(IntPtr bitmap);

        #endregion
    }
}