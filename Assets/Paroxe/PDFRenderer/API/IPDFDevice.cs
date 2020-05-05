using UnityEngine;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Interface for device implementation. PDFViewer implements it. It allows to
    /// decouple PDFViewer from the API
    /// </summary>
    public interface IPDFDevice
    {
        PDFDocument Document { get; }

        Vector2 GetDevicePageSize(int pageIndex);
        void GoToPage(int pageIndex);

        IPDFDeviceActionHandler BookmarksActionHandler { get; set; }
        IPDFDeviceActionHandler LinksActionHandler { get; set; }

        bool AllowOpenURL { get; set; }

#if !UNITY_WEBGL
        void LoadDocument(PDFDocument document, int pageIndex);
        void LoadDocument(PDFDocument document, string password, int pageIndex);

        void LoadDocumentFromAsset(PDFAsset pdfAsset, int pageIndex);
        void LoadDocumentFromAsset(PDFAsset pdfAsset, string password, int pageIndex);

        void LoadDocumentFromResources(string folder, string fileName, int pageIndex);
        void LoadDocumentFromResources(string folder, string fileName, string password, int pageIndex);

        void LoadDocumentFromStreamingAssets(string folder, string fileName, int pageIndex);
        void LoadDocumentFromStreamingAssets(string folder, string fileName, string password, int pageIndex);

        void LoadDocumentFromPersistentData(string folder, string fileName, int pageIndex);
        void LoadDocumentFromPersistentData(string folder, string fileName, string password, int pageIndex);

        void LoadDocumentFromWeb(string url, int pageIndex);
        void LoadDocumentFromWeb(string url, string password, int pageIndex);

        void LoadDocumentFromBuffer(byte[] buffer, int pageIndex);
        void LoadDocumentFromBuffer(byte[] buffer, string password, int pageIndex);

        void LoadDocumentFromFile(string filePath, int pageIndex);
        void LoadDocumentFromFile(string filePath, string password, int pageIndex);
#endif
    }
}
