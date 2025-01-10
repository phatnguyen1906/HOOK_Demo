using Azure.AI.OpenAI;
using System;
using System.Threading.Tasks;

public class OpenAIService
{
    private readonly OpenAIClient _client;
    private const string DEPLOYMENT_NAME = "gpt-3.5-turbo";

    public OpenAIService(string apiKey)
    {
        _client = new OpenAIClient(apiKey);
    }

    public async Task<string> GetDetailedExplanation(string word)
    {
        try
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, 
                        "Bạn là một từ điển thông minh. Hãy giải thích từ theo format sau:\n" +
                        "1. Nghĩa của từ\n" +
                        "2. Phát âm (nếu là tiếng Anh)\n" +
                        "3. Loại từ (danh từ, động từ, ...)\n" +
                        "4. Cách sử dụng\n" +
                        "5. Ví dụ\n" +
                        "6. Các từ đồng nghĩa/trái nghĩa (nếu có)"
                    ),
                    new ChatMessage(ChatRole.User, $"Giải thích từ: {word}")
                },
                MaxTokens = 500,
                Temperature = 0.7f
            };

            var response = await _client.GetChatCompletionsAsync(
                deploymentOrModelName: DEPLOYMENT_NAME,
                chatCompletionsOptions);

            return response.Value.Choices[0].Message.Content;
        }
        catch (Exception ex)
        {
            return $"Lỗi khi truy vấn OpenAI: {ex.Message}";
        }
    }
} 