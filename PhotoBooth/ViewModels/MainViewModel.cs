using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoBooth.Service;
using PhotoBooth.Service.CameraServices;
using System;
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
    [RelayCommand]
    private void IncreaseTimer()
    {
        TimerSeconds++;
    }

    [RelayCommand]
    private void DecreaseTimer()
    {
        if (TimerSeconds > 1)
            TimerSeconds--;
    }

    [RelayCommand]
    private async Task StartPhotoAsync()
    {
        var photo = await _cameraService.CapturePhotoAsync();
        PreviewImage = photo;
        LastCapturedPhoto = photo;

        // Optional: Foto direkt speichern
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PhotoBooth"); //Set specific Path for Photos.. now it is in "c:/Users/Pictures/Photobooth"
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
        PreviewImage = frame; // Avalonia UI bindet an PreviewImage
    }
}
