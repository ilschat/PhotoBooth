using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBooth.Service.CameraServices
{
    public class RaspberryPiCameraService : ICameraService
    {
        private Process? _ffmpegProcess;
        private CancellationTokenSource? _cts;

        private byte[]? _lastFrame;

        public event Action<byte[]>? PreviewFrameReady;

        public Task StartPreviewAsync()
        {
            // Bereits gestartet?
            if (_ffmpegProcess != null &&
                !_ffmpegProcess.HasExited)
            {
                return Task.CompletedTask;
            }

            _cts = new CancellationTokenSource();

            _ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",

                    Arguments =
                        "-fflags nobuffer " +
                        "-f v4l2 " +
                        "-video_size 640x480 " +
                        "-framerate 30 " +
                        "-i /dev/video1 " +
                        "-vf fps=15 " +
                        "-c:v mjpeg " +
                        "-q:v 7 " +
                        "-f image2pipe -",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _ffmpegProcess.Start();

            // stderr lesen damit ffmpeg nicht blockiert
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_ffmpegProcess.HasExited)
                    {
                        var line =
                            await _ffmpegProcess
                                .StandardError
                                .ReadLineAsync();

                        if (line == null)
                            break;

                        Console.WriteLine(line);
                    }
                }
                catch
                {
                }
            });

            // Frames lesen
            _ = Task.Run(async () =>
            {
                try
                {
                    await ReadFramesAsync(
                        _ffmpegProcess.StandardOutput.BaseStream,
                        _cts.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });

            return Task.CompletedTask;
        }

        private async Task ReadFramesAsync(
            Stream stream,
            CancellationToken token)
        {
            var buffer = new byte[8192];

            MemoryStream? currentFrame = null;

            bool insideFrame = false;

            byte prevByte = 0;

            while (!token.IsCancellationRequested)
            {
                int read = await stream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length,
                    token);

                if (read <= 0)
                {
                    await Task.Delay(1, token);
                    continue;
                }

                for (int i = 0; i < read; i++)
                {
                    byte currentByte = buffer[i];

                    // JPEG START
                    if (!insideFrame &&
                        prevByte == 0xFF &&
                        currentByte == 0xD8)
                    {
                        insideFrame = true;

                        currentFrame = new MemoryStream();

                        currentFrame.WriteByte(0xFF);
                        currentFrame.WriteByte(0xD8);
                    }
                    else if (insideFrame)
                    {
                        currentFrame!.WriteByte(currentByte);

                        // JPEG ENDE
                        if (prevByte == 0xFF &&
                            currentByte == 0xD9)
                        {
                            var frame =
                                currentFrame.ToArray();

                            _lastFrame = frame;

                            PreviewFrameReady?.Invoke(frame);

                            currentFrame.Dispose();
                            currentFrame = null;

                            insideFrame = false;
                        }
                    }

                    prevByte = currentByte;
                }
            }
        }

        public Task StopPreviewAsync()
        {
            try
            {
                _cts?.Cancel();

                if (_ffmpegProcess != null)
                {
                    if (!_ffmpegProcess.HasExited)
                    {
                        _ffmpegProcess.Kill(true);

                        _ffmpegProcess.WaitForExit();
                    }

                    _ffmpegProcess.Dispose();
                    _ffmpegProcess = null;
                }
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        public Task<byte[]> CapturePhotoAsync()
        {
            if (_lastFrame == null)
                throw new Exception(
                    "Kein Kamerabild vorhanden.");

            return Task.FromResult(_lastFrame);
        }
    }
}