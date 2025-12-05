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

        [ObservableProperty]
        private ObservableCollection<Character> characters = new();

        public CharacterListViewModel(ICharacterService characterService, INavigationService navigationService)
        {
            _characterService = characterService;
            _navigationService = navigationService;
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
    }
}
