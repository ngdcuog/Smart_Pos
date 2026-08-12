using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartPOS.ViewModels;

public abstract class PlaceholderViewModel : ObservableObject
{
    protected PlaceholderViewModel(string title, string description, string developmentMessage)
    {
        Title = title;
        Description = description;
        DevelopmentMessage = developmentMessage;
    }

    public string Title { get; }

    public string Description { get; }

    public string DevelopmentMessage { get; }
}
