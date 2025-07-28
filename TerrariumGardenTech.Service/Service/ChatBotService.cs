//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TerrariumGardenTech.Repositories.Repositories;
//using TerrariumGardenTech.Service.IService;

//namespace TerrariumGardenTech.Service.Service
//{
//    public class ChatBotService
//    {
//        private readonly ChatGPTRepository _chatGPTRepository;

//        public ChatBotService()
//        {
//            _chatGPTRepository = new ChatGPTRepository();
//        }

//        public async Task<string> GetChatResponseAsync(string userInput)
//        {
//            return await _chatGPTRepository.GetResponseFromGPTAsync(userInput);
//        }
//    }
//}
