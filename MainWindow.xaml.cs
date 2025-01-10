public sealed partial class MainWindow : Window
{
    private TextSelectionService _textSelectionService;
    private OpenAIService _openAIService;

    public MainWindow()
    {
        this.InitializeComponent();
        
        // Khởi tạo services
        _openAIService = new OpenAIService("your-openai-api-key-here");
        _textSelectionService = new TextSelectionService(this, _openAIService);
        _textSelectionService.Initialize();
    }
} 