using DnDToolkit.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace DnDToolkit.Services
{
    public class UpdateService
    {
        private const string RepoOwner = "cadros1";
        private const string RepoName = "DndToolkit";
        private const string ApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

        public async Task<(bool hasUpdate, GitHubRelease? releaseInfo, string currentVersion)> CheckForUpdateAsync()
        {
            // 1. 获取当前版本
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string currentVersionStr = $"{version?.Major}.{version?.Minor}.{version?.Build}"; // 例如 1.0.0

            try
            {
                using HttpClient client = new HttpClient();
                // GitHub API 要求必须设置 User-Agent
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DnDToolkit-App");
                client.Timeout = TimeSpan.FromSeconds(10);

                // 2. 请求 GitHub API
                var release = await client.GetFromJsonAsync<GitHubRelease>(ApiUrl);

                if (release != null)
                {
                    // 处理版本号：GitHub tag 通常是 "v1.0.1"，我们需要去掉 'v'
                    string remoteVersionStr = release.TagName.TrimStart('v');

                    // 3. 对比版本
                    if (Version.TryParse(remoteVersionStr, out Version? remoteVersion) &&
                        Version.TryParse(currentVersionStr, out Version? currentVersion))
                    {
                        if (remoteVersion > currentVersion)
                        {
                            return (true, release, currentVersionStr);
                        }
                    }
                }

                return (false, release, currentVersionStr);
            }
            catch
            {
                // 网络错误或解析失败，视为无更新或检查失败
                return (false, null, currentVersionStr);
            }
        }
    }
}