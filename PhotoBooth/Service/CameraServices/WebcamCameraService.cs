using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace PhotoBooth.Service.CameraServices
{
    // Cross-platform webcam service using OpenCvSharp.
    // Works on Windows, Linux (Raspberry Pi), and macOS with a connected webcam.
    // Requires OpenCvSharp4 and OpenCvSharp4.runtime.anycpu for multi-platform support.

    public class WebcamCameraService : ICameraService
    {
        private VideoCapture? _capture;                  // Video capture object (nullable for nullability checks)
        private CancellationTokenSource? _cts;           // Used to stop the preview loop

        public event Action<byte[]>? PreviewFrameReady;  // Event raised for each new preview frame (nullable)

        /// <summary>
        /// Starts the camera preview asynchronously.
        /// Continuously reads frames from the webcam and raises the PreviewFrameReady event.
        /// Can be stopped by calling StopPreviewAsync().
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the webcam cannot be opened.</exception>
        public async Task StartPreviewAsync()
        {
            _capture = new VideoCapture(0); // 0 = first available camera (Windows/Linux/macOS)
            if (!_capture.IsOpened())
                throw new Exception("Failed to open webcam. Check if camera is connected and accessible.");

            _cts = new CancellationTokenSource();
            int fps = 30;
            int delay = 1000 / fps;
            await Task.Run(() =>
            {
                using var frame = new Mat();
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_capture.Read(frame))
                    {
                        var bytes = frame.ImEncode(".jpg");
                        PreviewFrameReady?.Invoke(bytes);
                    }
                    Thread.Sleep(delay);
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Stops the camera preview and releases all camera resources.
        /// Cancels the running preview loop.
        /// </summary>
        public Task StopPreviewAsync()
        {
            _cts?.Cancel();
            _capture?.Release();
            _capture?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Captures a single photo from the webcam and returns it as a JPEG byte array.
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
        /// Converts an OpenCvSharp Mat frame to a JPEG byte array.
        /// </summary>
        private byte[] ConvertFrame(Mat frame)
        {
            return frame.ImEncode(".jpg");
        }
    }
}
