using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBooth.Service
{
    public interface ICameraService
    {
        Task StartPreviewAsync();
        Task StopPreviewAsync();

        Task<byte[]> CapturePhotoAsync();

        event Action<byte[]> PreviewFrameReady;
    }
}
