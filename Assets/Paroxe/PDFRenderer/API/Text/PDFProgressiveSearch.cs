using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.PdfRenderer
{
#if !UNITY_WEBGL
    /// <summary>
    /// Don't instantiate this class directly. Use the static method instead PDFProgressiveSearch.CreateSearch
    /// </summary>
    public class PDFProgressiveSearch : UIBehaviour
    {
        private bool m_AbortRequested;
        private int m_CurrentPage;
        private PDFDocument m_Document;
        private PDFSearchHandle.MatchOption m_Flags;
        private bool m_NewSearchRequested;
        private int m_PageCount;
        private byte[] m_Search;
        private IList<PDFSearchResult>[] m_SearchResults;
        private bool m_SearchStarted;
        private float m_TimeBudget = 0.50f;
        private int m_Total;

        public delegate void ProgressEventHandler(PDFProgressiveSearch sender, int total);

        public delegate void SearchFinishedEventHandler(PDFProgressiveSearch sender, IList<PDFSearchResult>[] searchResults);

        public event ProgressEventHandler OnProgressChanged;

        public event SearchFinishedEventHandler OnSearchFinished;

        /// <summary>
        /// Create a progressive search object within the scene.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="timeBudget">Time budget per frame. The value must be metween 0.0f and 1.0f</param>
        /// <returns></returns>
        public static PDFProgressiveSearch CreateSearch(PDFDocument document, float timeBudget)
        {
            GameObject searchObject = new GameObject();
            searchObject.name = "PDFProgressiveSearch";

            PDFProgressiveSearch progressiveSearch = searchObject.AddComponent<PDFProgressiveSearch>();
            progressiveSearch.m_Document = document;
            progressiveSearch.m_TimeBudget = Mathf.Clamp01(timeBudget);

            return progressiveSearch;
        }

        /// <summary>
        /// Stop the current search request.
        /// </summary>
        public void Abort()
        {
            m_AbortRequested = true;
        }

        /// <summary>
        /// Start a new search and if a search is already started this method will stop it.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="flags">PDFSearchHandle.MATCH_CASE, (PDFSearchHandle.MATCH_WHOLE_WORD or PDFSearchHandle.MATCH_CASE | PDFSearchHandle.MATCH_WHOLE_WORD)</param>
        public void StartSearch(string search, PDFSearchHandle.MatchOption flags)
        {
            if (string.IsNullOrEmpty(search.Trim()))
                m_Search = null;
            else
                m_Search = Encoding.Unicode.GetBytes(search.Trim() + "\0");
            m_Flags = flags;

            m_PageCount = m_Document.GetPageCount();

            m_NewSearchRequested = true;
        }

        private void LateUpdate()
        {
            if (!m_SearchStarted && m_NewSearchRequested)
            {
                m_CurrentPage = 0;
                m_SearchResults = new IList<PDFSearchResult>[m_PageCount];

                m_NewSearchRequested = false;
                m_SearchStarted = true;

                m_AbortRequested = false;

                m_Total = 0;
            }

            if (m_SearchStarted)
            {
                if (m_AbortRequested)
                {
                    m_SearchStarted = false;
                    m_NewSearchRequested = false;

                    m_AbortRequested = false;

                    m_Total = 0;
                    return;
                }
                if (m_NewSearchRequested)
                {
                    m_CurrentPage = 0;
                    m_SearchResults = new IList<PDFSearchResult>[m_PageCount];

                    m_NewSearchRequested = false;

                    m_Total = 0;
                }

                if (m_Search == null || m_Search.Length == 0)
                {
                    m_SearchStarted = false;

                    if (OnProgressChanged != null)
                    {
                        OnProgressChanged(this, 0);
                    }

                    var handler = OnSearchFinished;
                    if (handler != null)
                    {
                        handler(this, null);
                    }
                }
                else
                {
                    Stopwatch timer = Stopwatch.StartNew();

                    for (int i = m_CurrentPage; i < m_PageCount; ++i)
                    {
                        using (PDFTextPage textPage = m_Document.GetPage(i).GetTextPage())
                        {
                            IList<PDFSearchResult> searchResults = textPage.Search(m_Search, m_Flags);

                            m_SearchResults[i] = searchResults;

                            m_Total += searchResults.Count;

                            ++m_CurrentPage;

                            if (timer.ElapsedMilliseconds >=
                                m_TimeBudget * 1000.0f * (1.0f / Mathf.Max(Application.targetFrameRate, 1.0f / Time.deltaTime)))
                            {
                                if (OnProgressChanged != null)
                                {
                                    OnProgressChanged(this, m_Total);
                                }

                                break;
                            }
                        }
                    }

                    if (m_CurrentPage + 1 > m_PageCount)
                    {
                        m_SearchStarted = false;

                        if (OnProgressChanged != null)
                        {
                            OnProgressChanged(this, m_Total);
                        }

                        var handler = OnSearchFinished;
                        if (handler != null)
                        {
                            handler(this, m_SearchResults);
                        }

                        if (OnProgressChanged != null)
                        {
                            OnProgressChanged(this, m_Total);
                        }
                    }
                }
            }
        }
    }
#endif
}