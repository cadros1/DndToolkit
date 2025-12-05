using CommunityToolkit.Mvvm.Messaging.Messages;
using DnDToolkit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Messages
{
    public class EditCharacterMessage : ValueChangedMessage<Character>
    {
        public EditCharacterMessage(Character character) : base(character)
        {
        }
    }
}
