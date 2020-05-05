using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFThumbnailItem : UIBehaviour
    {
        public AspectRatioFitter m_AspectRatioFitter;
        public Image m_Highlighted;
        public LayoutElement m_LayoutElement;
        public Text m_PageIndexLabel;
        public RawImage m_PageThumbnailRawImage;
        public RectTransform m_RectTransform;

        public void LateUpdate()
        {
            m_LayoutElement.preferredHeight = 180.0f * (m_RectTransform.sizeDelta.x / 320.0f);
        }

        public void OnThumbnailClicked()
        {
            GetComponentInParent<PDFViewer>().GoToPage(int.Parse(m_PageIndexLabel.text) - 1);
        }
    }
}