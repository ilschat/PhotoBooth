using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PhotoBooth.ViewModels;

namespace PhotoBooth.Views;

public partial class PhotoGalleryView : UserControl
{
    public PhotoGalleryView()
    {
        InitializeComponent();
    }
    private void OnPhotoPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border &&
            border.Tag is byte[] photo &&
            DataContext is PhotoGalleryViewModel vm)
        {
            vm.OpenPhotoDetailCommand.Execute(photo);
        }
    }
}