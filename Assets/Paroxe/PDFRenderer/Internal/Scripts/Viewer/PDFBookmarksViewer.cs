using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFBookmarksViewer : UIBehaviour
    {
        public RectTransform m_BooksmarksContainer;
        public PDFBookmarkListItem m_ItemPrefab;
        public Image m_LastHighlightedImage;
#if !UNITY_WEBGL
        private CanvasGroup m_CanvasGroup;
        private bool m_Initialized = false;
        private RectTransform m_LeftPanel;
        private bool m_Loaded = false;
        private PDFDocument m_Document;
        private PDFViewer m_PDFViewer;
        private RectTransform m_RectTransform;
        private List<RectTransform> m_TopLevelItems;
#endif
        private WeakDeviceReference m_WeakDeviceReference;

#if !UNITY_WEBGL
        private PDFBookmark m_RootBookmark;
#endif

        public void DoUpdate()
        {
#if !UNITY_WEBGL
            if (m_Initialized)
            {
                float containerHeight = 0.0f;
                foreach (RectTransform child in m_TopLevelItems)
                {
                    containerHeight += child.sizeDelta.y;
                }
            }

            if (m_RectTransform != null && m_LeftPanel != null &&
                m_RectTransform.sizeDelta.x != m_LeftPanel.sizeDelta.x - 24.0f)
            {
                m_RectTransform.sizeDelta = new Vector2(m_LeftPanel.sizeDelta.x - 24.0f, m_RectTransform.sizeDelta.y);
            }
#endif
        }

        private void Cleanup()
        {
#if !UNITY_WEBGL
            if (m_Loaded)
            {
                m_Loaded = false;
                m_Initialized = false;
                m_TopLevelItems = null;
                m_Document = null;
                m_WeakDeviceReference.Detach();
                m_WeakDeviceReference = null;
                m_RootBookmark = null;

                bool isNotFirst = false;
                foreach (Transform child in m_BooksmarksContainer.transform)
                {
                    if (isNotFirst)
                        Destroy(child.gameObject);
                    else
                        isNotFirst = true;
                }

                m_ItemPrefab.gameObject.SetActive(false);
                m_CanvasGroup.alpha = 0.0f;

                PDFLibrary.Instance.ForceGabageCollection();
            }
#endif
        }

        public void OnDocumentLoaded(PDFDocument document)
        {
#if !UNITY_WEBGL
            if (!m_Loaded && gameObject.activeInHierarchy)
            {
                m_Loaded = true;
                m_Document = document;

                m_TopLevelItems = new List<RectTransform>();

                m_RectTransform = transform as RectTransform;
                m_LeftPanel = transform.parent as RectTransform;

                PDFViewer viewer = GetComponentInParent<PDFViewer>();

                if (m_RootBookmark == null)
                {
                    if (m_WeakDeviceReference == null)
                        m_WeakDeviceReference = new WeakDeviceReference(viewer);

                    m_RootBookmark = m_Document.GetRootBookmark(viewer);
                }
                    
                if (m_RootBookmark != null)
                {
                    m_ItemPrefab.gameObject.SetActive(true);

                    foreach (PDFBookmark child in m_RootBookmark.EnumerateChildrenBookmarks())
                    {
                        PDFBookmarkListItem item = null;

                        item = Instantiate(m_ItemPrefab.gameObject).GetComponent<PDFBookmarkListItem>();
                        RectTransform itemTransform = item.transform as RectTransform;
                        itemTransform.SetParent(m_BooksmarksContainer, false);
                        itemTransform.localScale = Vector3.one;
                        itemTransform.anchorMin = new Vector2(0.0f, 1.0f);
                        itemTransform.anchorMax = new Vector2(0.0f, 1.0f);
                        itemTransform.offsetMin = Vector2.zero;
                        itemTransform.offsetMax = Vector2.zero;

                        m_TopLevelItems.Add(item.transform as RectTransform);

                        item.Initilize(child, 0, false);
                    }

                    m_ItemPrefab.gameObject.SetActive(false);

                    m_Initialized = true;

                    if (GetComponentInParent<PDFViewerLeftPanel>().m_Thumbnails.gameObject.GetComponent<CanvasGroup>().alpha == 0.0f)
                        StartCoroutine(DelayedShow());
                }
            }
#endif
        }

#if !UNITY_WEBGL
        IEnumerator DelayedShow()
        {
            yield return null;
            yield return null;
            yield return null;
            m_CanvasGroup.alpha = 1.0f;
        }
#endif

        public void OnDocumentUnloaded()
        {
#if !UNITY_WEBGL
            Cleanup();
#endif
        }

#if !UNITY_WEBGL
        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_Loaded)
            {
                Cleanup();
            }
        }
#endif

#if !UNITY_WEBGL
        protected override void OnEnable()
        {
            base.OnEnable();

            DoOnEnable();
        }
#endif

        public void DoOnEnable()
        {
#if !UNITY_WEBGL
            if (m_PDFViewer == null)
                m_PDFViewer = GetComponentInParent<PDFViewer>();
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_RectTransform == null)
                m_RectTransform = transform as RectTransform;

            m_ItemPrefab.gameObject.SetActive(false);
            m_CanvasGroup.alpha = 0.0f;

            if (!m_Loaded && m_PDFViewer.Document != null && m_PDFViewer.Document.IsValid)
                OnDocumentLoaded(m_PDFViewer.Document);
#endif
        }

        private class WeakDeviceReference : IPDFDevice
        {
            private IPDFDevice m_Device;

            public WeakDeviceReference(IPDFDevice device)
            {
                m_Device = device;
            }

            public void Detach()
            {
                m_Device = null;
            }

            bool IPDFDevice.AllowOpenURL
            {
                get { return m_Device.AllowOpenURL; }
                set { m_Device.AllowOpenURL = value; }
            }

            IPDFDeviceActionHandler IPDFDevice.BookmarksActionHandler
            {
                get { return m_Device.BookmarksActionHandler; }
                set { m_Device.BookmarksActionHandler = value; }
            }

            PDFDocument IPDFDevice.Document
            {
                get { return m_Device.Document; }
            }

            IPDFDeviceActionHandler IPDFDevice.LinksActionHandler
            {
                get { return m_Device.LinksActionHandler; }
                set { m_Device.LinksActionHandler = value; }
            }

            Vector2 IPDFDevice.GetDevicePageSize(int pageIndex)
            {
                return m_Device.GetDevicePageSize(pageIndex);
            }

            void IPDFDevice.GoToPage(int pageIndex)
            {
                m_Device.GoToPage(pageIndex);
            }

#if !UNITY_WEBGL
            void IPDFDevice.LoadDocument(PDFDocument document, int pageIndex)
            {
                m_Device.LoadDocument(document, pageIndex);
            }

            void IPDFDevice.LoadDocument(PDFDocument document, string password, int pageIndex)
            {
                m_Device.LoadDocument(document, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromAsset(PDFAsset pdfAsset, int pageIndex)
            {
                m_Device.LoadDocumentFromAsset(pdfAsset, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromAsset(PDFAsset pdfAsset, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromAsset(pdfAsset, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromBuffer(byte[] buffer, int pageIndex)
            {
                m_Device.LoadDocumentFromBuffer(buffer, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromBuffer(byte[] buffer, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromBuffer(buffer, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromFile(string filePath, int pageIndex)
            {
                m_Device.LoadDocumentFromFile(filePath, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromFile(string filePath, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromFile(filePath, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromPersistentData(string folder, string fileName, int pageIndex)
            {
                m_Device.LoadDocumentFromFile(folder, fileName, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromPersistentData(string folder, string fileName, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromPersistentData(folder, fileName, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromResources(string folder, string fileName, int pageIndex)
            {
                m_Device.LoadDocumentFromResources(folder, fileName, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromResources(string folder, string fileName, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromResources(folder, fileName, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromStreamingAssets(string folder, string fileName, int pageIndex)
            {
                m_Device.LoadDocumentFromStreamingAssets(folder, fileName, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromStreamingAssets(string folder, string fileName, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromStreamingAssets(folder, fileName, password, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromWeb(string url, int pageIndex)
            {
                m_Device.LoadDocumentFromWeb(url, pageIndex);
            }

            void IPDFDevice.LoadDocumentFromWeb(string url, string password, int pageIndex)
            {
                m_Device.LoadDocumentFromWeb(url, password, pageIndex);
            }
#endif
        }
    }
}