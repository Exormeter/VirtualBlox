using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using Paroxe.PdfRenderer;

namespace Paroxe.PdfRenderer.WebGL
{
    public class PDFJS_Library : MonoBehaviour
    {
        private Hashtable m_PromiseCoroutineMap = new Hashtable();
        private bool m_IsInitialized;

        private static PDFJS_Library s_Instance;

        public static PDFJS_Library Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindObjectOfType<PDFJS_Library>();
                if (s_Instance == null)
                    s_Instance = new GameObject("WebGL_JSRuntime").AddComponent<PDFJS_Library>();
                return s_Instance;
            }
        }

        public PDFJS_PromiseCoroutine PreparePromiseCoroutine(
            Func<PDFJS_PromiseCoroutine, IPDFJS_Promise, object, IEnumerator> coroutine, IPDFJS_Promise promise, object parameters)
        {
            m_PromiseCoroutineMap[promise.PromiseHandle] = new PDFJS_PromiseCoroutine(this, promise, coroutine, parameters);
            return (PDFJS_PromiseCoroutine)m_PromiseCoroutineMap[promise.PromiseHandle];
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private string GetMessagePromiseHandle(string message)
        {
            string promiseHandle = message.Substring(message.IndexOf('{'));
            return promiseHandle.Substring(0, promiseHandle.IndexOf('}') + 1).Trim();
        }

        private string GetMessageObjectHandle(string message)
        {
            return message.Substring(message.IndexOf("objectHandle:")).Replace("objectHandle:", "").Trim();
        }

        private string GetMessageProgress(string message)
        {
            return message.Substring(message.IndexOf("progress:")).Replace("progress:", "").Trim();

        }

        private void OnPromiseProgress(string message)
        {
            string promiseHandle = GetMessagePromiseHandle(message);
            string progress = GetMessageProgress(message);

            if (m_PromiseCoroutineMap.Contains(promiseHandle))
            {
                PDFJS_PromiseCoroutine promiseCoroutine = (PDFJS_PromiseCoroutine)m_PromiseCoroutineMap[promiseHandle];

                promiseCoroutine.Promise.Progress = float.Parse(progress);
            }
        }

        private void OnPromiseThen(string message)
        {
            string promiseHandle = GetMessagePromiseHandle(message);
            string objectHandle = GetMessageObjectHandle(message);

            if (m_PromiseCoroutineMap.Contains(promiseHandle))
            {
                PDFJS_PromiseCoroutine promiseCoroutine = (PDFJS_PromiseCoroutine)m_PromiseCoroutineMap[promiseHandle];

                promiseCoroutine.Promise.JSObjectHandle = objectHandle;
                promiseCoroutine.Promise.HasSucceeded = true;
                promiseCoroutine.Promise.HasReceivedJSResponse = true;

                m_PromiseCoroutineMap.Remove(promiseHandle);
            }
        }

        private void OnPromiseCatch(string message)
        {
            string promiseHandle = GetMessagePromiseHandle(message);
            string objectHandle = GetMessageObjectHandle(message);

            if (m_PromiseCoroutineMap.Contains(promiseHandle))
            {
                PDFJS_PromiseCoroutine promiseCoroutine = (PDFJS_PromiseCoroutine)m_PromiseCoroutineMap[promiseHandle];

                promiseCoroutine.Promise.JSObjectHandle = objectHandle;
                promiseCoroutine.Promise.HasSucceeded = false;
                promiseCoroutine.Promise.HasReceivedJSResponse = true;

                m_PromiseCoroutineMap.Remove(promiseHandle);
            }
        }

        private void OnPromiseCancel(string message)
        {
            string promiseHandle = GetMessagePromiseHandle(message);
            string objectHandle = GetMessageObjectHandle(message);

            if (m_PromiseCoroutineMap.Contains(promiseHandle))
            {
                PDFJS_PromiseCoroutine promiseCoroutine = (PDFJS_PromiseCoroutine)m_PromiseCoroutineMap[promiseHandle];

                promiseCoroutine.Promise.JSObjectHandle = objectHandle;
                promiseCoroutine.Promise.HasBeenCancelled = true;
                promiseCoroutine.Promise.HasSucceeded = false;
                promiseCoroutine.Promise.HasReceivedJSResponse = true;

                m_PromiseCoroutineMap.Remove(promiseHandle);
            }
        }

        public void OnLibraryInitialized(string message)
        {
            PDFLibrary.Instance.IsInitialized = true;
        }

        public void TryTerminateRenderingWorker(string promiseHandle)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PDFJS_TryTerminateRenderWorker(promiseHandle);
#endif
        }


#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(PDFLibrary.PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_TryTerminateRenderWorker(string promiseHandle);
#endif
    }
}