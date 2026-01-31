using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoBooth.ViewModels;

namespace PhotoBooth.ViewModels;
public partial class PhotoDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private byte[]? photo;

    public MainViewModel MainVM { get; }

    public PhotoDetailViewModel(MainViewModel mainVM, byte[] photo)
    {
        MainVM = mainVM;
        Photo = photo;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        MainVM.ClosePhotoDetail();
    }
}
