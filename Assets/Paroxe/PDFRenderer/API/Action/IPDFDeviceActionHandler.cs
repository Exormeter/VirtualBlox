
namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// Implement this class to provide a custom action handling stategy. For example, PDFViewer.BookmarksActionHandler 
    /// and PDFViewer.LinksActionHandler both refer to a default implementation of this interface.
    /// </summary>
    public interface IPDFDeviceActionHandler
    {
        /// <summary>
        /// Called when a goto action is triggered.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pageIndex"></param>
        void HandleGotoAction(IPDFDevice device, int pageIndex);

        /// <summary>
        /// Called when a launch action is triggered.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filePath"></param>
        void HandleLaunchAction(IPDFDevice device, string filePath);

        /// <summary>
        /// Implement the function if you want to provide password. This method received the resolved path
        /// returned by the previous method (HandleRemoteGotoActionPathResolving)
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resolvedFilePath"></param>
        /// <returns></returns>
        string HandleRemoteGotoActionPasswordResolving(IPDFDevice device, string resolvedFilePath);

        /// <summary>
        /// Implement the function if you want custom path resolving before PDF Viewer open other pdf file. 
        /// The method must return the modified filePath or just return the original filePath. 
        /// See PDFViewerDefaultActionHandler class for the default implementation.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string HandleRemoteGotoActionPathResolving(IPDFDevice device, string filePath);

        /// <summary>
        /// This method is called after the new pdf document is loaded but not yet opened in the pdfViewer.
        /// See PDFViewerDefaultActionHandler class for the default implementation.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="document"></param>
        /// <param name="pageIndex"></param>
        void HandleRemoteGotoActionResolved(IPDFDevice device, PDFDocument document, int pageIndex);

        /// <summary>
        /// This method is called when the pdf pdf file at filePath doesn't exists or is invalid.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="resolvedFilePath"></param>
        void HandleRemoteGotoActionUnresolved(IPDFDevice device, string resolvedFilePath);

        /// <summary>
        /// Called when the action is unsuported
        /// </summary>
        /// <param name="device"></param>
        void HandleUnsuportedAction(IPDFDevice device);

        /// <summary>
        /// Called for action that point to an Uri (Universal Resource Identifier)
        /// </summary>
        /// <param name="device"></param>
        /// <param name="uri"></param>
        void HandleUriAction(IPDFDevice device, string uri);
    }
}