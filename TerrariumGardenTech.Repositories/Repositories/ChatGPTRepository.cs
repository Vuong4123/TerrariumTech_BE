//using System.Threading.Tasks;
//using TerrariumGardenTech.Repositories.Base;
//using OpenAI; // Đảm bảo bạn đang sử dụng không gian tên đúng
//using OpenAI.Chat;

//namespace TerrariumGardenTech.Repositories.Repositories
//{
//    public class ChatGPTRepository : GenericRepository<object>
//    {
//        private readonly string _apiKey = "YOUR_OPENAI_API_KEY";
//        private readonly OpenAIClient _api;

//        public ChatGPTRepository() : base()
//        {
//            _api = new OpenAIClient(_apiKey);
//        }

//        public async Task<string> GetResponseFromGPTAsync(string userInput)
//        {
//            // Create a chat message for the request
//            var chatRequest = new ChatRequest
//            {
//                Model = "gpt-3.5-turbo", // Hoặc model khác nếu cần
//                Messages = new[]
//                {
//                    new ChatMessage
//                    {
//                        Role = ChatRole.User,
//                        Content = userInput
//                    }
//                }
//            };



//            // Call the OpenAI API to get the result
//            var response = await _api.ChatEndpoint.GetCompletionAsync(chatRequest);
//            return response.Choices[0].Message.Content.Trim(); // Trả về nội dung phản hồi từ GPT
//        }
//    }
//}
