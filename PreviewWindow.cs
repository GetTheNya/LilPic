using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace LilPic;

public class PreviewWindow : Form {
    private SkiaSharp.Views.Desktop.SKControl previewControl;
    private TrackBar slider;
    private Label infoLabel;
    private Panel topPanel;
    private CheckBox modeToggle;
    private Button resetBtn;
    
    private SKBitmap originalBitmap;
    private SKBitmap compressedBitmap;
    private float splitPosition = 0.5f;
    
    // Zoom/Pan State
    private float zoomScale = 1.0f;
    private SKPoint panOffset = new SKPoint(0, 0);
    private bool isDragging = false;
    private Point lastMousePos;
    private bool isSideBySide = false;

    private bool isLoading = true;

    public PreviewWindow(string filePath, int quality, int resizePercent, SKEncodedImageFormat format, 
                         bool stripMetadata, int targetWidth, int targetHeight, long targetFileSize) {
        this.Text = $"Preview: {Path.GetFileName(filePath)}";
        this.Icon = Utils.AppIcon;
        this.Size = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterParent;
        this.DoubleBuffered = true;

        InitializeUI(0, 0);
        
        // Background loading
        Task.Run(() => {
            try {
                var data = File.ReadAllBytes(filePath);
                originalBitmap = SKBitmap.Decode(data);
                
                var compressedBytes = Compressor.CompressImage(data, resizePercent, quality, format, stripMetadata, targetWidth, targetHeight, targetFileSize);
                compressedBitmap = SKBitmap.Decode(compressedBytes);

                this.Invoke(new Action(() => {
                    isLoading = false;
                    infoLabel.Text = $"Original: {Utils.FormatSize(data.Length)} ({originalBitmap.Width}x{originalBitmap.Height})   ➜   Compressed: ~{Utils.FormatSize(compressedBytes.Length)} ({compressedBitmap.Width}x{compressedBitmap.Height})";
                    previewControl.Invalidate();
                }));
            } catch (Exception ex) {
                this.Invoke(new Action(() => {
                    MessageBox.Show($"Failed to generate preview: {ex.Message}");
                    this.Close();
                }));
            }
        });
    }

    private void InitializeUI(long originalSize, long compressedSize) {
        topPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5) };
        
        modeToggle = new CheckBox { 
            Text = "Side-by-Side Mode", 
            AutoSize = true, 
            Location = new Point(10, 10),
            Checked = false 
        };
        modeToggle.CheckedChanged += (s, e) => {
            isSideBySide = modeToggle.Checked;
            slider.Visible = !isSideBySide;
            previewControl.Invalidate();
        };

        resetBtn = new Button { 
            Text = "Reset Zoom/Pan", 
            Location = new Point(150, 7), 
            Size = new Size(120, 25) 
        };
        resetBtn.Click += (s, e) => {
            zoomScale = 1.0f;
            panOffset = new SKPoint(0, 0);
            previewControl.Invalidate();
        };

        Label helpLabel = new Label {
            Text = "Mouse Wheel: Zoom | Drag: Pan",
            AutoSize = true,
            Location = new Point(280, 13),
            ForeColor = Color.Gray
        };

        topPanel.Controls.Add(modeToggle);
        topPanel.Controls.Add(resetBtn);
        topPanel.Controls.Add(helpLabel);
        this.Controls.Add(topPanel);

        infoLabel = new Label { 
            Dock = DockStyle.Bottom, 
            Height = 30, 
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Loading comparison images..."
        };
        this.Controls.Add(infoLabel);

        slider = new TrackBar { 
            Dock = DockStyle.Bottom, 
            Minimum = 0, 
            Maximum = 100, 
            Value = 50 
        };
        slider.Scroll += (s, e) => {
            splitPosition = slider.Value / 100f;
            previewControl.Invalidate();
        };
        this.Controls.Add(slider);

        previewControl = new SKControl { Dock = DockStyle.Fill };
        previewControl.PaintSurface += PreviewControl_PaintSurface;
        previewControl.MouseWheel += PreviewControl_MouseWheel;
        previewControl.MouseDown += PreviewControl_MouseDown;
        previewControl.MouseMove += PreviewControl_MouseMove;
        previewControl.MouseUp += PreviewControl_MouseUp;
        
        this.Controls.Add(previewControl);
    }

    private void PreviewControl_MouseWheel(object sender, MouseEventArgs e) {
        float oldScale = zoomScale;
        if (e.Delta > 0) zoomScale *= 1.15f;
        else zoomScale /= 1.15f;
        
        zoomScale = Math.Clamp(zoomScale, 0.1f, 30f);

        if (oldScale != zoomScale) {
            float ratio = zoomScale / oldScale;
            
            float mouseRelX, mouseRelY;
            
            if (isSideBySide) {
                // In side-by-side mode, zooming should be relative to the center of the hovered half
                float halfWidth = previewControl.Width / 2f;
                if (e.X < halfWidth) {
                    // Left half (Original)
                    mouseRelX = e.X - halfWidth / 2f;
                } else {
                    // Right half (Compressed)
                    mouseRelX = (e.X - halfWidth) - halfWidth / 2f;
                }
                mouseRelY = e.Y - previewControl.Height / 2f;
            } else {
                // In slider mode, zooming is relative to the control center
                mouseRelX = e.X - previewControl.Width / 2f;
                mouseRelY = e.Y - previewControl.Height / 2f;
            }
            
            panOffset.X = mouseRelX - (mouseRelX - panOffset.X) * ratio;
            panOffset.Y = mouseRelY - (mouseRelY - panOffset.Y) * ratio;

            previewControl.Invalidate();
        }
    }

    private void PreviewControl_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Left) {
            isDragging = true;
            lastMousePos = e.Location;
        }
    }

    private void PreviewControl_MouseMove(object sender, MouseEventArgs e) {
        if (isDragging) {
            panOffset.X += e.X - lastMousePos.X;
            panOffset.Y += e.Y - lastMousePos.Y;
            lastMousePos = e.Location;
            previewControl.Invalidate();
        }
    }

    private void PreviewControl_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Left) isDragging = false;
    }

    private void PreviewControl_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.DarkGray);

        if (isLoading || originalBitmap == null || compressedBitmap == null) {
            using var paint = new SKPaint { 
                Color = SKColors.White, 
                IsAntialias = true
            };
#pragma warning disable CS0618
            paint.TextAlign = SKTextAlign.Center;
#pragma warning restore CS0618
            using var font = new SKFont(SKTypeface.Default, 24);
            canvas.DrawText("Calculating Preview...", e.Info.Width / 2f, e.Info.Height / 2f, font, paint);
            return;
        }

        if (isSideBySide) {
            DrawSideBySide(canvas, e.Info);
        } else {
            DrawSliderView(canvas, e.Info);
        }
    }

    private void DrawSliderView(SKCanvas canvas, SKImageInfo info) {
        float scale = Math.Min((float)info.Width / originalBitmap.Width, (float)info.Height / originalBitmap.Height) * zoomScale;
        float x = (info.Width - originalBitmap.Width * scale) / 2 + panOffset.X;
        float y = (info.Height - originalBitmap.Height * scale) / 2 + panOffset.Y;
        var destRect = new SKRect(x, y, x + originalBitmap.Width * scale, y + originalBitmap.Height * scale);

        // Draw compressed image
        canvas.DrawBitmap(compressedBitmap, destRect);

        // Clip and draw original
        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, info.Width * splitPosition, info.Height));
        canvas.DrawBitmap(originalBitmap, destRect);
        canvas.Restore();

        // Split line
        using var paint = new SKPaint { Color = SKColors.White, StrokeWidth = 2 };
        canvas.DrawLine(info.Width * splitPosition, 0, info.Width * splitPosition, info.Height, paint);
    }

    private void DrawSideBySide(SKCanvas canvas, SKImageInfo info) {
        float halfWidth = info.Width / 2f;
        float scale = Math.Min(halfWidth / originalBitmap.Width, (float)info.Height / originalBitmap.Height) * zoomScale;
        
        // Original (Left)
        float x1 = (halfWidth - originalBitmap.Width * scale) / 2 + panOffset.X;
        float y1 = (info.Height - originalBitmap.Height * scale) / 2 + panOffset.Y;
        var rect1 = new SKRect(x1, y1, x1 + originalBitmap.Width * scale, y1 + originalBitmap.Height * scale);
        
        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, halfWidth, info.Height));
        canvas.DrawBitmap(originalBitmap, rect1);
        canvas.Restore();

        // Compressed (Right)
        float x2 = halfWidth + (halfWidth - originalBitmap.Width * scale) / 2 + panOffset.X;
        float y2 = (info.Height - originalBitmap.Height * scale) / 2 + panOffset.Y;
        var rect2 = new SKRect(x2, y2, x2 + originalBitmap.Width * scale, y2 + originalBitmap.Height * scale);

        canvas.Save();
        canvas.ClipRect(new SKRect(halfWidth, 0, info.Width, info.Height));
        canvas.DrawBitmap(compressedBitmap, rect2);
        canvas.Restore();

        // Separator
        using var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 4 };
        canvas.DrawLine(halfWidth, 0, halfWidth, info.Height, paint);
        
        // Labels
        using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        using var font = new SKFont(SKTypeface.Default, 20);
        
        canvas.DrawText("ORIGINAL", 10, 30, font, textPaint);
        canvas.DrawText("COMPRESSED", halfWidth + 10, 30, font, textPaint);
    }


    protected override void Dispose(bool disposing) {
        if (disposing) {
            originalBitmap?.Dispose();
            compressedBitmap?.Dispose();
        }
        base.Dispose(disposing);
    }
}
