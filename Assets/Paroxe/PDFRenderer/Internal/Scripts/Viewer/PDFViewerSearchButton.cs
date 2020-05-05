using UnityEngine.EventSystems;

namespace Paroxe.PdfRenderer.Internal.Viewer
{
    public class PDFViewerSearchButton : UIBehaviour
    {
        public void OnClick()
        {
#if !UNITY_WEBGL
            GetComponentInParent<PDFViewer>().m_Internal.m_SearchPanel.GetComponent<PDFSearchPanel>().Toggle();
#endif
        }
    }
}