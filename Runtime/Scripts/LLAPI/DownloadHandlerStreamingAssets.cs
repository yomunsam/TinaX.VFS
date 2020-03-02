using System;
/*
 * 本页参考：https://gist.github.com/luke161/a251b01c00f58d65a252812be8dce670
 */
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TinaX;

namespace TinaX.VFSKitInternal
{
    /// <summary>
    /// Use to load android 
    /// </summary>
    public class DownloadHandlerStreamingAssets : DownloadHandlerScript
    {

        private ulong mContentLength = 0;
        private ulong mReceivedLength = 0; //已送达的数据长度
        private FileStream mStream;


        /// <summary>
        /// HTTP请求中收到Content-Length头时会调用，可能会在请求中响应多次。
        /// </summary>
        /// <param name="contentLength"></param>
        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            mContentLength = contentLength;
        }

        /// <summary>
        /// 数据到达，从Remote获取到数据时，每帧调用一次。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length == 0) return false; //如果返回值为false，UnityWebRequest将被中止， 如果返回true, 则继续运行。
            mReceivedLength += (ulong)dataLength;
            return true;
        }

    }
}
