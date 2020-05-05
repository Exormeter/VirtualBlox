using Paroxe.PdfRenderer.WebGL;
using UnityEngine;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFPageTextureHolder
    {
        public AspectRatioFitter m_AspectRatioFitter;
        public int m_PageIndex;
        public GameObject m_Page;
#if UNITY_WEBGL
        public bool m_RenderingStarted;
        public bool m_Visible;
        public IPDFJS_Promise m_RenderingPromise;
#endif

        private Texture2D m_Texture;
        public PDFViewer m_PDFViewer;

        public void RefreshTexture()
        {
            Texture = m_Texture;
        }

        public Texture2D Texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                m_Texture = value;

                RawImage rawImage = m_Page.GetComponent<RawImage>();
                if (rawImage == null)
                    rawImage = m_Page.gameObject.AddComponent<RawImage>();

                if (value != null)
                {
                    rawImage.texture = value;
                    rawImage.uvRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                    rawImage.color = Color.white;
                }
                else
                {
                    if (m_PDFViewer.PageTileTexture != null)
                    {
                        rawImage.texture = m_PDFViewer.PageTileTexture;

                        RectTransform rt = rawImage.transform as RectTransform;

                        rawImage.uvRect = new Rect(0.0f, 0.0f,
                            rt.sizeDelta.x / rawImage.texture.width,
                            rt.sizeDelta.y / rawImage.texture.height);
                    }
                    else
                    {
                        rawImage.texture = null;
                    }

                    rawImage.color = m_PDFViewer.PageColor;
                }

                if (m_AspectRatioFitter != null)
                    m_AspectRatioFitter.aspectRatio = value.width / (float)value.height;
            }
        }
    }
}