using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bing_Wallpaper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string name = "Bing Wallpaper";
            Console.Title = name;
            string url = "https://bing.com/HPImageArchive.aspx?n=1";
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Bing Wallpaper";
            Directory.CreateDirectory(path);
            if (path == "\\Pictures\\Bing Wallpaper")
            {
                path = ".\\";
            }
            Console.WriteLine(path);
            string temp = $"{Environment.GetEnvironmentVariable("Temp")}\\{name}.xml";
            if (temp == $"\\{name}.xml")
            {
                temp = ".\\";
            }
            try
            {
            string[] Files = Directory.GetFiles(path);
            string LastFile = Files[Files.LongLength - 1];
            if (LastFile.Substring(LastFile.Length - 4, 4) == ".jpg")
            {
                SetWallpaper(name, LastFile);
            }
            }
            catch
            {
                //Do Nothing;
            }
            if (DownLoadFile(url, temp, (double Maximum, double Value, double Percent) =>
            {
                string Value_Unit = "Bytes";
                string Maximum_Unit = "Bytes";
                if (Value >= 1024)
                {
                    Value /= 1024;
                    Value_Unit = "Kilobytes";
                    if (Value >= 1024)
                    {
                        Value /= 1024;
                        Value_Unit = "Megabytes";
                        if (Value >= 1024)
                        {
                            Value /= 1024;
                            Value_Unit = "Gigabyte";//不要问我为什么要写这么多单位
                        }
                    }
                }
                if (Maximum >= 1024)
                {
                    Maximum /= 1024;
                    Maximum_Unit = "Kilobytes";
                    if (Maximum >= 1024)
                    {
                        Maximum /= 1024;
                        Maximum_Unit = "Megabytes";
                        if (Maximum >= 1024)
                        {
                            Maximum /= 1024;
                            Maximum_Unit = "Gigabyte";
                        }
                    }
                }
                Console.Title = $"{name} - Downloading - {Value} {Value_Unit} of {Maximum} {Maximum_Unit} - {Percent}%";
            }))
            {
                Console.Title = $"{name} - Download - Done";
                string xml = File.ReadAllText(temp);
                url = $"https://bing.com{GetXMLItemValue(xml, "url")}";
                url = url[0..url.IndexOf("&")];
                path += $"\\{GetXMLItemValue(xml, "enddate")}.jpg";
                if (DownLoadFile(url, path, (double Maximum, double Value, double Percent) =>
                {
                    string Value_Unit = "Bytes";
                    string Maximum_Unit = "Bytes";
                    if (Value >= 1024)
                    {
                        Value /= 1024;
                        Value_Unit = "Kilobytes";
                        if (Value >= 1024)
                        {
                            Value /= 1024;
                            Value_Unit = "Megabytes";
                            if (Value >= 1024)
                            {
                                Value /= 1024;
                                Value_Unit = "Gigabyte";
                            }
                        }
                    }
                    if (Maximum >= 1024)
                    {
                        Maximum /= 1024;
                        Maximum_Unit = "Kilobytes";
                        if (Maximum >= 1024)
                        {
                            Maximum /= 1024;
                            Maximum_Unit = "Megabytes";
                            if (Maximum >= 1024)
                            {
                                Maximum /= 1024;
                                Maximum_Unit = "Gigabyte";
                            }
                        }
                    }
                    Console.Title = $"{name} - Downloading - {Value} {Value_Unit} of {Maximum} {Maximum_Unit} - {Percent}%";
                }))
                {
                    Console.Title = $"{name} - Download - Done";
                    SetWallpaper(name, path);
                }
            }
        }

        private static void SetWallpaper(string name, string file)
        {
            Console.Title = $"{name} - Set Wallpaper";
            Console.WriteLine($"[Set Wallpaper]\nFile={file}");
            SystemParametersInfo(20, 0, file, 0x2);
            Console.Title = $"{name} - Set Wallpaper - Done";
        }

        /// <summary>
        /// 取XML项目值
        /// </summary>
        /// <param name="XML">XML文本</param>
        /// <param name="item">项目名称</param>
        /// <returns></returns>
        public static string GetXMLItemValue(string XML, string item)
        {
            string text = XML;
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (string.IsNullOrEmpty(item))
            {
                return "";
            }

            string left = $"<{item}>";
            string right = $"</{item}>";

            int Lindex = text.IndexOf(left);

            if (Lindex == -1)
            {
                return "";
            }

            Lindex += left.Length;

            int Rindex = text.IndexOf(right, Lindex);

            if (Rindex == -1)
            {
                return "";
            }

            return text[Lindex..Rindex];
        }

        public static bool DownLoadFile(string URL, string Filename, Action<double, double, double> updateProgress = null)
        {
            Console.Title = $"Bing Wallpaper - Download";
            Console.WriteLine($"[Download]\nUrl={URL}\nPath={Filename}");
            Stream st = null;
            Stream so = null;
            System.Net.HttpWebRequest Myrq = null;
            System.Net.HttpWebResponse myrp = null;
            bool flag = false;
            try
            {
                Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL); //从URL地址得到一个WEB请求     
                myrp = (System.Net.HttpWebResponse)Myrq.GetResponse(); //从WEB请求得到WEB响应     
                long totalBytes = myrp.ContentLength; //从WEB响应得到总字节数
                if (File.Exists(Filename) && (int)totalBytes == File.ReadAllBytes(Filename).Length)
                {
                    return true;
                }
                updateProgress?.Invoke((int)totalBytes, 0, 0);//从总字节数得到进度条的最大值  
                st = myrp.GetResponseStream(); //从WEB请求创建流（读）     
                so = new System.IO.FileStream(Filename, System.IO.FileMode.Create); //创建文件流（写）     
                long totalDownloadedByte = 0; //下载文件大小     
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, by.Length); //读流     
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte; //更新文件大小     
                    //Application.DoEvents();
                    so.Write(by, 0, osize); //写流     
                    updateProgress?.Invoke((int)totalBytes, (int)totalDownloadedByte, (double)totalDownloadedByte / totalBytes * 100);//更新进度条 
                    osize = st.Read(by, 0, by.Length); //读流     
                }
                //更新进度
                updateProgress?.Invoke((int)totalBytes, (int)totalDownloadedByte, (double)totalDownloadedByte / totalBytes * 100);
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
                throw;
            }
            finally
            {
                if (Myrq != null)
                {
                    Myrq.Abort();//销毁关闭连接
                }
                if (myrp != null)
                {
                    myrp.Close();//销毁关闭响应
                }
                if (so != null)
                {
                    so.Close(); //关闭流 
                }
                if (st != null)
                {
                    st.Close(); //关闭流  
                }
            }
            return flag;
        }

        /// <summary>
        /// 系统参数信息
        /// </summary>
        /// <param name="uAction">20</param>
        /// <param name="uParam">0</param>
        /// <param name="lpvParam">file</param>
        /// <param name="fuWinIni">0x2</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}
