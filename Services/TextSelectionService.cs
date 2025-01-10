using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;

public class TextSelectionService
{
    private readonly Window _window;
    private readonly OpenAIService _openAIService;
    private Popup _dictionaryPopup;
    private StackPanel _popupContent;

    public TextSelectionService(Window window, OpenAIService openAIService)
    {
        _window = window;
        _openAIService = openAIService;
        InitializePopup();
    }

    private void InitializePopup()
    {
        _popupContent = new StackPanel
        {
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            MaxWidth = 400
        };

        _dictionaryPopup = new Popup
        {
            Child = _popupContent
        };

        // Thêm sự kiện click bên ngoài để đóng popup
        _window.Content.PointerPressed += (s, e) =>
        {
            if (_dictionaryPopup.IsOpen)
            {
                _dictionaryPopup.IsOpen = false;
            }
        };
    }

    public void Initialize()
    {
        _window.Content.PointerReleased += Content_PointerReleased;
    }

    private async void Content_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var selectedText = await GetSelectedText();
        if (!string.IsNullOrEmpty(selectedText))
        {
            ShowLoadingPopup(e.GetCurrentPoint(null).Position);
            var explanation = await _openAIService.GetDetailedExplanation(selectedText);
            UpdatePopupContent(selectedText, explanation);
        }
    }

    private async Task<string> GetSelectedText()
    {
        var dataPackage = Clipboard.GetContent();
        if (dataPackage.Contains(StandardDataFormats.Text))
        {
            return await dataPackage.GetTextAsync();
        }
        return string.Empty;
    }

    private void ShowLoadingPopup(Point position)
    {
        _popupContent.Children.Clear();
        _popupContent.Children.Add(new ProgressRing 
        { 
            IsActive = true,
            Margin = new Thickness(10)
        });
        
        _dictionaryPopup.HorizontalOffset = position.X;
        _dictionaryPopup.VerticalOffset = position.Y;
        _dictionaryPopup.IsOpen = true;
    }

    private void UpdatePopupContent(string word, string explanation)
    {
        _popupContent.Children.Clear();

        // Từ được chọn
        _popupContent.Children.Add(new TextBlock
        {
            Text = word,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        });

        // Nút đóng
        var closeButton = new Button
        {
            Content = "✕",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, -30, 0, 10)
        };
        closeButton.Click += (s, e) => _dictionaryPopup.IsOpen = false;
        _popupContent.Children.Add(closeButton);

        // Giải thích từ OpenAI
        _popupContent.Children.Add(new TextBlock
        {
            Text = explanation,
            TextWrapping = TextWrapping.Wrap
        });
    }
} 