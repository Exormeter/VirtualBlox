using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Paroxe.PdfRenderer
{
    public class PDFLibrary : IDisposable
    {
#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        public const string PLUGIN_ASSEMBLY = "__Internal";
#else
        public const string PLUGIN_ASSEMBLY = "pdfrenderer";
#endif

        public static object nativeLock = new object();

        public enum ErrorCode
        {
            ErrSuccess = 0, // No error.
            ErrUnknown = 1, // Unknown error.
            ErrFile = 2, // File not found or could not be opened.
            ErrFormat = 3, // File not in PDF format or corrupted.
            ErrPassword = 4, // Password required or incorrect password.
            ErrSecurity = 5, // Unsupported security scheme.
            ErrPage = 6 // Page not found or content error.
        }

        private static bool m_Disposed;
        private static PDFLibrary m_Instance;
        private static int m_RefCount;
        private
#if UNITY_WEBGL && !UNITY_EDITOR
		static
#endif
        bool m_IsInitialized;
        private static bool m_AlreadyDestroyed;

#if UNITY_WEBGL || !UNITY_EDITOR
        private static bool m_AlreadyInitialized;
#endif

        private PDFLibrary()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!m_AlreadyInitialized)
            {
                PDFJS_InitLibrary();
                m_AlreadyInitialized = true;
            }
#else

#if !UNITY_EDITOR
			if (!m_AlreadyInitialized)
#endif
            {
                m_IsInitialized = true;
                m_AlreadyDestroyed = false;

#if !UNITY_EDITOR && (UNITY_ANDROID)
                if (Marshal.SizeOf(typeof(IntPtr)) == 4)
                {
                    FPDF_InitLibrary(
                        "These binary file is intended to be used only by PDFRenderer plugin for Unity3D. http://paroxe.com/products/pdf-renderer/");
                }
                else
                {
                    FPDF_InitLibrary();
                }
#else
                FPDF_InitLibrary(
                    "These binary file is intended to be used only by PDFRenderer plugin for Unity3D. http://paroxe.com/products/pdf-renderer/");
#endif

#if !UNITY_EDITOR
				m_AlreadyInitialized = true;
#endif
            }

#endif
        }

        ~PDFLibrary()
        {
            Dispose(false);
        }

        /// <summary>
        /// Return the last error code.
        /// </summary>
        /// <returns></returns>
        public static ErrorCode GetLastError()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return ErrorCode.ErrSuccess;
#else
            Instance.EnsureInitialized();
            return (ErrorCode)FPDF_GetLastError();
#endif
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (nativeLock)
            {
                if (!m_Disposed)
                {
                    if (m_RefCount == 0)
                    {
#if PDFRENDERER_DEBUG
                        PrintInstanceMap();
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
                        if (!m_AlreadyDestroyed)
                        {
                            m_AlreadyDestroyed = true;
#if UNITY_EDITOR
                            FPDF_DestroyLibrary();
#endif
                        }
#endif

                        m_Disposed = true;
                        m_Instance = null;
                    }

                    m_Disposed = true;
                }
            }
        }

#if PDFRENDERER_DEBUG
        private static Dictionary<string, int> s_InstanceMap = new Dictionary<string, int>();

        private static void PrintInstanceMap()
        {
            foreach (string key in s_InstanceMap.Keys)
            {
                Debug.Log(key + " Count: " + s_InstanceMap[key]);
            }
        }
#endif

        internal static void AddRef(string token)
        {
            lock (nativeLock)
            {
#if PDFRENDERER_DEBUG
                if (s_InstanceMap.ContainsKey(token))
                    s_InstanceMap[token] = s_InstanceMap[token] + 1;
                else
                    s_InstanceMap.Add(token, 1);
#endif

                ++m_RefCount;
                Instance.EnsureInitialized();
            }
        }

        internal static void RemoveRef(string token)
        {
            lock (nativeLock)
            {
                --m_RefCount;

#if PDFRENDERER_DEBUG
                s_InstanceMap[token] = s_InstanceMap[token] - 1;
#endif
                if (m_RefCount == 0)
                {
                    if (m_Disposed)
                    {
#if PDFRENDERER_DEBUG
                        PrintInstanceMap();
#endif
#if !UNITY_WEBGL || UNITY_EDITOR
                        if (!m_AlreadyDestroyed)
                        {
                            m_AlreadyDestroyed = true;
                            FPDF_DestroyLibrary();
                        }
#endif

                        m_Instance = null;
                    }
                    else
                        m_Instance.Dispose();
                }
            }
        }

        public static PDFLibrary Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new PDFLibrary();

                return m_Instance;
            }
        }

        public void EnsureInitialized()
        {
        }

        public bool IsInitialized
        {
            get { return m_IsInitialized; }
            set { m_IsInitialized = value; }
        }

        public static int RefCount
        {
            get { return m_RefCount; }
        }

        public void ForceGabageCollection()
        {
            if (PDFLibraryMemoryWatcher.I != null)
                PDFLibraryMemoryWatcher.I.ForceGarbageCollection = true;
        }

        #region NATIVE

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(PLUGIN_ASSEMBLY)]
        private static extern void PDFJS_InitLibrary();
#else
        [DllImport(PLUGIN_ASSEMBLY)]
        private static extern uint FPDF_GetLastError();

        [DllImport(PLUGIN_ASSEMBLY)]
        private static extern void FPDF_DestroyLibrary();

        [DllImport(PLUGIN_ASSEMBLY)]
        private static extern void FPDF_InitLibrary();

        [DllImport(PLUGIN_ASSEMBLY, CharSet = CharSet.Ansi)]
        private static extern void FPDF_InitLibrary(string libSignature);
#endif

        #endregion
    }

    public class PDFLibraryMemoryWatcher : MonoBehaviour
    {
        private static PDFLibraryMemoryWatcher s_I;

        public static PDFLibraryMemoryWatcher I
        {
            get { return s_I; }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void EnsureInitialized()
        {
            if (s_I == null)
            {
                GameObject newObj = new GameObject("PDFLibraryMemoryWatcher");
                newObj.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(newObj);

                s_I = newObj.AddComponent<PDFLibraryMemoryWatcher>();
            }
        }


        public bool ForceGarbageCollection { get; set; }

        private const int CollectPressure = 16;
        private const float CollectPressureInterval = 2.0f;
        private int m_LastCollectRefCount;

        private IEnumerator Start()
        {
            while (true)
            {
                if (ForceGarbageCollection || Mathf.Abs(m_LastCollectRefCount - PDFLibrary.RefCount) >= CollectPressure)
                {
                    m_LastCollectRefCount = PDFLibrary.RefCount;

                    if (ForceGarbageCollection)
                    {
                        ForceGarbageCollection = false;

                        yield return null;
                    }

                    GC.Collect();

                    float time = Time.unscaledTime;

                    while (Time.unscaledTime - time < CollectPressureInterval)
                        yield return null;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}