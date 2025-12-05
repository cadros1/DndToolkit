using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDToolkit.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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
                FileName = "DnD_5e_Character_Sheet.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await _characterService.SaveTemplatePdfAsync(saveFileDialog.FileName);
                MessageBox.Show("PDF 已保存 (模拟)", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
