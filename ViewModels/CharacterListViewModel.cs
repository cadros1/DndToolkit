using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DnDToolkit.Messages;
using DnDToolkit.Models;
using DnDToolkit.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;

namespace DnDToolkit.ViewModels
{
    public partial class CharacterListViewModel : ObservableObject
    {
        private readonly ICharacterService _characterService;
        private readonly INavigationService _navigationService;
        private readonly PdfDataService _pdfDataService;

        [ObservableProperty]
        private ObservableCollection<Character> characters = new();

        public CharacterListViewModel(ICharacterService characterService, INavigationService navigationService, PdfDataService pdfDataService)
        {
            _characterService = characterService;
            _navigationService = navigationService;
            _pdfDataService = pdfDataService;

        }

        // 页面加载时调用此方法刷新数据
        public async Task LoadDataAsync()
        {
            var list = await _characterService.GetAllCharactersAsync();
            Characters = new ObservableCollection<Character>(list);
        }

        [RelayCommand]
        private void CreateNew()
        {
            _navigationService.Navigate(typeof(Views.EditorPage));
            // 发送 null 表示新建
            WeakReferenceMessenger.Default.Send(new EditCharacterMessage(null));
        }

        [RelayCommand]
        private void Edit(Character character)
        {
            // 跳转
            _navigationService.Navigate(typeof(Views.EditorPage));
            // 发送要编辑的角色对象
            WeakReferenceMessenger.Default.Send(new EditCharacterMessage(character));
        }

        [RelayCommand]
        private async Task Delete(Character character)
        {
            if (character == null) return;

            // 1. 弹出确认框
            var result = MessageBox.Show(
                $"确定要永久删除角色 {character.Profile.CharacterName} 吗？\n此操作无法撤销。",
                "删除确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // 2. 从硬盘删除文件
                await _characterService.DeleteCharacterAsync(character);

                // 3. 从界面列表中移除
                Characters.Remove(character);
            }
        }

        // === 导入功能 ===
        [RelayCommand]
        private async Task Import()
        {
            // 1. 调用服务读取 PDF
            var character = await _pdfDataService.ImportCharacterPdfAsync();

            if (character != null)
            {
                // 2. 保存到本地数据库/JSON
                await _characterService.SaveCharacterAsync(character);

                // 3. 添加到界面列表
                Characters.Add(character);

                MessageBox.Show($"角色“{character.Profile.CharacterName}”导入成功！", "成功");
            }
        }

        // === 导出功能 ===
        [RelayCommand]
        private async Task Export(Character character)
        {
            if (character == null) return;

            // 调用服务导出 PDF
            await _pdfDataService.ExportCharacterPdfAsync(character);

            // (PdfDataService 内部已经处理了 SaveFileDialog，所以这里不需要额外提示，除非出错)
        }
    }
}
