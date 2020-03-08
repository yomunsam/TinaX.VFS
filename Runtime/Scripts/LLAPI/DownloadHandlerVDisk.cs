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
using TinaX.IO;

namespace TinaX.VFSKit.Network
{
    /// <summary>
    /// Use to load android 
    /// </summary>
    public class DownloadHandlerVDisk : DownloadHandlerScript
    {

        private ulong mContentLength = 0;
        private ulong mReceivedLength = 0; //已送达的数据长度
        private FileStream mStream;

        public DownloadHandlerVDisk(string save_target,int bufferSize = 4096, FileShare fileShare = FileShare.ReadWrite) :base(new byte[bufferSize])
        {
            string directory = Path.GetDirectoryName(save_target);
            XDirectory.CreateIfNotExists(directory);
            mStream = new FileStream(save_target, FileMode.OpenOrCreate, FileAccess.Write, fileShare, bufferSize);
        }

        protected override float GetProgress()
        {
            return mContentLength <= 0 ? 0 : Mathf.Clamp01((float)mReceivedLength / (float)mContentLength);
        }

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
            mStream.Write(data, 0, dataLength);

            return true;
        }

        protected override void CompleteContent()
        {
            CloseStream();
        }

        public new void Dispose()
        {
            CloseStream();
            base.Dispose();
        }

        private void CloseStream()
        {
            if (mStream != null)
            {
                mStream.Dispose();
                mStream = null;
            }
        }

    }
}
