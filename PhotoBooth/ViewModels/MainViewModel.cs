using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoBooth.Service;
using PhotoBooth.Service.CameraServices;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace PhotoBooth.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICameraService _cameraService;

    public MainViewModel()
    {
        //Set which Service should be taken ---- 

        //_cameraService = new MockCameraService(); //Mock
        _cameraService = new WebcamCameraService();  //Webcam
        //_cameraService = new RaspberryPiCameraService();
        //_cameraService = new DslrCameraService();
        //_cameraService = new IpCameraService();
        // Event für PreviewFrames abonnieren
        _cameraService.PreviewFrameReady += OnPreviewFrameReady;

        _ = StartPreviewAsync();
    }

    [ObservableProperty]
    private int timerSeconds = 5;

    [ObservableProperty]
    private string countdownText = "";

    [ObservableProperty]
    private bool isCountdownVisible = false;

    [ObservableProperty]
    private bool isAdminMode = false;

    [ObservableProperty]
    private ObservableCollection<byte[]> allPhotos = new();

    [ObservableProperty]
    private bool isGalleryVisible = false;

    [ObservableProperty]
    private bool isPhotoDetailVisible = false;

    [ObservableProperty]
    private byte[]? selectedPhotoDetail;

    [ObservableProperty]
    private PhotoGalleryViewModel? galleryViewModel;

    private int _adminClickCount = 0;
    private DateTime _lastAdminClick = DateTime.MinValue;

    private readonly TimeSpan _adminClickThreshold = TimeSpan.FromSeconds(2);


    private byte[]? _previewImage;
    public byte[]? PreviewImage
    {
        get => _previewImage;
        set
        {
            if (_previewImage != value)
            {
                _previewImage = value;
                OnPropertyChanged();
            }
        }
    }
    [ObservableProperty]
    private byte[]? lastCapturedPhoto;

    [ObservableProperty]
    private double countdownScale = 1.0;

    [RelayCommand]
    private void IncreaseTimer()
    {
        if (TimerSeconds < 15) TimerSeconds++;
    }

    [RelayCommand]
    private void DecreaseTimer()
    {
        if (TimerSeconds > 1) TimerSeconds--;
    }

    [RelayCommand]
    private async Task StartPhotoAsync()
    {
        IsCountdownVisible = true;

        for (int i = TimerSeconds; i > 0; i--)
        {
            CountdownText = i.ToString();
            await AnimateCountdownPop();
            await Task.Delay(1000 - 200);
        }

        IsCountdownVisible = false;

        var photo = await _cameraService.CapturePhotoAsync();
        PreviewImage = photo;
        LastCapturedPhoto = photo;
        await AnimateThumbnailPop();
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PhotoBooth");
        Directory.CreateDirectory(folder);
        string filePath = Path.Combine(folder, $"Photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
        await File.WriteAllBytesAsync(filePath, photo);
    }

    public async Task StartPreviewAsync()
    {
        await _cameraService.StartPreviewAsync();
    }

    public async Task StopPreviewAsync()
    {
        await _cameraService.StopPreviewAsync();
    }

    private void OnPreviewFrameReady(byte[] frame)
    {
        PreviewImage = frame;
    }


    private async Task AnimateCountdownPop()
    {
        double[] keyFrames = { 0.3, 1.4, 1.0 };
        int frameDelay = 70;

        foreach (var scale in keyFrames)
        {
            CountdownScale = scale;
            await Task.Delay(frameDelay);
        }
    }


    [RelayCommand]
    private void AdminButtonClick()
    {
        var now = DateTime.Now;

        if (now - _lastAdminClick > _adminClickThreshold)
            _adminClickCount = 0;

        _adminClickCount++;
        _lastAdminClick = now;

        if (_adminClickCount >= 5)
        {
            _adminClickCount = 0;
            IsAdminMode = true;

            OpenAdminWindow();
        }
    }

    public void ShowPhotoDetail(byte[] photo)
    {
        SelectedPhotoDetail = photo;
        IsPhotoDetailVisible = true;
        IsGalleryVisible = false;
    }

    [RelayCommand]
    public void ClosePhotoDetail()
    {
        IsPhotoDetailVisible = false;
        IsGalleryVisible = true;
    }


    [RelayCommand]
    private void ShowGallery()
    {
        LoadAllPhotos();
        GalleryViewModel = new PhotoGalleryViewModel(this);
        IsGalleryVisible = true;
    }

    public void HideGallery()
    {
        IsGalleryVisible = false;
        GalleryViewModel = null;
    }

    private void LoadAllPhotos()
    {
        AllPhotos.Clear();
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PhotoBooth");
        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder, "*.jpg");
            foreach (var file in files)
            {
                var bytes = File.ReadAllBytes(file);
                AllPhotos.Add(bytes);
            }
        }
    }

    private double _thumbnailScale = 1;
    public double ThumbnailScale
    {
        get => _thumbnailScale;
        set => SetProperty(ref _thumbnailScale, value);
    }

    private async Task AnimateThumbnailPop()
    {
        ThumbnailScale = 1.5;
        await Task.Delay(150);
        ThumbnailScale = 1;
    }

    //Maybe open admin Winow/or setting ... if "?" button was spammed 5 times.. todo 
    private void OpenAdminWindow()
    {

    }

}
