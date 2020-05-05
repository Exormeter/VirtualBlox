using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFViewerLeftPanel : UIBehaviour
    {
        public RectTransform m_Bookmarks;
        public Image m_BookmarksTab;
        public Text m_BookmarksTabTitle;
        public Sprite m_ClosedTabSprite;
        public Sprite m_CloseSprite;
        public float m_MaxWidth = 500.0f;
        public float m_MinWidth = 250.0f;
        public Sprite m_OpenedTabSprite;
        public Sprite m_OpenSprite;
        public Texture2D m_ResizeCursor;
        public Image m_SideBarImage;
        public RectTransform m_Thumbnails;
        public Scrollbar m_ThumbnailsScrollbar;
        public Image m_ThumbnailsTab;
        public Text m_ThumbnailsTabTitle;
        public PDFThumbnailsViewer m_ThumbnailsViewer;

        private RectTransform m_HorizontalScrollBar;
        private float m_LastPanelWidth;
        private bool m_Opened = true;
        private RectTransform m_RectTransform;
        private Vector2 m_StartDragPointerPosition;
        private RectTransform m_ViewerViewport;
        private bool m_Drag = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        private bool m_PointerIn = false;

#endif

        public void OnBeginDrag()
        {
            if (!m_Opened)
                return;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Cursor.SetCursor(m_ResizeCursor, new Vector2(16.0f, 16.0f), CursorMode.Auto);
            m_Drag = true;
#endif
        }

        public void OnBookmarksTabClicked()
        {
            m_BookmarksTab.sprite = m_OpenedTabSprite;
            m_BookmarksTabTitle.color = Color.black;
            m_ThumbnailsTab.sprite = m_ClosedTabSprite;
            m_ThumbnailsTabTitle.color = new Color(0.50f, 0.50f, 0.50f);

            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().interactable = true;
            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;

            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().interactable = false;
            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        public void OnDrag(BaseEventData eventData)
        {
            if (!m_Drag)
                return;

            var pointerData = eventData as PointerEventData;
            if (pointerData == null)
                return;

            m_RectTransform.sizeDelta += new Vector2(pointerData.delta.x, 0.0f);
            m_RectTransform.sizeDelta = new Vector2(Mathf.Clamp(m_RectTransform.sizeDelta.x, m_MinWidth, m_MaxWidth),
                m_RectTransform.sizeDelta.y);
            m_LastPanelWidth = m_RectTransform.sizeDelta.x;

            UpdateViewport();
        }

        public void OnEndDrag()
        {
            if (!m_Drag)
                return;

            if (!m_Opened)
                return;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (!m_PointerIn)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            m_Drag = false;
#endif
        }

        public void OnPointerEnter()
        {
            if (!m_Opened)
            {
                return;
            }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Cursor.SetCursor(m_ResizeCursor, new Vector2(16.0f, 16.0f), CursorMode.Auto);
            m_PointerIn = true;
#endif
        }

        public void OnPointerExit()
        {
            if (!m_Opened)
            {
                return;
            }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (!m_Drag)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            m_PointerIn = false;
#endif
        }

        public void OnThumbnailsTabClicked()
        {
            m_BookmarksTab.sprite = m_ClosedTabSprite;
            m_BookmarksTabTitle.color = new Color(0.50f, 0.50f, 0.50f);
            m_ThumbnailsTab.sprite = m_OpenedTabSprite;
            m_ThumbnailsTabTitle.color = Color.black;

            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().interactable = false;
            m_Bookmarks.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;

            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().interactable = true;
            m_Thumbnails.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        public void SetActive(bool active)
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = transform as RectTransform;
            }
            if (m_ViewerViewport == null)
            {
                m_ViewerViewport = GetComponentsInParent<PDFViewer>(true)[0].m_Internal.m_Viewport;
            }
            if (m_HorizontalScrollBar == null)
            {
                m_HorizontalScrollBar = GetComponentsInParent<PDFViewer>(true)[0].m_Internal.m_HorizontalScrollBar;
            }

            gameObject.SetActive(active);
            if (!active)
            {
                m_ViewerViewport.offsetMin = new Vector2(0.0f, m_ViewerViewport.offsetMin.y);
                m_HorizontalScrollBar.offsetMin = new Vector2(0.0f, m_HorizontalScrollBar.offsetMin.y);
            }
            else
            {
                UpdateViewport();
            }
        }

        public bool IsOpened
        {
            get { return m_Opened; }
        }

        public void SetOpened(bool opened)
        {
            m_Opened = opened;
            UpdateGraphics();
            UpdateViewport();
        }

        public void Toggle()
        {
            m_Opened = !m_Opened;
            UpdateGraphics();
            UpdateViewport();
        }

        protected override void OnEnable()
        {
            m_RectTransform = transform as RectTransform;
            m_ViewerViewport = GetComponentInParent<PDFViewer>().m_Internal.m_Viewport;
            m_HorizontalScrollBar = GetComponentInParent<PDFViewer>().m_Internal.m_HorizontalScrollBar;

            m_LastPanelWidth = 350.0f;

            UpdateViewport();

        }

        private void UpdateGraphics()
        {
            m_SideBarImage.sprite = m_Opened ? m_CloseSprite : m_OpenSprite;

            if (m_RectTransform == null)
            {
                m_RectTransform = transform as RectTransform;
            }
            if (m_ViewerViewport == null)
            {
                m_ViewerViewport = GetComponentInParent<PDFViewer>().m_Internal.m_Viewport;
            }
            if (m_HorizontalScrollBar == null)
            {
                m_HorizontalScrollBar = GetComponentInParent<PDFViewer>().m_Internal.m_HorizontalScrollBar;
            }

            if (m_Opened)
            {
                m_RectTransform.sizeDelta = new Vector2(m_LastPanelWidth, m_RectTransform.sizeDelta.y);
            }
            else
            {
                m_RectTransform.sizeDelta = new Vector2(24.0f, m_RectTransform.sizeDelta.y);
            }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (m_Opened)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
#endif
        }

        private void UpdateViewport()
        {
            if (m_ViewerViewport.offsetMin.x != m_RectTransform.sizeDelta.x)
            {
                m_ViewerViewport.offsetMin = new Vector2(m_RectTransform.sizeDelta.x, m_ViewerViewport.offsetMin.y);
                m_HorizontalScrollBar.offsetMin = new Vector2(m_RectTransform.sizeDelta.x,
                    m_HorizontalScrollBar.offsetMin.y);
            }
        }
    }
}