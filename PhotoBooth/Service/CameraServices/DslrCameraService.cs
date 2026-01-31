using Avalonia.Controls;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBooth.Service.CameraServices
{
    internal class DslrCameraService
    {
        //todo sai -> resize preview but take foto with full size -> posible memory peak on pi 
        //var preview = new Mat();
        //Cv2.Resize(fullResFrame, preview, new OpenCvSharp.Size(1920,1080));
        //PreviewFrameReady?.Invoke(preview.ImEncode(".jpg"));

    }
}
