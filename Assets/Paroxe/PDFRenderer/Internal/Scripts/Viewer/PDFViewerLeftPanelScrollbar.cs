using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFViewerLeftPanelScrollbar : UIBehaviour
    {
        private CanvasGroup m_CanvasGroup;
        private Scrollbar m_Scrollbar;

        void LateUpdate()
        {
            if (m_Scrollbar.size >= 0.98f && m_CanvasGroup.alpha != 0.0f)
            {
                m_CanvasGroup.alpha = 0.0f;
            }
            else if (m_Scrollbar.size < 0.98f && m_CanvasGroup.alpha != 1.0f)
            {
                m_CanvasGroup.alpha = 1.0f;
            }
        }

        protected override void OnEnable()
        {
            m_Scrollbar = GetComponent<Scrollbar>();
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }
    }
}