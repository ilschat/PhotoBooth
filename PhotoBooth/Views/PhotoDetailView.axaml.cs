using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PhotoBooth.ViewModels;

namespace PhotoBooth.Views;

public partial class PhotoDetailView : UserControl
{
    public PhotoDetailView()
    {
        InitializeComponent();
    }
    private void OnBackgroundPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.ClosePhotoDetailCommand.Execute(null);
        }
    }
}