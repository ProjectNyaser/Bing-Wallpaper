using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Bing_Wallpaper
{
    internal class Program
    {
        private const string BaseAddress = "https://cn.bing.com";
        private const string Url = "/HPImageArchive.aspx?n=1&mkt=zh-CN";
        private static readonly HttpClient httpClient = new();
        private static readonly Stopwatch stopwatch = new();

        public static async Task Main()
        {
            string address = "";
            string fileName = "";
            string SaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (!Directory.Exists(SaveFolder))
            {
                SaveFolder = Environment.CurrentDirectory;
            }
            SaveFolder += "\\Bing Wallpaper\\";
            Console.Title = "Bing Wallpaper";
            Console.Write("保存位置：");
            Console.WriteLine(SaveFolder);
            if (!Directory.CreateDirectory(SaveFolder).Exists)
            {
                Console.Write("创建目录失败，点击任意键退出..");
                _ = Console.ReadKey(true);
                return;
            }
            try
            {
                httpClient.BaseAddress = new Uri(BaseAddress);
                httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");

                Console.Write($"建立连接：{BaseAddress}..");
                stopwatch.Start();
                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = new HttpMethod("HEAD"),
                    RequestUri = httpClient.BaseAddress,
                });
                stopwatch.Stop();
                Console.WriteLine($"{httpResponseMessage.EnsureSuccessStatusCode().StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
                httpResponseMessage.EnsureSuccessStatusCode();

                Console.Write($"下    载：{Url}..");
                stopwatch.Start();
                httpResponseMessage = await httpClient.GetAsync(Url);
                stopwatch.Stop();
                Console.WriteLine($"{httpResponseMessage.EnsureSuccessStatusCode().StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
                httpResponseMessage.EnsureSuccessStatusCode();

                Console.Write("分    析：Content..");
                stopwatch.Start();
                string html = await httpResponseMessage.Content.ReadAsStringAsync();
                html = html.Replace("&amp;", "&");
                foreach (string s in from string s in html.Split(new string[] { "<startdate>", "</startdate>" }, StringSplitOptions.None)
                                     where s != "" && !s.Contains('<') && !s.Contains('>')
                                     select s)
                {
                    fileName  = $"{s}.jpg";
                }
                foreach (string s in from string s in html.Split(new string[] { "<urlBase>", "</urlBase>" }, StringSplitOptions.None)
                                     where s != "" && !s.Contains('<') && !s.Contains('>')
                                     select s)
                {
                    address = $"{s}_UHD.jpg";
                }
                stopwatch.Stop();
                Console.WriteLine($"OK ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();

                Console.Write($"下    载：{address}..");
                stopwatch.Start();
                httpResponseMessage = await httpClient.GetAsync(address);
                stopwatch.Stop();
                Console.WriteLine($"{httpResponseMessage.EnsureSuccessStatusCode().StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
                httpResponseMessage.EnsureSuccessStatusCode();

                httpClient.Dispose();

                Console.Write($"保    存：{fileName}..");
                stopwatch.Start();
                byte[] wallpaperBinary = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes($"{SaveFolder}{fileName}", wallpaperBinary);
                stopwatch.Stop();
                Console.WriteLine($"{wallpaperBinary.Length:000,000} Bytes ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();

                Console.Write($"设置壁纸：{fileName}..");
                stopwatch.Start();
                [DllImport("user32.dll", EntryPoint = "SystemParametersInfoA")]
                static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
                int stateCode = SystemParametersInfo(20, 0, $"{SaveFolder}{fileName}", 1);
                stopwatch.Stop();
                Console.WriteLine($"{stateCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine($"\r\n\r\nError: {e.Message}\r\n{e.StackTrace}\r\n\r\n");
                File.WriteAllText($"{DateTime.Now.Ticks}.log", e.StackTrace);
                Console.Write("点击任意键退出..");
                _ = Console.ReadKey(true);
            }
        }
    }
}
