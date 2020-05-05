using System;
using System.Runtime.InteropServices;

namespace Paroxe.PdfRenderer.WebGL
{
    public class PDFJS_WebGLCanvas : IDisposable
    {
        private bool m_Disposed;
        private IntPtr m_NativePointer;

        public PDFJS_WebGLCanvas(IntPtr nativePointer)
        {
            m_NativePointer = nativePointer;
        }

        ~PDFJS_WebGLCanvas()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (m_NativePointer != IntPtr.Zero)
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    PDFJS_DestroyCanvas(m_NativePointer.ToInt32());
#endif
                    m_NativePointer = IntPtr.Zero;
                }

                m_Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_DestroyCanvas(int canvasHandle);
#endif

    }
}