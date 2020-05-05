using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFBookmarkListItem : UIBehaviour
    {
        public Sprite m_CollapseSprite;
        public Image m_ExpandImage;
        public Sprite m_ExpandSprite;
        public Image m_Highlighted;
        public RectTransform m_HorizontalLine;
        public RectTransform m_Internal;
        public RectTransform m_NextSibling;
        public Text m_Title;
        public RectTransform m_VerticalLine;
        public RectTransform m_VerticalLine2;

#if !UNITY_WEBGL
        private CanvasGroup m_CanvasGroup;
        private List<PDFBookmarkListItem> m_ChildrenItems;
        private bool m_Expanded;
        private bool m_Initialized;
        private bool m_IsLastSibling = false;
        private float m_LastClickTimestamp = 0.0f;
        private LayoutElement m_LayoutElement;
        private PDFBookmark m_PDFBookmark;
        private RectTransform m_RectTransform;
        private int m_SizeAdjusted = 0;
        private PDFBookmarksViewer m_BooksmarkViewer;

        public void Initilize(PDFBookmark bookmark, int indent, bool lastSibling)
        {
            m_ChildrenItems = new List<PDFBookmarkListItem>();

            m_IsLastSibling = lastSibling;
            m_PDFBookmark = bookmark;
            m_HorizontalLine.gameObject.SetActive(true);
            m_VerticalLine.gameObject.SetActive(false);
            m_VerticalLine2.gameObject.SetActive(true);
            m_ExpandImage.gameObject.SetActive(true);

            if (m_PDFBookmark.IsTopLevelBookmark && m_PDFBookmark.ChildCount == 0)
            {
                m_HorizontalLine.gameObject.SetActive(false);
                m_ExpandImage.gameObject.SetActive(false);
            }
            else if (m_PDFBookmark.ChildCount > 0)
            {
                m_HorizontalLine.gameObject.SetActive(true);
                m_ExpandImage.gameObject.SetActive(true);
            }
            else if (!m_PDFBookmark.IsTopLevelBookmark)
            {
                m_HorizontalLine.gameObject.SetActive(true);
                m_HorizontalLine.offsetMin = new Vector2(m_HorizontalLine.offsetMin.x + 6, m_HorizontalLine.offsetMin.y);
                if (!m_IsLastSibling)
                {
                    m_VerticalLine2.gameObject.SetActive(true);

                }
                m_ExpandImage.gameObject.SetActive(false);
            }

            m_Title.text = m_PDFBookmark.GetTitle();

            name = m_Title.text.Substring(0, Mathf.Min(24, m_Title.text.Length));

            for (int i = 0; i < m_PDFBookmark.ChildCount; ++i)
            {
                PDFBookmark child = m_PDFBookmark.GetChild(i);

                PDFBookmarkListItem item = Instantiate(gameObject).GetComponent<PDFBookmarkListItem>();
                m_ChildrenItems.Add(item);

                RectTransform itemTransform = item.transform as RectTransform;
                itemTransform.SetParent(transform.parent, false);
                itemTransform.localScale = Vector3.one;
                itemTransform.anchorMin = new Vector2(0.0f, 1.0f);
                itemTransform.anchorMax = new Vector2(0.0f, 1.0f);
                itemTransform.offsetMin = Vector2.zero;
                itemTransform.offsetMax = Vector2.zero;

                item.Initilize(child, indent + 1, i == m_PDFBookmark.ChildCount - 1);

                if (indent == 0)
                {
                    StartCoroutine(SetVisible());
                    item.m_CanvasGroup.alpha = 0.0f;
                }
            }

            for (int i = 0; i < m_ChildrenItems.Count - 1; ++i)
            {
                m_ChildrenItems[i].m_NextSibling = m_ChildrenItems[i + 1].gameObject.transform as RectTransform;
            }

            m_Internal.offsetMin = new Vector2(20.0f * indent, m_Internal.offsetMin.y);
            m_Initialized = true;
            m_SizeAdjusted = 1;
        }

        public void OnExpandButton()
        {
            if (m_PDFBookmark.ChildCount > 0)
            {
                m_Expanded = !m_Expanded;

                m_VerticalLine.gameObject.SetActive(m_Expanded);

                m_ExpandImage.sprite = m_Expanded ? m_CollapseSprite : m_ExpandSprite;

                foreach (PDFBookmarkListItem child in m_ChildrenItems)
                {
                    child.SetState(m_Expanded);
                }
            }
        }

        public void OnItemClicked()
        {
            if (Time.fixedTime - m_LastClickTimestamp < 0.5f)
            {
                OnExpandButton();
            }
            m_LastClickTimestamp = Time.fixedTime;

            if (m_BooksmarkViewer == null)
                m_BooksmarkViewer = GetComponentInParent<PDFBookmarksViewer>();

            if (m_BooksmarkViewer.m_LastHighlightedImage != null)
            {
                m_BooksmarkViewer.m_LastHighlightedImage.gameObject.SetActive(false);
            }
            m_Highlighted.gameObject.SetActive(true);
            m_BooksmarkViewer.m_LastHighlightedImage = m_Highlighted;

            m_PDFBookmark.ExecuteBookmarkAction();
        }

        public void SetState(bool active)
        {
            gameObject.SetActive(active);
            if (active)
            {
                StartCoroutine(SetVisible());
            }

            if (m_ChildrenItems.Count > 0)
            {
                m_VerticalLine.gameObject.SetActive(!active);
                m_ExpandImage.sprite = !active ? m_CollapseSprite : m_ExpandSprite;
            }

            if (!active)
            {
                foreach (PDFBookmarkListItem child in m_ChildrenItems)
                {
                    child.SetState(active);
                }
            }
        }

        protected override void OnEnable()
        {
            m_RectTransform = transform as RectTransform;
            m_LayoutElement = GetComponent<LayoutElement>();

            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                m_CanvasGroup.interactable = true;
                m_CanvasGroup.blocksRaycasts = true;
            }

            m_CanvasGroup.alpha = 1.0f;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_PDFBookmark = null;
            m_ChildrenItems = null;

            if (m_BooksmarkViewer != null)
            {
                if (m_BooksmarkViewer.m_LastHighlightedImage == this)
                    m_BooksmarkViewer.m_LastHighlightedImage = null;

                m_BooksmarkViewer = null;
            }
        }

        private IEnumerator SetVisible()
        {
            m_CanvasGroup.alpha = 0.0f;

            yield return new WaitForSeconds(1.0f / 60.0f);

            m_CanvasGroup.alpha = 1.0f;
        }

        void Update()
        {
            if (m_Initialized && m_SizeAdjusted == 0)
            {
                m_LayoutElement.preferredHeight = m_Title.preferredHeight + 20.0f;

                if (m_VerticalLine.gameObject.activeInHierarchy)
                {
                    m_VerticalLine.sizeDelta = new Vector2(1.0f, m_LayoutElement.preferredHeight);
                }

                if (!m_PDFBookmark.IsTopLevelBookmark)
                {
                    gameObject.SetActive(false);
                }

                m_SizeAdjusted = -1;
            }
            else if (m_SizeAdjusted > 0)
            {
                m_SizeAdjusted--;
            }

            if (m_Initialized && m_SizeAdjusted <= -1)
            {
                m_LayoutElement.preferredHeight = m_Title.preferredHeight + 20.0f;

                if (m_VerticalLine.gameObject.activeInHierarchy)
                {
                    m_VerticalLine.sizeDelta = new Vector2(1.0f, m_LayoutElement.preferredHeight);
                }

                if (m_NextSibling != null)
                {
                    if (m_VerticalLine2.gameObject.activeInHierarchy)
                    {
                        float newHeight =
                            Mathf.Abs((m_NextSibling.anchoredPosition - m_RectTransform.anchoredPosition).y);
                        m_VerticalLine2.sizeDelta = new Vector2(1.0f, newHeight);
                    }
                }
                else if (m_VerticalLine2.gameObject.activeInHierarchy)
                {
                    m_VerticalLine2.gameObject.SetActive(false);
                }

                if (m_SizeAdjusted == -1)
                {
                    m_SizeAdjusted = -2;
                }
            }
        }
#endif
    }
}