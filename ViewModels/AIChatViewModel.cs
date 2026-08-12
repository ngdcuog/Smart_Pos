using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Services;
using System.Windows;

namespace SmartPOS.ViewModels;

public sealed record ChatMessage(string Role, string Content, DateTime Timestamp);

public partial class AIChatViewModel(IAIChatService ai)
    : PlaceholderViewModel("Trợ lý AI", "Hỏi về doanh thu, tồn kho và hoạt động cửa hàng.", string.Empty)
{
    public ObservableCollection<ChatMessage> Messages { get; } = [];
    public string[] SuggestedQuestions => ["Doanh thu hôm nay", "Top sản phẩm 7 ngày", "Sản phẩm sắp hết", "Đơn hàng gần đây"];

    [ObservableProperty] private string currentQuestion = string.Empty;
    [ObservableProperty] private bool isSending;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private bool hasMessages;
    [ObservableProperty] private bool hasError;
    public Visibility MessagesEmptyVisibility => HasMessages ? Visibility.Collapsed : Visibility.Visible;

    [RelayCommand]
    private async Task SendAsync()
    {
        var question = CurrentQuestion.Trim();
        if (question.Length == 0 || IsSending) return;

        Messages.Add(new ChatMessage("Bạn", question, DateTime.Now));
        HasMessages = true;
        CurrentQuestion = string.Empty;

        try
        {
            IsSending = true;
            ErrorMessage = null;
            Messages.Add(new ChatMessage("Trợ lý AI", "Đang phân tích dữ liệu...", DateTime.Now));
            var pending = Messages[^1];
            var answer = await ai.AskAsync(question);
            Messages.Remove(pending);
            Messages.Add(new ChatMessage("Trợ lý AI", answer, DateTime.Now));
        }
        catch (InvalidOperationException ex)
        {
            if (Messages.LastOrDefault()?.Content == "Đang phân tích dữ liệu...") Messages.RemoveAt(Messages.Count - 1);
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            if (Messages.LastOrDefault()?.Content == "Đang phân tích dữ liệu...") Messages.RemoveAt(Messages.Count - 1);
            ErrorMessage = $"Không thể kết nối trợ lý AI: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    [RelayCommand]
    private Task UseSuggestedAsync(string question)
    {
        CurrentQuestion = question;
        return SendAsync();
    }

    [RelayCommand]
    private void Clear()
    {
        Messages.Clear();
        ErrorMessage = null;
        HasMessages = false;
    }

    partial void OnErrorMessageChanged(string? value) => HasError = !string.IsNullOrWhiteSpace(value);
    partial void OnHasMessagesChanged(bool value) => OnPropertyChanged(nameof(MessagesEmptyVisibility));
}
