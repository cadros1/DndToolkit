using DnDToolkit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Services
{
    public interface ICharacterService
    {
        Task<List<Character>> GetAllCharactersAsync();
        Task SaveCharacterAsync(Character character);
        Task DeleteCharacterAsync(Character character);
        // 为了演示下载PDF功能
        Task SaveTemplatePdfAsync(string destinationPath);
    }
}
