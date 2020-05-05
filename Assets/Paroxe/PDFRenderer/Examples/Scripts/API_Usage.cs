using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Paroxe.PdfRenderer.Examples
{
    public class API_Usage : MonoBehaviour
    {
#if !UNITY_WEBGL
        private IEnumerator Start()
        {
            // UnityWebRequest or WWW can be use instead of PDFWebRequest.
            using (PDFWebRequest www = new PDFWebRequest("https://www.dropbox.com/s/tssavtnvaym2t6b/DocumentationEN.pdf?raw=1"))
            {
                www.SendWebRequest();

                Debug.Log("Downloading document...");

                yield return www;

                if (www == null || !string.IsNullOrEmpty(www.error) || !www.isDone)
                    yield break;

                PDFDocument document = new PDFDocument(www.bytes, "");

                Debug.Log("Page count: " + document.GetPageCount());

                TextPageAPI(document);
                SearchAPI(document);
                BookmarkAPI(document);
            }
        }

        private void TextPageAPI(PDFDocument document)
        {
            Debug.Log("TEXTPAGE API-----------------------");

            PDFPage page = document.GetPage(1);
            Debug.Log("Page size: " + page.GetPageSize());

            PDFTextPage textPage = page.GetTextPage();
            int countChars = textPage.CountChars();
            Debug.Log("Page chars count: " + countChars);

            string str = textPage.GetText(0, countChars);
            Debug.Log("Page text: " + str);

            int rectCount = textPage.CountRects(0, countChars);
            Debug.Log("Rect count: " + rectCount);

            string boundedText = textPage.GetBoundedText(0, 0, page.GetPageSize().x, page.GetPageSize().y * 0.5f, 2000);
            Debug.Log("Bounded text: " + boundedText);
        }

        private void SearchAPI(PDFDocument document)
        {
            Debug.Log("SEARCH API-------------------------");

            IList<PDFSearchResult> results = document.GetPage(1).GetTextPage().Search("pdf");

            Debug.Log("Search results count: " + results.Count);
            Debug.Log("First result char index: " + results[0].StartIndex + " and chars count: " + results[0].Count);

            // Get all rects corresponding to the first search result
            int rectsCount = document.GetPage(1).GetTextPage().CountRects(results[0].StartIndex, results[0].Count);
            Debug.Log("Search result rects count: " + rectsCount);
        }

        private void BookmarkAPI(PDFDocument document)
        {
            Debug.Log("BOOKMARK API-----------------------");

            PDFBookmark rootBookmark = document.GetRootBookmark();
            OutputBookmarks(rootBookmark, 0);
        }

        private void OutputBookmarks(PDFBookmark bookmark, int indent)
        {
            string line = "";
            for (int i = 0; i < indent; ++i)
                line += "    ";
            line += bookmark.GetTitle();
            Debug.Log(line);

            foreach (PDFBookmark child in bookmark.EnumerateChildrenBookmarks())
            {
                OutputBookmarks(child, indent + 1);
            }
        }
#endif
    }
}
