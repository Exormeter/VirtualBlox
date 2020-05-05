using Paroxe.PdfRenderer.WebGL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFPageRange : IEquatable<PDFPageRange>
    {
        public int m_From = -1;
        public int m_To = -1;

        public bool IsValid
        {
            get { return (m_From != -1 && m_To != -1); }
        }

        public int RangeLength
        {
            get { return IsValid ? (m_To - m_From) : 0; }
        }

        public static int[] GetPagesToload(PDFPageRange from, PDFPageRange to)
        {
            List<int> pagesToLoad = new List<int>();

            if (!from.IsValid)
            {
                for (int i = to.m_From; i < to.m_To; ++i)
                {
                    pagesToLoad.Add(i);
                }

                return pagesToLoad.ToArray();
            }

            bool[] toLoad = new bool[Math.Max(from.m_To, to.m_To)];

            for (int i = to.m_From; i < to.m_To; ++i)
            {
                toLoad[i] = true;
            }

            for (int i = from.m_From; i < from.m_To; ++i)
            {
                toLoad[i] = false;
            }

            for (int i = 0; i < toLoad.Length; ++i)
            {
                if (toLoad[i])
                {
                    pagesToLoad.Add(i);
                }
            }

            return pagesToLoad.ToArray();
        }

        public static int[] GetPagesToUnload(PDFPageRange from, PDFPageRange to)
        {
            List<int> pagesToUnLoad = new List<int>();

            if (!from.IsValid)
            {
                return pagesToUnLoad.ToArray();
            }

            bool[] toUnload = new bool[Math.Max(from.m_To, to.m_To)];

            for (int i = from.m_From; i < from.m_To; ++i)
            {
                toUnload[i] = true;
            }

            for (int i = to.m_From; i < to.m_To; ++i)
            {
                toUnload[i] = false;
            }

            for (int i = 0; i < toUnload.Length; ++i)
            {
                if (toUnload[i])
                {
                    pagesToUnLoad.Add(i);
                }
            }

            return pagesToUnLoad.ToArray();
        }

        public static void UpdatePageAgainstRanges(PDFPageRange fromRange, PDFPageRange toRange, PDFDocument pdfDocument,
            PDFPageTextureHolder[] pageTextureHolderList, PDFRenderer.RenderSettings renderSettings, float scale,
            IPDFColoredRectListProvider rectsProvider, Vector2[] pageSizes)
        {
            int[] pagesToLoad = GetPagesToload(fromRange, toRange);
            int[] pagesToUnLoad = GetPagesToUnload(fromRange, toRange);

            List<Texture2D> recyclableTextures = new List<Texture2D>();

            for (int i = 0; i < pagesToUnLoad.Length; ++i)
            {
#if UNITY_WEBGL
                pageTextureHolderList[pagesToUnLoad[i]].m_Visible = false;

                if (pageTextureHolderList[pagesToUnLoad[i]].m_RenderingPromise != null)
                {
                    PDFJS_Library.Instance.TryTerminateRenderingWorker(pageTextureHolderList[pagesToUnLoad[i]].m_RenderingPromise.PromiseHandle);
                    pageTextureHolderList[pagesToUnLoad[i]].m_RenderingPromise = null;
                }

                Texture2D tex = pageTextureHolderList[pagesToUnLoad[i]].Texture;
                if (tex != null)
                {
                    recyclableTextures.Add(tex);
                    pageTextureHolderList[pagesToUnLoad[i]].Texture = null;
                }
#else
                Texture2D tex = pageTextureHolderList[pagesToUnLoad[i]].Texture;
                if (tex != null)
                {
                    recyclableTextures.Add(tex);
                    pageTextureHolderList[pagesToUnLoad[i]].Texture = null;
                }
#endif
            }

            for (int i = 0; i < pagesToLoad.Length; ++i)
            {
#if UNITY_WEBGL
                pageTextureHolderList[pagesToLoad[i]].m_Visible = true;

                if (pageTextureHolderList[pagesToLoad[i]].m_RenderingStarted)
                    continue;
#endif

                int w = Mathf.RoundToInt(pageSizes[pagesToLoad[i]].x * scale);
                int h = Mathf.RoundToInt(pageSizes[pagesToLoad[i]].y * scale);

                Texture2D tex = null;

                foreach (Texture2D texture in recyclableTextures)
                {
                    if (texture.width == w && texture.height == h)
                    {
                        tex = texture;
                        break;
                    }
                }

#if UNITY_WEBGL
                if (tex != null)
                {
                    recyclableTextures.Remove(tex);

                    pageTextureHolderList[pagesToLoad[i]].m_RenderingStarted = true;
                    PDFJS_Library.Instance.StartCoroutine(UpdatePageWithExistingTexture(pdfDocument, pagesToLoad[i], tex, pageTextureHolderList));
                }
                else
                {

                    pageTextureHolderList[pagesToLoad[i]].m_RenderingStarted = true;
                    PDFJS_Library.Instance.StartCoroutine(UpdatePageWithNewTexture(pdfDocument, pagesToLoad[i], pageTextureHolderList, w, h));
                }
#else
                if (tex != null)
                {
                    recyclableTextures.Remove(tex);
                    pdfDocument.Renderer.RenderPageToExistingTexture(pdfDocument.GetPage(pagesToLoad[i]), tex, rectsProvider, renderSettings);
                }
                else
                    tex = pdfDocument.Renderer.RenderPageToTexture(pdfDocument.GetPage(pagesToLoad[i]), w, h, rectsProvider, renderSettings);

                pageTextureHolderList[pagesToLoad[i]].Texture = tex;
#endif
            }

            foreach (Texture2D unusedTexture in recyclableTextures)
            {
                UnityEngine.Object.Destroy(unusedTexture);
            }

            recyclableTextures.Clear();
        }

#if UNITY_WEBGL
        public static IEnumerator UpdatePageWithExistingTexture(PDFDocument document, int pageIndex, Texture2D existingTexture, PDFPageTextureHolder[] pageTextureHolderList)
        {
            PDFJS_Promise<PDFPage> pagePromise = document.GetPageAsync(pageIndex);

            while (!pagePromise.HasFinished)
                yield return null;

            if (pagePromise.HasSucceeded)
            {
                PDFJS_Promise<Texture2D> renderPromise = PDFRenderer.RenderPageToExistingTextureAsync(pagePromise.Result, existingTexture);

                pageTextureHolderList[pageIndex].m_RenderingPromise = renderPromise;

                while (!renderPromise.HasFinished)
                    yield return null;

                pageTextureHolderList[pageIndex].m_RenderingPromise = null;
                pageTextureHolderList[pageIndex].m_RenderingStarted = false;

                if (renderPromise.HasSucceeded)
                {
                    if (pageTextureHolderList[pageIndex].Texture != null
                        && pageTextureHolderList[pageIndex].Texture != renderPromise.Result)
                    {
                        UnityEngine.Object.Destroy(pageTextureHolderList[pageIndex].Texture);
                        pageTextureHolderList[pageIndex].Texture = null;
                    }

                    if (pageTextureHolderList[pageIndex].m_Visible)
                        pageTextureHolderList[pageIndex].Texture = renderPromise.Result;
                    else
                    {

                        UnityEngine.Object.Destroy(renderPromise.Result);
                        renderPromise.Result = null;
                    }
                }
            }
            else
            {
                pageTextureHolderList[pageIndex].m_RenderingPromise = null;
                pageTextureHolderList[pageIndex].m_RenderingStarted = false;
            }
        }

        public static IEnumerator UpdatePageWithNewTexture(PDFDocument document, int pageIndex, PDFPageTextureHolder[] pageTextureHolderList, int width, int height)
        {
            PDFJS_Promise<PDFPage> pagePromise = document.GetPageAsync(pageIndex);

            while (!pagePromise.HasFinished)
                yield return null;

            if (pagePromise.HasSucceeded)
            {
                PDFJS_Promise<Texture2D> renderPromise = PDFRenderer.RenderPageToTextureAsync(pagePromise.Result, width, height);

                pageTextureHolderList[pageIndex].m_RenderingPromise = renderPromise;

                while (!renderPromise.HasFinished)
                    yield return null;

                pageTextureHolderList[pageIndex].m_RenderingPromise = null;
                pageTextureHolderList[pageIndex].m_RenderingStarted = false;

                if (renderPromise.HasSucceeded)
                {
                    if (pageTextureHolderList[pageIndex].Texture != null)
                    {
                        UnityEngine.Object.Destroy(pageTextureHolderList[pageIndex].Texture);
                        pageTextureHolderList[pageIndex].Texture = null;
                    }

                    if (pageTextureHolderList[pageIndex].m_Visible)
                        pageTextureHolderList[pageIndex].Texture = renderPromise.Result;
                    else
                    {
                        UnityEngine.Object.Destroy(renderPromise.Result);
                        renderPromise.Result = null;
                    }
                }
            }
            else
            {
                pageTextureHolderList[pageIndex].m_RenderingPromise = null;
                pageTextureHolderList[pageIndex].m_RenderingStarted = false;
            }
        }
#endif


        public bool ContainsPage(int pageIndex)
        {
            if (!IsValid)
            {
                return false;
            }
            return (pageIndex >= m_From && pageIndex < m_To);
        }

        public bool Equals(PDFPageRange other)
        {
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            var objectToCompareWith = (PDFPageRange)other;

            return (objectToCompareWith.m_From == m_From && objectToCompareWith.m_To == m_To);
        }

        public override int GetHashCode()
        {
            var calculation = m_From + m_To;
            return calculation.GetHashCode();
        }
    }
}