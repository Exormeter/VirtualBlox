#if UNITY_2018_3_OR_NEWER
#define USE_UNITYWEBREQUEST
#endif

#if USE_UNITYWEBREQUEST
using UnityEngine.Networking;
#else
using UnityEngine;
#endif

using System;
using System.Collections;

namespace Paroxe.PdfRenderer
{
    /// <summary>
    /// WWW is deprecated in recent version of Unity but it's not available on older version like Unity 5.3
    /// This class only acts like a shim supporting the right implementation depending on which version of 
    /// Unity being use.
    /// </summary>
    public sealed class PDFWebRequest
#if USE_UNITYWEBREQUEST
        : UnityWebRequest, IEnumerator
#else
        : IDisposable, IEnumerator
#endif
    {
#if !USE_UNITYWEBREQUEST
        private WWW m_WWW;
#endif

        public PDFWebRequest(string url)
#if USE_UNITYWEBREQUEST
            : base(url)
#endif
        {
#if USE_UNITYWEBREQUEST
            downloadHandler = new DownloadHandlerBuffer();
            disposeDownloadHandlerOnDispose = true;
#else
            m_WWW = new WWW(url);
#endif
        }

#if USE_UNITYWEBREQUEST
        public float progress
        {
            get { return downloadProgress; }
        }

        public byte[] bytes
        {
            get { return downloadHandler.data; }
        }

        object IEnumerator.Current
        {
            get { return null; }
        }

        bool IEnumerator.MoveNext()
        {
            return !isDone;
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }
#else
        public float progress
        {
            get { return m_WWW.progress; }
        }

        public byte[] bytes
        {
            get { return m_WWW.bytes; }
        }

        public string error
        {
            get { return m_WWW.error; }
        }

        public bool isDone
        {
            get { return m_WWW.isDone; }
        }

        object IEnumerator.Current
        {
            get { return null; }
        }

        bool IEnumerator.MoveNext()
        {
            return !isDone;
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }
#endif

#if !USE_UNITYWEBREQUEST
        public void SendWebRequest() { }

        public void Dispose()
        {
            m_WWW.Dispose();
        }
#endif
    }
}
