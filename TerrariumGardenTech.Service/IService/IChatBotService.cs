using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.IService
{
    public interface IChatBotService
    {
        Task<string> GetChatResponseAsync(string userInput);
    }
}
