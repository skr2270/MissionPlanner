using System.Drawing;

namespace GMap.NET.WindowsForms
{
    public interface IGraphics
    {
        void TranslateTransform(float p1, float p2, System.Drawing.Drawing2D.MatrixOrder matrixOrder);

        void ScaleTransform(float p1, float p2, System.Drawing.Drawing2D.MatrixOrder matrixOrder);

        void ResetTransform();

        void TranslateTransform(float p1, float p2);

        void RotateTransform(float p);

        System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }

        System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get; set; }

        SizeF MeasureString(string p, System.Drawing.Font Font);

        System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set; }

        System.Drawing.Drawing2D.CompositingMode CompositingMode { get; set; }

        void FillRectangle(System.Drawing.Brush Fill, System.Drawing.Rectangle rect);

        void DrawRectangle(System.Drawing.Pen Stroke, System.Drawing.Rectangle rect);

        void DrawString(string p, System.Drawing.Font Font, System.Drawing.Brush Foreground, System.Drawing.Rectangle rect, System.Drawing.StringFormat Format);

        void FillRectangle(System.Drawing.Brush EmptytileBrush, System.Drawing.RectangleF rectangleF);

        void DrawString(string p, System.Drawing.Font MissingDataFont, System.Drawing.Brush brush, System.Drawing.RectangleF rectangleF);

        void DrawString(string EmptyTileText, System.Drawing.Font MissingDataFont, System.Drawing.Brush brush, System.Drawing.RectangleF rectangleF, System.Drawing.StringFormat CenterFormat);

        void DrawRectangle(System.Drawing.Pen EmptyTileBorders, int p1, int p2, int p3, int p4);

        void DrawString(string p1, System.Drawing.Font CopyrightFont, System.Drawing.Brush brush, int p2, int p3);

        void DrawRectangle(System.Drawing.Pen SelectionPen, long x1, long y1, long p1, long p2);

        void FillRectangle(System.Drawing.Brush SelectedAreaFill, long x1, long y1, long p1, long p2);

        void DrawLine(System.Drawing.Pen Stroke, double p1, double p2, double p3, double p4);

        void FillPath(Brush Fill, System.Drawing.Drawing2D.GraphicsPath gp);

        void DrawPath(Pen pen, System.Drawing.Drawing2D.GraphicsPath gp);

        void DrawImage(Image image, int p1, int p2, int p3, int p4);

        void DrawImage(Image image, Rectangle rectangle, int p1, int p2, long p3, long p4, GraphicsUnit graphicsUnit, System.Drawing.Imaging.ImageAttributes TileFlipXYAttributes);

        void DrawImage(Image image, long p1, long p2, long p3, long p4);

        void DrawImage(Image image, Rectangle dst, float p1, float p2, float p3, float p4, GraphicsUnit graphicsUnit, System.Drawing.Imaging.ImageAttributes TileFlipXYAttributes);

        void DrawImage(Bitmap backBuffer, int p1, int p2);

        void Clear(Color EmptyMapBackground);

        void Dispose();

        System.Drawing.Drawing2D.Matrix Transform { get; set; }

        void FillPie(SolidBrush solidBrush, int x, int y, int widtharc, int heightarc, int p1, int p2);

        void FillEllipse(Brush brush, Rectangle rectangle);

        void DrawImageUnscaled(Bitmap localcache2, int p1, int p2);

        void DrawArc(Pen Pen, Rectangle rectangle, int p1, int p2);

        void DrawArc(Pen pen, float p1, float p2, float p3, float p4, float cog, float alpha);
    }
}
