using UnityEngine;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// For internal use only.
    /// </summary>
    public class PDFAsset : ScriptableObject
    {
        [SerializeField]
        public byte[] m_FileContent;
        [SerializeField]
        public string m_Password;

        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }

        public void Load(byte[] fileContent)
        {
            m_FileContent = fileContent;
        }
    }
}