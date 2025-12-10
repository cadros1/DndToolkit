using System.Text.Json.Serialization;

namespace DnDToolkit.Models
{
    // 用于接收 GitHub API 的返回数据
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = ""; // 版本号，如 "v1.0.1"

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = ""; // Release 页面链接

        [JsonPropertyName("body")]
        public string Body { get; set; } = ""; // 更新日志

        [JsonPropertyName("name")]
        public string Name { get; set; } = ""; // Release 标题
    }
}