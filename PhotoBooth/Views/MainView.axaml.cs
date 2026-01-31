using Avalonia.Controls;
using Avalonia.Input;
using PhotoBooth.ViewModels;

namespace PhotoBooth.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    private void OnThumbnailPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.ShowGalleryCommand.Execute(null);
        }
    }
}
