using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDToolkit.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DnDToolkit.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ICharacterService _characterService;

        public DashboardViewModel(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [RelayCommand]
        private async Task DownloadPdf()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = "DnD5e角色卡.pdf"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("Character.pdf", StringComparison.OrdinalIgnoreCase)) ?? throw new FileNotFoundException("Character.pdf template not found.");
                using Stream templateStream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("读取嵌入模板失败");

                using var fileStream = System.IO.File.Create(saveFileDialog.FileName);
                await templateStream.CopyToAsync(fileStream);

                MessageBox.Show("PDF 已保存。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存PDF时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
