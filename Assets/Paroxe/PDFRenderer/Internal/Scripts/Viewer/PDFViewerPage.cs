using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
#if UNITY_WEBGL
    public class PDFViewerPage : UIBehaviour
    {
        public Texture2D m_HandCursor;
    }
#endif

#if !UNITY_WEBGL
    public class PDFViewerPage : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        , IPointerEnterHandler, IPointerExitHandler
#endif
    {
        public Texture2D m_HandCursor;

        private PDFViewer m_PDFViewer;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private bool m_PointerInside = false;
        private bool m_HandCursorSettedByMe = false;
#endif
        private Camera m_CanvasCamera;
        public PDFPage m_Page;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_PDFViewer == null)
                m_PDFViewer = GetComponentInParent<PDFViewer>();
            if (m_PDFViewer.Document == null)
                return;
            if (m_PDFViewer.LinksActionHandler == null)
                return;
            if (m_Page == null)
                m_PDFViewer.Document.GetPage(transform.GetSiblingIndex());

            PDFLink link = GetLinkAtPoint(eventData.pressPosition, eventData.pressEventCamera);

            if (link != null)
                PDFActionHandlerHelper.ExecuteLinkAction(m_PDFViewer, link);
            else if (m_PDFViewer.ParagraphZoomingEnable && eventData.clickCount == 2)
            {
                using (PDFTextPage textPage = m_Page.GetTextPage())
                {
                    Rect pageRect = new Rect();

                    Vector2 pos = eventData.pressPosition;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, pos, GetComponent<Camera>(), out pos);
                    RectTransform rt = transform as RectTransform;
                    pos += rt.sizeDelta.x * 0.5f * Vector2.right;
                    pos += rt.sizeDelta.y * 0.5f * Vector2.up;
                    pos = pos.x * (rt.sizeDelta.y / rt.sizeDelta.x) * Vector2.right + pos.y * Vector2.up;

                    Vector2 pagePoint = m_Page.DeviceToPage(0, 0, (int)rt.sizeDelta.y, (int)rt.sizeDelta.y, PDFPage.PageRotation.Normal, (int)pos.x, (int)pos.y);
                    Vector2 pageSize = m_Page.GetPageSize();

                    float threshold = m_PDFViewer.ParagraphDetectionThreshold;

                    string text = GetBoundedText(textPage, 0.0f, pagePoint.y + 0.0f, pageSize.x, pagePoint.y - 1.0f);

                    if (!string.IsNullOrEmpty(text.Trim()) && text.Trim().Length > 4)
                    {
                        string prevText = text;

                        float bottomOffset = 0.0f;
                        float topOffset = 0.0f;
                        float t = 0.0f;

                        while (true)
                        {
                            bottomOffset += 2.0f;
                            text = GetBoundedText(textPage, 0.0f, pagePoint.y + 0.0f, pageSize.x, pagePoint.y - bottomOffset);

                            if (text == prevText)
                                t += 2.0f;
                            else
                                t = 0.0f;
                            if (t >= threshold)
                                break;

                            prevText = text;
                        }

                        t = 0.0f;
                        while (true)
                        {
                            topOffset += 2.0f;
                            text = GetBoundedText(textPage, 0.0f, pagePoint.y + topOffset, pageSize.x, pagePoint.y - bottomOffset);

                            if (text == prevText)
                                t += 2.0f;
                            else
                                t = 0.0f;
                            if (t >= threshold)
                                break;

                            prevText = text;
                        }

                        pageRect = new Rect(0.0f, pagePoint.y + topOffset, pageSize.x, (pagePoint.y + topOffset) - (pagePoint.y - bottomOffset));

                        m_PDFViewer.ZoomOnParagraph(this, pageRect);
                    }
                }
            }
        }

        private string GetBoundedText(PDFTextPage textPage, float left, float top, float right, float bottom)
        {
            return textPage.GetBoundedText(left, top, right, bottom, 4096);
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_PDFViewer == null)
            {
                m_PDFViewer = GetComponentInParent<PDFViewer>();
            }

            if (m_PDFViewer.Document == null)
            {
                return;
            }

            m_PointerInside = true;

            if (m_Page == null)
                m_PDFViewer.Document.GetPage(transform.GetSiblingIndex());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_PointerInside = false;

            if (m_HandCursorSettedByMe)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }

            if (m_Page != null)
                m_Page.Dispose();
            m_Page = null;
        }
#endif

        protected override void OnEnable()
        {
            if (m_PDFViewer == null)
                m_PDFViewer = GetComponentInParent<PDFViewer>();
            if (m_CanvasCamera == null)
                m_CanvasCamera = FindCanvasCamera(transform as RectTransform);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            m_PointerInside = false;
#endif
        }

        protected override void OnDisable()
        {
            if (m_Page != null)
                m_Page.Dispose();
            m_Page = null;
        }

        private Camera FindCanvasCamera(RectTransform rt)
        {
            RectTransform parent = rt.parent as RectTransform;
            if (parent != null)
            {
                Canvas canvas = parent.GetComponent<Canvas>();
                if (canvas != null && canvas.worldCamera != null)
                {
                    return canvas.worldCamera;
                }

                return FindCanvasCamera(parent);
            }
            return null;
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private void Update()
        {
            if (m_PDFViewer == null)
                m_PDFViewer = GetComponentInParent<PDFViewer>();

            if (m_HandCursor == null || !m_PDFViewer.ChangeCursorWhenOverURL)
                return;

            if (m_CanvasCamera == null)
                m_CanvasCamera = FindCanvasCamera(transform as RectTransform);

            if (m_PointerInside)
            {
                PDFLink link = GetLinkAtPoint(Input.mousePosition, m_CanvasCamera);

                if (link != null)
                {
                    Cursor.SetCursor(m_HandCursor, new Vector2(6.0f, 0.0f), CursorMode.Auto);
                    m_HandCursorSettedByMe = true;
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }
#endif

        public void OnPointerDown(PointerEventData eventData) { }

        public void OnPointerUp(PointerEventData eventData) { }

        private PDFLink GetLinkAtPoint(Vector2 point, Camera camera)
        {
            if (m_PDFViewer == null)
                m_PDFViewer = GetComponentInParent<PDFViewer>();

            if (m_Page == null)
                m_Page = m_PDFViewer.Document.GetPage(transform.GetSiblingIndex());

            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, point, camera,
                out localPointerPosition))
            {
                RectTransform rt = transform as RectTransform;

                localPointerPosition += rt.sizeDelta.x * 0.5f * Vector2.right;
                localPointerPosition += rt.sizeDelta.y * 0.5f * Vector2.up;

                localPointerPosition = localPointerPosition.x * (rt.sizeDelta.y / rt.sizeDelta.x) * Vector2.right +
                                       localPointerPosition.y * Vector2.up;

                Vector2 pagePoint = m_Page.DeviceToPage(0, 0, (int)rt.sizeDelta.y, (int)rt.sizeDelta.y,
                    PDFPage.PageRotation.Normal,
                    (int)localPointerPosition.x, (int)localPointerPosition.y);


                return m_Page.GetLinkAtPoint(pagePoint);
            }

            return null;
        }
    }
#endif
}