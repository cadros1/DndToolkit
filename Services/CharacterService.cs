using DnDToolkit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DnDToolkit.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly string _folderPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public CharacterService()
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _folderPath = Path.Combine(docPath, "DnDToolkit", "Characters");
            Directory.CreateDirectory(_folderPath);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = true
            };
        }

        public async Task<List<Character>> GetAllCharactersAsync()
        {
            var list = new List<Character>();
            // 确保文件夹存在
            if (!Directory.Exists(_folderPath)) return list;

            var files = Directory.GetFiles(_folderPath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    string json = await File.ReadAllTextAsync(file);
                    var character = JsonSerializer.Deserialize<Character>(json, _jsonOptions);

                    if (character != null)
                    {
                        // 记录该对象是从哪个文件加载的
                        character.FilePath = file;

                        // 容错处理：如果 JSON 里没有 ID (旧数据)，给它生成一个
                        if (character.Id == Guid.Empty)
                        {
                            character.Id = Guid.NewGuid();
                        }

                        list.Add(character);
                    }
                }
                catch { /* 忽略损坏文件 */ }
            }
            return list;
        }

        public async Task SaveCharacterAsync(Character character)
        {
            // 1. 确定标准的目标路径：文件夹 + GUID + .json
            string standardFileName = $"{character.Id}.json";
            string targetFilePath = Path.Combine(_folderPath, standardFileName);

            // 2. 清理旧文件逻辑 (防止产生垃圾文件)
            // 如果我们知道这个角色之前存放在 FilePath 中
            // 且之前的路径 不等于 现在的标准路径 (比如之前叫 "Name_ID.json")
            // 且之前的那个文件确实存在
            if (!string.IsNullOrEmpty(character.FilePath) &&
                !string.Equals(character.FilePath, targetFilePath, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(character.FilePath))
            {
                try
                {
                    File.Delete(character.FilePath); // 删除旧命名的文件
                }
                catch
                {
                    // 如果删除失败（比如被占用），可以选择记录日志或忽略
                    // 最坏的情况也就是多了一个冗余文件
                }
            }

            // 3. 执行保存
            string json = JsonSerializer.Serialize(character, _jsonOptions);
            await File.WriteAllTextAsync(targetFilePath, json);

            // 4. 更新内存中的路径
            character.FilePath = targetFilePath;
        }

        public async Task DeleteCharacterAsync(Character character)
        {
            if (string.IsNullOrEmpty(character.FilePath)) return;

            // 为了防止文件占用导致报错，包裹在一个 try-catch 中
            await Task.Run(() =>
            {
                if (File.Exists(character.FilePath))
                {
                    File.Delete(character.FilePath);
                }
            });
        }

        public async Task SaveTemplatePdfAsync(string destinationPath)
        {
            // 实际开发中，你应该将PDF作为 "嵌入的资源" (Embedded Resource) 放在项目中
            // 这里演示简单的写入逻辑
            // byte[] pdfBytes = Properties.Resources.CharacterSheetTemplate; 
            // await File.WriteAllBytesAsync(destinationPath, pdfBytes);

            // 模拟延迟
            await Task.Delay(500);
        }
    }
}
