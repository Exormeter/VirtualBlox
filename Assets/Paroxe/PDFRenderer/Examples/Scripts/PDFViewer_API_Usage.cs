using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Paroxe.PdfRenderer.Examples
{
    public class PDFViewer_API_Usage : MonoBehaviour
    {
        public PDFViewer m_Viewer;
        public PDFAsset m_PDFAsset;

        IEnumerator Start()
        {
#if UNITY_WEBGL
            yield break;
#else
            Debug.Log(Application.persistentDataPath);

            m_Viewer.gameObject.SetActive(true);

            m_Viewer.LoadDocumentFromWeb("http://www.pdf995.com/samples/pdf.pdf", "");

            // Wait until the pdf document is loaded.
            while (!m_Viewer.IsLoaded)
                yield return null;

            PDFDocument document = m_Viewer.Document;
            Debug.Log("Page count: " + document.GetPageCount());

            PDFPage firstPage = document.GetPage(0);
            Debug.Log("First Page Size: " + firstPage.GetPageSize());

            PDFTextPage firstTextPage = firstPage.GetTextPage();
            Debug.Log("First Page Chars Count: " + firstTextPage.CountChars());

            IList<PDFSearchResult> searchResults = firstTextPage.Search("the", PDFSearchHandle.MatchOption.NONE, 0);
            Debug.Log("Search Results Count: " + searchResults.Count);

            // Wait 2 seconds and then load another document
            yield return new WaitForSeconds(2.0f);

            m_Viewer.LoadDocumentFromAsset(m_PDFAsset);
#endif

        }
    }
}

