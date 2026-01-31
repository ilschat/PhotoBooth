using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PhotoBooth.ViewModels;

public partial class PhotoGalleryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<byte[]> photos;

    public MainViewModel MainVM { get; }

    public PhotoGalleryViewModel(MainViewModel mainVM)
    {
        MainVM = mainVM;
        Photos = new ObservableCollection<byte[]>(mainVM.AllPhotos);
    }

    [RelayCommand]
    private void CloseGallery()
    {
        MainVM.HideGallery();
    }

    [RelayCommand]
    private void OpenPhotoDetail(byte[] photo)
    {
        MainVM.ShowPhotoDetail(photo);
    }
}