using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBooth.Service.CameraServices
{
    public class MockCameraService : ICameraService
    {
        public event Action<byte[]> PreviewFrameReady;

        private Timer? _timer;

        public Task StartPreviewAsync()
        {
            _timer = new Timer(_ =>
            {
                PreviewFrameReady?.Invoke(RandomImage());
            }, null, 0, 500);

            return Task.CompletedTask;
        }

        public Task StopPreviewAsync()
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public Task<byte[]> CapturePhotoAsync()
        {
            return Task.FromResult(RandomImage());
        }

        private byte[] RandomImage()
        {
            // Path to Assets.. i just set some random test image... 
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "64508202-test-written-by-hand-hand-writing-on-transparent-board-photo.jpg");

            if (!File.Exists(path))
                throw new FileNotFoundException("Mock image not found", path);

            return File.ReadAllBytes(path);
        }
    }

}
