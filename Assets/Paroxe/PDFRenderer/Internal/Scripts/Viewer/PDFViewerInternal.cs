using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFViewerInternal : UIBehaviour
    {
        [SerializeField]
        public RectTransform m_DownloadDialog;
        [SerializeField]
        public Text m_DownloadSourceLabel;
        [SerializeField]
        public bool m_DrawDefaultInspector = false;
        [SerializeField]
        public RectTransform m_HorizontalScrollBar;
        [SerializeField]
        public Image m_InvalidPasswordImage;
        [SerializeField]
        public PDFViewerLeftPanel m_LeftPanel = null;
        [SerializeField]
        public CanvasGroup m_Overlay;
        [SerializeField]
        public RectTransform m_PageContainer;
        [SerializeField]
        public Text m_PageCountLabel;
        [SerializeField]
        public Button m_PageDownButton;
        [SerializeField]
        public InputField m_PageInputField;
        [SerializeField]
        public RawImage m_PageSample;
        [SerializeField]
        public Button m_PageUpButton;
        [SerializeField]
        public Text m_PageZoomLabel;
        [SerializeField]
        public RectTransform m_PasswordDialog;
        [SerializeField]
        public InputField m_PasswordInputField;
        [SerializeField]
        public Text m_ProgressLabel;
        [SerializeField]
        public RectTransform m_ProgressRect;
        [SerializeField]
        public RectTransform m_ScrollCorner;
        [SerializeField]
        public ScrollRect m_ScrollRect;
        [SerializeField]
        public RectTransform m_TopPanel;
        [SerializeField]
        public RectTransform m_VerticalScrollBar;
        [SerializeField]
        public RectTransform m_Viewport;
        [SerializeField]
        public RectTransform m_SearchPanel;

        public PDFViewer m_PDFViewer = null;

        public void OnDownloadCancelButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.OnDownloadCancelButtonClicked();
            }
        }

        public void OnNextPageButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.GoToNextPage();
            }
        }

        public void OnPageIndexEditEnd()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.OnPageEditEnd();
            }
        }

        public void OnPasswordDialogCancelButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.OnPasswordDialogCancelButtonClicked();
            }
        }

        public void OnPasswordDialogOkButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.OnPasswordDialogOkButtonClicked();
            }
        }

        public void OnPreviousPageButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.GoToPreviousPage();
            }
        }

        public void OnZoomInButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.ZoomIn();
            }
        }

        public void OnZoomOutButtonClicked()
        {
            if (m_PDFViewer != null)
            {
                m_PDFViewer.ZoomOut();
            }
        }
    }
}