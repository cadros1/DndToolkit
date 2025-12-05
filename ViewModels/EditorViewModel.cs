using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DnDToolkit.Messages;
using DnDToolkit.Models;
using DnDToolkit.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;

namespace DnDToolkit.ViewModels
{
    public partial class EditorViewModel : ObservableObject, IRecipient<EditCharacterMessage>
    {
        private readonly ICharacterService _characterService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private Character currentCharacter = new();

        public EditorViewModel(ICharacterService characterService, INavigationService navigationService)
        {
            _characterService = characterService;
            _navigationService = navigationService;
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(EditCharacterMessage message)
        {
            if (message.Value == null)
            {
                CurrentCharacter = new Character();
            }
            else
            {
                CurrentCharacter = message.Value;
            }

            // 确保至少有3个武器槽位供界面绑定
            while (CurrentCharacter.Weapons.Count < 3)
            {
                CurrentCharacter.Weapons.Add(new Weapon());
            }
        }

        [RelayCommand]
        private void ChangePortrait()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Title = "选择角色画像"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                    string base64String = Convert.ToBase64String(imageBytes);
                    CurrentCharacter.Profile.PortraitBase64 = base64String;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"图片读取失败: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentCharacter.Profile.CharacterName))
            {
                MessageBox.Show("角色名称不能为空", "提示");
                return;
            }
            try
            {
                CurrentCharacter.Combat.HitDiceCurrent = CurrentCharacter.Combat.HitDiceTotal;
                CurrentCharacter.Combat.HitPointsCurrent = CurrentCharacter.Combat.HitPointsMax;
                var allSpells = CurrentCharacter.Spellbook.AllSpells;
                for (int i = 1; i <= 9; i++)
                {
                    allSpells[i].RemainSlots = allSpells[i].TotalSlots;
                }
                await _characterService.SaveCharacterAsync(CurrentCharacter);
                MessageBox.Show("保存成功！", "提示");
                _navigationService.Navigate(typeof(Views.CharacterListPage));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误");
            }
        }

        [RelayCommand]
        private void Cancel() => _navigationService.GoBack();
    }
}
