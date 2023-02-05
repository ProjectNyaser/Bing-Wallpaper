using System.Net.Http.Headers;
using System.Linq;

namespace Bing_Wallpaper
{
    internal class Program
    {
        private const string BaseAddress = "https://cn.bing.com";
        private const string Url = "/HPImageArchive.aspx?n=1&mkt=zh-CN";
        private static readonly HttpClient httpClient = new();
        private static readonly Stopwatch stopwatch = new();

        public static async Task Main(string[] args)
        {
            string[] args_lower = new string[args.Length];
            bool pauseBeforeExiting = false;
            for (int i = 0; i < args.Length; i++)
            {
                args_lower[i] = args[i];
            }
            if (args_lower.Contains("-m", "--minimize"))
            {
                _ = WindowHelpers.ShowWindow(WindowHelpers.FindWindow(null, Console.Title), WindowHelpers.SW_MINIMIZE);
            } else if (args_lower.Contains("-k", "--keep"))
            {
                pauseBeforeExiting = true;
            }

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
                throw new($"创建目录 {SaveFolder} 失败");
            }
            try
            {
                httpClient.BaseAddress = new Uri(BaseAddress);

                Console.Write($"建立连接：{BaseAddress}..");
                stopwatch.Start();
                httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = new HttpMethod("HEAD"),
                });
                stopwatch.Stop();
                Console.WriteLine($"{httpResponseMessage.EnsureSuccessStatusCode().StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
                _ = httpResponseMessage.EnsureSuccessStatusCode();

                Console.Write($"下    载：{Url}..");
                stopwatch.Start();
                httpResponseMessage = await httpClient.GetAsync(Url);
                stopwatch.Stop();
                Console.WriteLine($"{httpResponseMessage.EnsureSuccessStatusCode().StatusCode} ({stopwatch.ElapsedMilliseconds} ms)");
                stopwatch.Reset();
                _ = httpResponseMessage.EnsureSuccessStatusCode();

                Console.Write("分    析：Content..");
                stopwatch.Start();
                string html = await httpResponseMessage.Content.ReadAsStringAsync();
                html = html.Replace("&amp;", "&");
                foreach (string s in from string s in html.Split(new string[] { "<startdate>", "</startdate>" }, StringSplitOptions.None)
                                     where s != "" && !s.Contains('<') && !s.Contains('>')
                                     select s)
                {
                    fileName = $"{s}.jpg";
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
                _ = httpResponseMessage.EnsureSuccessStatusCode();

                httpClient.DefaultRequestHeaders.Connection.Add("close");
                HttpResponseHeaders headers = (await httpClient.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Head
                })).Headers;
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
                pauseBeforeExiting = true;
                _ = WindowHelpers.ShowWindow(WindowHelpers.FindWindow(null, Console.Title), WindowHelpers.SW_RESTORE);
                string logPath = $@"{Directory.GetCurrentDirectory()}\Logs\";
                string message = $"{e.Message}\r\n{e.StackTrace}";
                Directory.CreateDirectory(logPath);
                File.WriteAllText(logPath+=$"{DateTime.Now.Ticks}.log", message);
                Console.WriteLine($"\r\n\r\n抛出异常：{message}\r\n保存日志：{logPath}\r\n\r\n");
            }
            finally
            {
                if (pauseBeforeExiting)
                {
                    Console.Write("点击 Enter 或 Ctrl + C 退出..");
                    _ = Console.ReadLine();
                }
            }
        }
    }

    internal static class WindowHelpers
    {
        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
    }

    public static class Extensions
    {
        public static bool Contains(this Array array, params object[] values)
        {
            foreach (var _ in from object value in values
                              from object item in array
                              where Equals(item, value)
                              select new { })
            {
                return true;
            }

            return false;
        }
    }
}
