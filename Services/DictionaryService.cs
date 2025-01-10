public class DictionaryService
{
    private readonly HttpClient _httpClient;
    private const string API_URL = "https://api.dictionaryapi.dev/api/v2/entries/en/";

    public DictionaryService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> LookupWord(string word)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{API_URL}{word}");
            // Parse JSON response và trả về nghĩa của từ
            // Đây là ví dụ đơn giản, bạn cần parse JSON phù hợp với API
            return response;
        }
        catch (Exception ex)
        {
            return $"Không tìm thấy từ này trong từ điển: {ex.Message}";
        }
    }
} 