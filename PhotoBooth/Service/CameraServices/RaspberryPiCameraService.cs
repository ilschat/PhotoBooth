using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace PhotoBooth.Service.CameraServices
{
    public class RaspberryPiCameraService : ICameraService
    {
        private VideoCapture? _capture;                  // Video capture object
        private CancellationTokenSource? _cts;           // Used to stop the preview loop

        public event Action<byte[]>? PreviewFrameReady;  // Event for each new frame

        /// <summary>
        /// Startet die Kamera und Preview-Loop (~30 FPS)
        /// </summary>
        public async Task StartPreviewAsync()
        {
            // Pi Camera als V4L2-Gerät
            _capture = new VideoCapture(0); // 0 = /dev/video0 auf Raspberry Pi
            _capture.Open(0);              
            if (!_capture.IsOpened())
                throw new Exception("Failed to open Raspberry Pi camera. Check if camera is connected and accessible.");

            _cts = new CancellationTokenSource();

            await Task.Run(() =>
            {
                var frame = new Mat();
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_capture.Read(frame))
                    {
                        PreviewFrameReady?.Invoke(ConvertFrame(frame));
                    }
                    Thread.Sleep(33); // ~30 FPS
                }
                frame.Dispose();
            }, _cts.Token);
        }

        /// <summary>
        /// Stoppt Preview-Loop und gibt Kamera frei
        /// </summary>
        public Task StopPreviewAsync()
        {
            _cts?.Cancel();
            _capture?.Release();
            _capture?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Einzelbildaufnahme
        /// </summary>
        public Task<byte[]> CapturePhotoAsync()
        {
            var frame = new Mat();
            _capture?.Read(frame);
            var bytes = ConvertFrame(frame);
            frame.Dispose();
            return Task.FromResult(bytes);
        }

        /// <summary>
        /// Mat → JPEG Bytes
        /// </summary>
        private byte[] ConvertFrame(Mat frame)
        {
            return frame.ImEncode(".jpg");
        }
    }
}
