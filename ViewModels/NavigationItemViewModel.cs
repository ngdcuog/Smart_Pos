namespace SmartPOS.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string title, string glyph, PlaceholderViewModel viewModel)
    {
        Title = title;
        Glyph = glyph;
        ViewModel = viewModel;
    }

    public string Title { get; }

    public string Glyph { get; }

    public PlaceholderViewModel ViewModel { get; }
}
