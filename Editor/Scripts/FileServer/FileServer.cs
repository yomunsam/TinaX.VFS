using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace TinaXEditor.VFSKit.FileServer
{
    /// <summary>
    /// File Server 文件服务器
    /// </summary>
    public class FileServer
    {
        public static bool IsSupported => HttpListener.IsSupported;

        public int Port { get; set; } = 8080;

        public string UrlHead { get; set; } = "/files/";
        public string FilesRootFolder { get; set; } = "";

        public bool IsRunning => mHttp.IsListening;
        //public List<string> Prefixes = new List<string>();

        private HttpListener mHttp;


        public FileServer()
        {
            mHttp = new HttpListener();
        }

        public void Run()
        {
            mHttp.Prefixes.Add($"http://*:{this.Port}/");
            mHttp.Start();

            if (!UrlHead.StartsWith("/"))
                UrlHead = "/" + UrlHead;
            if (!UrlHead.EndsWith("/"))
                UrlHead += "/";

            mHttp.BeginGetContext(Result, null);

        }

        public void Stop()
        {
            mHttp.Stop();
        }

        private void Result(IAsyncResult result)
        {
            //再设置一次
            mHttp?.BeginGetContext(this.Result, null);

            var context = mHttp.EndGetContext(result);
            var req = context.Request;
            var resp = context.Response;
            resp.ContentEncoding = Encoding.UTF8;

            byte[] resp_bytes = null;


            if (req.HttpMethod != "GET")
            {
                Handle403(ref resp, ref resp_bytes, $"you can not request assets by url: {req.RawUrl}\nMethod:{req.HttpMethod}");
            }
            else
            {
                //Handle Url
                if (req.RawUrl.ToLower().StartsWith(UrlHead))
                {
                    //请求文件，
                    //首先检查文件服务器自身指定的文件根目录是否存在
                    if (!Directory.Exists(FilesRootFolder))
                    {
                        Handle502(ref resp, ref resp_bytes, "File Server Internal Error: No valid folder path specified");
                    }
                    else
                    {
                        string file_path = req.RawUrl.Substring(UrlHead.Length, req.RawUrl.Length - UrlHead.Length);
                        string sys_path = Path.Combine(FilesRootFolder, file_path);
                        if (File.Exists(sys_path))
                        {
                            resp_bytes = File.ReadAllBytes(sys_path);
                            resp.ContentType = "application/octet-stream";
                            resp.StatusCode = 200;
                        }
                        else
                        {
                            Handle404(ref resp, ref resp_bytes, "The file you request not found: " + file_path);
                        }
                    }
                }
                else
                {
                    Handle404(ref resp, ref resp_bytes, "The url you request not found: " + req.RawUrl);
                }
            }
            
            try
            {
                using (var stream = resp.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(resp_bytes, 0, resp_bytes.Length);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[TinaX.VFS][File Server] Response error:");
                UnityEngine.Debug.LogException(ex);
            }
        }


        private void Handle404(ref HttpListenerResponse resp,ref byte[] resp_bytes, string message)
        {
            resp.StatusCode = 404;
            resp.ContentType = "text/plain;charset=UTF-8";
            resp_bytes = Encoding.UTF8.GetBytes(message);
        }

        private void Handle403(ref HttpListenerResponse resp, ref byte[] resp_bytes, string message)
        {
            resp.StatusCode = 403;
            resp.ContentType = "text/plain;charset=UTF-8";
            resp_bytes = Encoding.UTF8.GetBytes(message);
        }

        private void Handle502(ref HttpListenerResponse resp, ref byte[] resp_bytes, string message)
        {
            resp.StatusCode = 502;
            resp.ContentType = "text/plain;charset=UTF-8";
            resp_bytes = Encoding.UTF8.GetBytes(message);
        }

    }
}
