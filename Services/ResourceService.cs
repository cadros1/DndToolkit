using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DnDToolkit.Services
{
    public class ResourceService
    {
        /// <summary>
        /// 将嵌入的资源保存到指定路径
        /// </summary>
        /// <param name="resourceEndName">资源文件的后缀名（如 "Character.pdf"）</param>
        /// <param name="destinationPath">保存的目标路径</param>
        public async Task ExtractResourceToFileAsync(string resourceEndName, string destinationPath)
        {
            await Task.Run(() =>
            {
                var assembly = Assembly.GetExecutingAssembly();

                // 模糊匹配资源名称
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith(resourceEndName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(resourceName))
                {
                    throw new FileNotFoundException($"未找到嵌入资源：{resourceEndName}");
                }

                using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                using FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                stream?.CopyTo(fileStream);
            });
        }
    }
}