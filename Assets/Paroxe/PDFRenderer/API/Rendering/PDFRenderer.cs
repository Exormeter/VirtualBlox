using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Paroxe.PdfRenderer.Internal;
using UnityEngine;
using System.Threading;
using System.Collections;
using Paroxe.PdfRenderer.WebGL;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// This class allow the application to render pages into textures.
    /// </summary>
    public class PDFRenderer : IDisposable
    {
        private bool m_Disposed;
#if !UNITY_WEBGL || UNITY_EDITOR
        private PDFBitmap m_Bitmap;
        private byte[] m_IntermediateBuffer;
#endif

        public PDFRenderer()
        {
            PDFLibrary.AddRef("PDFRenderer");
        }

        ~PDFRenderer()
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
                if (disposing)
                {
#if !UNITY_WEBGL || UNITY_EDITOR
                    m_Bitmap.Dispose();
                    m_Bitmap = null;
#endif
                }

                PDFLibrary.RemoveRef("PDFRenderer");

                m_Disposed = true;
            }
        }

#if UNITY_WEBGL
        public class RenderPageParameters
        {
            public IntPtr pageHandle;
            public Texture2D existingTexture;
            public Vector2 newTextureSize;


            public RenderPageParameters(IntPtr pageHandle, Texture2D existingTexture, Vector2 newTextureSize)
            {
                this.pageHandle = pageHandle;
                this.existingTexture = existingTexture;
                this.newTextureSize = newTextureSize;
            }
        }
#endif

        public static PDFJS_Promise<Texture2D> RenderPageToExistingTextureAsync(PDFPage page, Texture2D tex)
        {
            PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();

#if !UNITY_WEBGL || UNITY_EDITOR
            using (PDFRenderer renderer = new PDFRenderer())
            {
                renderPromise.HasFinished = true;
                renderPromise.HasSucceeded = true;
                renderPromise.HasReceivedJSResponse = true;
                renderer.RenderPageToExistingTexture(page, tex);
                renderPromise.Result = tex;
            }
#else

            RenderPageParameters parameters = new RenderPageParameters(page.NativePointer, tex, new Vector2(tex.width, tex.height));

            PDFJS_Library.Instance.PreparePromiseCoroutine(RenderPageCoroutine, renderPromise, parameters).Start();
#endif

            return renderPromise;
        }

        public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, int width, int height)
        {
            return RenderPageToTextureAsync(page, new Vector2(width, height));
        }

        public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, Vector2 size)
        {
            PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();

#if !UNITY_WEBGL || UNITY_EDITOR
            using (PDFRenderer renderer = new PDFRenderer())
            {
                renderPromise.HasFinished = true;
                renderPromise.HasSucceeded = true;
                renderPromise.HasReceivedJSResponse = true;
                renderPromise.Result = renderer.RenderPageToTexture(page, (int)size.x, (int)size.y);
            }
#else
            RenderPageParameters parameters = new RenderPageParameters(page.NativePointer, null, size);

            PDFJS_Library.Instance.PreparePromiseCoroutine(RenderPageCoroutine, renderPromise, parameters).Start();

#endif
            return renderPromise;
        }

        public static PDFJS_Promise<Texture2D> RenderPageToTextureAsync(PDFPage page, float scale = 1.0f)
        {
            PDFJS_Promise<Texture2D> renderPromise = new PDFJS_Promise<Texture2D>();

#if !UNITY_WEBGL || UNITY_EDITOR
            using (PDFRenderer renderer = new PDFRenderer())
            {
                renderPromise.HasFinished = true;
                renderPromise.HasSucceeded = true;
                renderPromise.HasReceivedJSResponse = true;
                Vector2 size = page.GetPageSize(scale);
                renderPromise.Result = renderer.RenderPageToTexture(page, (int)size.x, (int)size.y);
            }
#else

            RenderPageParameters parameters = new RenderPageParameters(page.NativePointer, null, page.GetPageSize(scale));

            PDFJS_Library.Instance.PreparePromiseCoroutine(RenderPageCoroutine, renderPromise, parameters).Start();
#endif

            return renderPromise;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static IEnumerator RenderPageCoroutine(PDFJS_PromiseCoroutine promiseCoroutine, IPDFJS_Promise promise, object parameters)
        {
            PDFJS_Promise<PDFJS_WebGLCanvas> renderToCanvasPromise = new PDFJS_Promise<PDFJS_WebGLCanvas>();

            PDFJS_Library.Instance.PreparePromiseCoroutine(null, renderToCanvasPromise, null);

            IntPtr pageHandle = ((RenderPageParameters)parameters).pageHandle;
            Texture2D texture = ((RenderPageParameters)parameters).existingTexture;
            Vector2 newtextureSize = ((RenderPageParameters)parameters).newTextureSize;

            Vector2 pageSize = PDFPage.GetPageSize(pageHandle, 1.0f);

            float scale = 1.0f;
            float width = 0.0f;
            float height = 0.0f;

            if (texture != null)
            {
                float wf = pageSize.x / texture.width;
                float hf = pageSize.y / texture.height;

                width = texture.width;
                height = texture.height;

                scale = 1.0f / Mathf.Max(wf, hf);
            }
            else
            {
                float wf = pageSize.x / newtextureSize.x;
                float hf = pageSize.y / newtextureSize.y;

                width = newtextureSize.x;
                height = newtextureSize.y;

                scale = 1.0f / Mathf.Max(wf, hf);
            }

            PDFJS_RenderPageIntoCanvas(renderToCanvasPromise.PromiseHandle, pageHandle.ToInt32(), scale, width, height);

            while (!renderToCanvasPromise.HasReceivedJSResponse)
                yield return null;

            if (renderToCanvasPromise.HasSucceeded)
            {
                int canvasHandle = int.Parse(renderToCanvasPromise.JSObjectHandle);

                using (PDFJS_WebGLCanvas canvas = new PDFJS_WebGLCanvas(new IntPtr(canvasHandle)))
                {
                    PDFJS_Promise<Texture2D> renderToTexturePromise = promise as PDFJS_Promise<Texture2D>;

                    if (texture == null)
                    {
                        texture = new Texture2D((int)newtextureSize.x, (int)newtextureSize.y, TextureFormat.ARGB32, false);
                        texture.filterMode = FilterMode.Bilinear;
                        texture.Apply();
                    }


                    PDFJS_RenderCanvasIntoTexture(canvasHandle, texture.GetNativeTexturePtr().ToInt32());

                    renderToTexturePromise.Result = texture;
                    renderToTexturePromise.HasSucceeded = true;
                    renderToTexturePromise.HasFinished = true;

                    promiseCoroutine.ExecuteThenAction(true, texture);
                }
            }
            else
            {
                PDFJS_Promise<Texture2D> renderToTexturePromise = promise as PDFJS_Promise<Texture2D>;

                renderToTexturePromise.Result = null;
                renderToTexturePromise.HasSucceeded = false;
                renderToTexturePromise.HasFinished = true;

                promiseCoroutine.ExecuteThenAction(false, null);
            }
        }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Render page into a new byte array.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        byte[] RenderPageToByteArray(PDFPage page)
        {
            return RenderPageToByteArray(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y, null,
                RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new byte array.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        byte[] RenderPageToByteArray(PDFPage page, int width, int height)
        {
            return RenderPageToByteArray(page, width, height, null, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new byte array.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rectsProvider"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        byte[] RenderPageToByteArray(PDFPage page, int width, int height,
            IPDFColoredRectListProvider rectsProvider)
        {
            return RenderPageToByteArray(page, width, height, rectsProvider, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new byte array.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rectsProvider"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        byte[] RenderPageToByteArray(PDFPage page, int width, int height,
            IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
        {
            if (m_Bitmap == null || !m_Bitmap.HasSameSize(width, height))
            {
                if (m_Bitmap != null)
                    m_Bitmap.Dispose();

                m_Bitmap = new PDFBitmap(width, height, false);
            }

            m_Bitmap.FillRect(0, 0, width, height, int.MaxValue);

            int flags = settings == null
                ? RenderSettings.defaultRenderSettings.ComputeRenderingFlags()
                : settings.ComputeRenderingFlags();

            FPDF_RenderPageBitmap(m_Bitmap.NativePointer, page.NativePointer, 0, 0, width, height, 0, flags);

            IntPtr bufferPtr = m_Bitmap.GetBuffer();

            if (bufferPtr == IntPtr.Zero)
                return null;

            int length = width * height * 4;

            if (m_IntermediateBuffer == null || m_IntermediateBuffer.Length < length)
                m_IntermediateBuffer = new byte[width * height * 4];

            Marshal.Copy(bufferPtr, m_IntermediateBuffer, 0, width * height * 4);

#if !UNITY_WEBGL

            IList<PDFColoredRect> coloredRects = rectsProvider != null
                ? rectsProvider.GetBackgroundColoredRectList(page)
                : null;

            if (coloredRects != null && coloredRects.Count > 0)
            {
                foreach (PDFColoredRect coloredRect in coloredRects)
                {
                    var r = (int)(coloredRect.Color.r * 255) & 0xFF;
                    var g = (int)(coloredRect.Color.g * 255) & 0xFF;
                    var b = (int)(coloredRect.Color.b * 255) & 0xFF;
                    var a = (int)(coloredRect.Color.a * 255) & 0xFF;

                    float alpha = (a / (float)255);
                    float reverseAlpha = 1.0f - alpha;

                    Rect deviceRect = page.ConvertPageRectToDeviceRect(coloredRect.PageRect, new Vector2(width, height));

                    for (int y = 0; y < -(int)deviceRect.height; ++y)
                    {
                        for (int x = 0; x < (int)deviceRect.width; ++x)
                        {
                            int s = (((int)deviceRect.y + y + (int)deviceRect.height) * width + (int)deviceRect.x + x) * 4;

                            var sr = m_IntermediateBuffer[s];
                            var sg = m_IntermediateBuffer[s + 1];
                            var sb = m_IntermediateBuffer[s + 2];

                            m_IntermediateBuffer[s] = (byte)Mathf.Clamp(alpha * r + (reverseAlpha * sr), 0, 255);
                            m_IntermediateBuffer[s + 1] = (byte)Mathf.Clamp(alpha * g + (reverseAlpha * sg), 0, 255);
                            m_IntermediateBuffer[s + 2] = (byte)Mathf.Clamp(alpha * b + (reverseAlpha * sb), 0, 255);
                            m_IntermediateBuffer[s + 3] = 0xFF;
                        }
                    }
                }
            }
#endif


            return m_IntermediateBuffer;
        }

        /// <summary>
        /// Render page into a new Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        Texture2D RenderPageToTexture(PDFPage page)
        {
            return RenderPageToTexture(page, (int)page.GetPageSize().x, (int)page.GetPageSize().y, null,
                RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        Texture2D RenderPageToTexture(PDFPage page, int width, int height)
        {
            return RenderPageToTexture(page, width, height, null, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rectsProvider"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        Texture2D RenderPageToTexture(PDFPage page, int width, int height,
            IPDFColoredRectListProvider rectsProvider)
        {
            return RenderPageToTexture(page, width, height, rectsProvider, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into a new Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rectsProvider"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        Texture2D RenderPageToTexture(PDFPage page, int width, int height,
            IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
        {
            Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            RenderPageToExistingTexture(page, newTex, rectsProvider, settings);

            return newTex;
        }

        /// <summary>
        /// Render page into an existing Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="texture"></param>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        void RenderPageToExistingTexture(PDFPage page, Texture2D texture)
        {
            RenderPageToExistingTexture(page, texture, null, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into an existing Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="texture"></param>
        /// <param name="rectsProvider"></param>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        void RenderPageToExistingTexture(PDFPage page, Texture2D texture,
            IPDFColoredRectListProvider rectsProvider)
        {
            RenderPageToExistingTexture(page, texture, rectsProvider, RenderSettings.defaultRenderSettings);
        }

        /// <summary>
        /// Render page into an existing Texture2D.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="texture"></param>
        /// <param name="rectsProvider"></param>
        /// <param name="settings"></param>
#if !UNITY_WEBGL
        public
#else
        private
#endif
        void RenderPageToExistingTexture(PDFPage page, Texture2D texture,
            IPDFColoredRectListProvider rectsProvider, RenderSettings settings)
        {
            byte[] byteArray = RenderPageToByteArray(page, texture.width, texture.height, rectsProvider, settings);

            if (byteArray != null)
            {
                if ((texture.format != TextureFormat.RGBA32
                     && texture.format != TextureFormat.ARGB32
                     && texture.format != TextureFormat.BGRA32
                     && texture.format != (TextureFormat)37) || texture.mipmapCount > 1)
                {
                    Color32[] pixels = new Color32[texture.width * texture.height];

                    for (int i = 0; i < pixels.Length; ++i)
                        pixels[i] = new Color32(
                            byteArray[i * 4],
                            byteArray[i * 4 + 1],
                            byteArray[i * 4 + 2],
                            byteArray[i * 4 + 3]);

                    texture.SetPixels32(pixels);
                    texture.Apply();
                }
                else
                {
                    texture.LoadRawTextureData(byteArray);
                    texture.Apply();
                }
            }
        }

#endif

        /// <summary>
        /// Allows the application to specify render settings.
        /// </summary>
        [Serializable]
        public class RenderSettings
        {
            public bool disableSmoothPath = false;
            public bool disableSmoothText = false;
            public bool disableSmoothImage = false;
            public bool grayscale = false;
            public bool optimizeTextForLCDDisplay = false;
            public bool renderAnnotations = false;
            public bool renderForPrinting = false;

            public static RenderSettings defaultRenderSettings
            {
                get { return new RenderSettings(); }
            }

            public int ComputeRenderingFlags()
            {
                int flags = 0x10;

                if (renderAnnotations)
                    flags |= 0x01;
                if (optimizeTextForLCDDisplay)
                    flags |= 0x02;
                if (grayscale)
                    flags |= 0x08;
                if (renderForPrinting)
                    flags |= 0x800;
                if (disableSmoothText)
                    flags |= 0x1000;
                if (disableSmoothImage)
                    flags |= 0x2000;
                if (disableSmoothPath)
                    flags |= 0x4000;

                return flags;
            }
        }


        #region NATIVE
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_RenderPageIntoCanvas(string promiseHandle, int pageHandle, float scale, float width, float height);

        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_RenderCanvasIntoTexture(int canvasHandle, int textureHandle);
#else
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);
#endif
        #endregion
    }
}