using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinaX.VFSKit.Exceptions
{
    public class DownloadNetworkException : VFSException
    {
        public string Url { get; private set; }
        public long? HttpStatusCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public DownloadNetworkException(string message, string url, string errorMsg, long? httpStatusCode) : base(message, VFSErrorCode.DownloadNetworkError) { this.Url = url; this.ErrorMessage = errorMsg; this.HttpStatusCode = httpStatusCode; }
    }
}
