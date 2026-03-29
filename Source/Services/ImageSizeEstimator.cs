using System;
using SkiaSharp;

namespace LilPic.Services;

public static class ImageSizeEstimator {
    /// <summary>
    /// Uses 2-point interpolation to estimate the quality needed to hit a target size.
    /// Reuses existing CompressImage logic but doesn't save to disk.
    /// </summary>
    public static int EstimateQuality(byte[] originalData, long targetBytes, int resizePercent, SKEncodedImageFormat format, bool stripMetadata) {
        if (targetBytes <= 0) return 80;

        // Pass 1: High quality
        long sHi = GetCompressedSize(originalData, resizePercent, 90, format, stripMetadata);
        if (sHi <= targetBytes) return 90;

        // Pass 2: Low quality
        long sLo = GetCompressedSize(originalData, resizePercent, 15, format, stripMetadata);
        if (sLo >= targetBytes) return 15;

        // Interpolate linearly on a log scale (sizes usually scale somewhat exponentially with quality)
        // log(size) = a * quality + b
        // a = (log(sHi) - log(sLo)) / (90 - 15)
        double logHi = Math.Log(sHi);
        double logLo = Math.Log(sLo);
        double targetLog = Math.Log(targetBytes);

        double q = 15 + (targetLog - logLo) / (logHi - logLo) * (90 - 15);
        int finalQ = (int)Math.Clamp(q, 10, 100);

        return finalQ;
    }

    private static long GetCompressedSize(byte[] data, int percent, int quality, SKEncodedImageFormat format, bool stripMetadata) {
        // We call the real compressor but just return the length of the array
        // This is fast since it's just in-memory.
        // We use a simplified version of Compressor.CompressImage to avoid dependencies
        // Actually, we can just use Compressor.CompressImage if we make it public enough or move it here.
        // Let's assume we'll use actual logic to be 100% accurate.
        return GenericCompress(data, percent, quality, format, stripMetadata).Length;
    }

    // Copy of Compressor.CompressImage but simplified for pure estimation
    private static byte[] GenericCompress(byte[] imageData, int percent, int quality, SKEncodedImageFormat format, bool stripMetadata) {
        using SKBitmap sourceBitmap = SKBitmap.Decode(imageData);
        if (sourceBitmap == null) return imageData;

        int targetWidth = Math.Max(1, (int)Math.Floor(sourceBitmap.Width / 100f * percent));
        int targetHeight = Math.Max(1, (int)Math.Floor(sourceBitmap.Height / 100f * percent));

        using SKBitmap resizedBitmap = sourceBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), SKSamplingOptions.Default);
        if (resizedBitmap == null) return imageData;

        using SKData encodedData = resizedBitmap.Encode(format, quality);
        if (encodedData == null) return imageData;

        return encodedData.ToArray();
    }
}
