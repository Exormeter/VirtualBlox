using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.PdfRenderer.Examples
{
    public class ShowPersistentData : UIBehaviour
    {
        [SerializeField]
        private Text m_Notice = null;

        protected override void Start()
        {
            base.Start();

            m_Notice.text += " " + Application.persistentDataPath;
        }
    }
}

