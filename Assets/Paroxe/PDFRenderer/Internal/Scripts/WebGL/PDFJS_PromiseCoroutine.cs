using UnityEngine;
using System.Collections;
using System;

namespace Paroxe.PdfRenderer.WebGL
{
    public class PDFJS_PromiseCoroutine
    {
        private Action<bool, object> m_Action;
        private MonoBehaviour m_MonoBehaviour;
        private Func<PDFJS_PromiseCoroutine, IPDFJS_Promise, object, IEnumerator> m_Coroutine;
        private object m_Parameters;
        private IPDFJS_Promise m_Promise;
        private float m_Progress;

        public IPDFJS_Promise Promise
        {
            get { return m_Promise; }
        }

        public object Parameters
        {
            get { return m_Parameters; }
            set { m_Parameters = value; }
        }

        public float Progress
        {
            get { return m_Progress; }
            set { m_Progress = value; }
        }

        public PDFJS_PromiseCoroutine(MonoBehaviour monoBehaviour, IPDFJS_Promise promise, Func<PDFJS_PromiseCoroutine, IPDFJS_Promise, object, IEnumerator> coroutine, object parameters)
        {
            m_MonoBehaviour = monoBehaviour;
            m_Coroutine = coroutine;
            m_Parameters = parameters;

            m_Promise = promise;
        }

        public PDFJS_PromiseCoroutine SetThenAction(Action<bool, object> action)
        {
            m_Action = action;
            return this;
        }

        public void ExecuteThenAction(bool success, object result)
        {
            if (m_Action != null)
                m_Action.Invoke(success, result);
        }

        public void Start()
        {
            m_MonoBehaviour.StartCoroutine(m_Coroutine(this, m_Promise, m_Parameters));
        }
    }
}