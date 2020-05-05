
namespace Paroxe.PdfRenderer.WebGL
{
    public interface IPDFJS_Promise
    {
        string PromiseHandle
        {
            get;
        }

        bool HasFinished
        {
            get;
            set;
        }

        bool HasSucceeded
        {
            get;
            set;
        }

        bool HasBeenCancelled
        {
            get;
            set;
        }

        string JSObjectHandle
        {
            get;
            set;
        }

        bool HasReceivedJSResponse
        {
            get;
            set;
        }

        float Progress
        {
            get;
            set;
        }
    }
}
