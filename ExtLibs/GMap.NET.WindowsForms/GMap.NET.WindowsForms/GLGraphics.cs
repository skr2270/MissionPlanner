using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using GMap.NET.WindowsForms;

namespace MissionPlanner.Controls
{
    public class GLGraphics : GLControl, IGraphics
    {
        public Graphics graphicsObjectGDIP;
        private bool started = false;
        private Image objBitmap = new Bitmap(150, 150);

        Matrix _gdiTransform = new Matrix();
        
        public System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }

        public System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get; set; }

        public GLGraphics()
        {
            opengl = true;

            graphicsObjectGDIP = Graphics.FromImage(objBitmap);

            try
            {

                OpenTK.Graphics.GraphicsMode test = this.GraphicsMode;

                int[] viewPort = new int[4];

                GL.GetInteger(GetPName.Viewport, viewPort);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, Width, Height, 0, -1, 1);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                GL.PushAttrib(AttribMask.DepthBufferBit);
                GL.Disable(EnableCap.DepthTest);
                //GL.Enable(EnableCap.Texture2D); 
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);


                GL.Enable(EnableCap.LineSmooth);
                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.PolygonSmooth);
            }
            catch (Exception ex) {  }

            started = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {

        }
        
        public override void Refresh()
        {
            using (Graphics gg = this.CreateGraphics())
            {
                OnPaint(new PaintEventArgs(gg, this.ClientRectangle));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                string versionString = GL.GetString(StringName.Version);
                string majorString = versionString.Split(' ')[0];
                var v = new Version(majorString);
                npotSupported = v.Major >= 2;
            }
            catch
            {
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                if (opengl)
                {
                    base.OnHandleCreated(e);
                }
            }
            catch (Exception ex) { opengl = false; } // macs/linux fail here
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (opengl)
                {
                    base.OnHandleDestroyed(e);
                }
            }
            catch (Exception ex) { opengl = false; }
        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode || !IsHandleCreated || !started)
                return;

            base.OnResize(e);

            graphicsObjectGDIP = Graphics.FromImage(objBitmap);

            try
            {
                foreach (character texid in charDict.Values)
                {
                    try
                    {
                        texid.bitmap.Dispose();
                    }
                    catch { }
                }
                charDict.Clear();

                if (opengl)
                {
                    foreach (character texid in charDict.Values)
                    {
                        if (texid.gltextureid != 0)
                            GL.DeleteTexture(texid.gltextureid);
                    }
                }
            }
            catch { }

            try
            {
                if (opengl)
                {
                    MakeCurrent();

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, Width, Height, 0, -1, 1);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();

                    GL.Viewport(0, 0, Width, Height);
                }
            }
            catch { }
        }


        public void Clear(Color color)
        {
            if (opengl)
            {
                GL.ClearColor(color);

            }
            else
            {
                graphicsObjectGDIP.Clear(color);
            }
        }

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        public void DrawArc(Pen penn, RectangleF rect, float start, float degrees)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineStrip);

                start = 360 - start;
                start -= 30;

                float x = 0, y = 0;
                for (float i = start; i <= start + degrees; i++)
                {
                    x = (float)Math.Sin(i * deg2rad) * rect.Width / 2;
                    y = (float)Math.Cos(i * deg2rad) * rect.Height / 2;
                    x = x + rect.X + rect.Width / 2;
                    y = y + rect.Y + rect.Height / 2;
                    GL.Vertex2(x, y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawArc(penn, rect, start, degrees);
            }
        }

        public void DrawEllipse(Pen penn, Rectangle rect)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                float x, y;
                for (float i = 0; i < 360; i += 1)
                {
                    x = (float)Math.Sin(i * deg2rad) * rect.Width / 2;
                    y = (float)Math.Cos(i * deg2rad) * rect.Height / 2;
                    x = x + rect.X + rect.Width / 2;
                    y = y + rect.Y + rect.Height / 2;
                    GL.Vertex2(x, y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawEllipse(penn, rect);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.ClearOutputChannelColorProfile();
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        int texture;
        Bitmap bitmap = new Bitmap(512, 512);

        public void DrawImage(Image image, Rectangle rectangle, float srcX, float srcY, float srcWidth, float srcHeight,
            GraphicsUnit graphicsUnit, ImageAttributes tileFlipXYAttributes)
        {
            //graphicsObjectGDIP.DrawImage(image, rectangle, srcX, srcY, srcWidth, srcHeight, graphicsUnit, TileFlipXYAttributes);
            
            DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void DrawImage(Image image, Rectangle rectangle, int p1, int p2, long p3, long p4, GraphicsUnit graphicsUnit, ImageAttributes TileFlipXYAttributes)
        {
            DrawImage(image,rectangle.X,rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void DrawImage(Image img, long x, long y, long width, long height)
        {
            DrawImage(img, (int)x, (int)y, (int)width, (int)height);
        }

        public void DrawImage(Image img, int x, int y, int width, int height)
        {
            if (opengl)
            {
                if (img == null)
                    return;
                //bitmap = new Bitmap(512,512);

                // If the image is already a bitmap and we support NPOT textures then simply use it.
                if (npotSupported && img is Bitmap)
                {
                    bitmap = (Bitmap)img;
                }
                else
                {
                    // Otherwise we have to resize img to be POT.
                    bitmap = ResizeImage(img, bitmap.Width, bitmap.Height);
                }

                GL.DeleteTexture(texture);

                GL.GenTextures(1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);

                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //Console.WriteLine("w {0} h {1}",data.Width, data.Height);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.Enable(EnableCap.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, texture);

                GL.Begin(PrimitiveType.Quads);

                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(x, y+height);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(x+width, y+height);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(x+width, y);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(x,y);

                GL.End();

                GL.Disable(EnableCap.Texture2D);
            }
            else
            {
                graphicsObjectGDIP.DrawImage(img, x, y, width, height);
            }
        }

        public void DrawPath(Pen penn, GraphicsPath gp)
        {
            try
            {
                if (gp.PointCount > 0)
                    DrawPolygon(penn, gp.PathPoints);
            }
            catch { }
        }

        public void FillPath(Brush brushh, GraphicsPath gp)
        {
            try
            {
                FillPolygon(brushh, gp.PathPoints);
            }
            catch { }
        }

        public void SetClip(Rectangle rect)
        {

        }

        public void ResetClip()
        {

        }

        public void FillPolygon(Brush brushh, Point[] list)
        {
            if (opengl)
            {
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Color4(((SolidBrush)brushh).Color);
                foreach (Point pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.Vertex2(list[list.Length - 1].X, list[list.Length - 1].Y);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillPolygon(brushh, list);
            }
        }

        public void FillPolygon(Brush brushh, PointF[] list)
        {
            if (opengl)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Color4(((SolidBrush)brushh).Color);
                foreach (PointF pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.Vertex2(list[0].X, list[0].Y);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillPolygon(brushh, list);
            }
        }

        public void DrawPolygon(Pen penn, Point[] list)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineStrip);
                foreach (Point pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawPolygon(penn, list);
            }
        }

        public void DrawPolygon(Pen penn, PointF[] list)
        {
            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineStrip);
                foreach (PointF pnt in list)
                {
                    GL.Vertex2(pnt.X, pnt.Y);
                }

                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawPolygon(penn, list);
            }
        }


        public void FillRectangle(Brush brushh, long x, long y, long width, long height)
        {
            FillRectangle(brushh, new RectangleF(x, y, width, height));
        }

        public void FillRectangle(Brush brushh, RectangleF rectf)
        {
            if (opengl)
            {
                float x1 = rectf.X;
                float y1 = rectf.Y;

                float width = rectf.Width;
                float height = rectf.Height;

                GL.Begin(PrimitiveType.Quads);

                GL.LineWidth(0);

                if (((Type)brushh.GetType()) == typeof(LinearGradientBrush))
                {
                    LinearGradientBrush temp = (LinearGradientBrush)brushh;
                    GL.Color4(temp.LinearColors[0]);
                }
                else
                {
                    GL.Color4(((SolidBrush)brushh).Color.R / 255f, ((SolidBrush)brushh).Color.G / 255f, ((SolidBrush)brushh).Color.B / 255f, ((SolidBrush)brushh).Color.A / 255f);
                }

                GL.Vertex2(x1, y1);
                GL.Vertex2(x1 + width, y1);

                if (((Type)brushh.GetType()) == typeof(LinearGradientBrush))
                {
                    LinearGradientBrush temp = (LinearGradientBrush)brushh;
                    GL.Color4(temp.LinearColors[1]);
                }
                else
                {
                    GL.Color4(((SolidBrush)brushh).Color.R / 255f, ((SolidBrush)brushh).Color.G / 255f, ((SolidBrush)brushh).Color.B / 255f, ((SolidBrush)brushh).Color.A / 255f);
                }

                GL.Vertex2(x1 + width, y1 + height);
                GL.Vertex2(x1, y1 + height);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.FillRectangle(brushh, rectf);
            }
        }

        public void DrawRectangle(Pen penn, RectangleF rect)
        {
            DrawRectangle(penn, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(Pen penn, double x1, double y1, double width, double height)
        {

            if (opengl)
            {
                GL.LineWidth(penn.Width);
                GL.Color4(penn.Color);

                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(x1, y1);
                GL.Vertex2(x1 + width, y1);
                GL.Vertex2(x1 + width, y1 + height);
                GL.Vertex2(x1, y1 + height);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawRectangle(penn, (float)x1, (float)y1, (float)width, (float)height);
            }
        }

        public void DrawLine(Pen penn, double x1, double y1, double x2, double y2)
        {

            if (opengl)
            {
                GL.Color4(penn.Color);
                GL.LineWidth(penn.Width);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(x1, y1);
                GL.Vertex2(x2, y2);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawLine(penn, (float)x1, (float)y1, (float)x2, (float)y2);
            }
        }


        /// <summary>
        /// pen for drawstring
        /// </summary>
        Pen P = new Pen(Color.FromArgb(0x26, 0x27, 0x28), 2f);
        /// <summary>
        /// pth for drawstring
        /// </summary>
        GraphicsPath pth = new GraphicsPath();

        Dictionary<int, character> charDict = new Dictionary<int, character>();
 

        class character
        {
            public Bitmap bitmap;
            public int gltextureid;
            public int width;
            public int size;
        }


        public void DrawString(string EmptyTileText, System.Drawing.Font MissingDataFont, Brush brush, RectangleF rectangleF, StringFormat CenterFormat)
        {
            //throw new NotImplementedException();
        }

        public void DrawString(string p1, System.Drawing.Font CopyrightFont, Brush brush, int p2, int p3)
        {
            drawstring(p1, Font, Font.SizeInPoints, (SolidBrush)brush, p2, p3);
        }

        public void DrawString(string p, System.Drawing.Font Font, Brush brush, RectangleF rectangleF)
        {
            drawstring(p, Font, Font.SizeInPoints, (SolidBrush)brush, rectangleF.X, rectangleF.Y);
        }

        void drawstring(string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (!opengl)
            {
                drawstring(graphicsObjectGDIP, text, font, fontsize, brush, x, y);
                return;
            }

            if (text == null || text == "")
                return;

            char[] chars = text.ToCharArray();

            float maxy = 1;

            foreach (char cha in chars)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character() { bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb), size = (int)fontsize };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (Graphics gfx = Graphics.FromImage(charDict[charid].bitmap))
                    {
                        pth.Reset();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0), StringFormat.GenericTypographic);

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(P, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);

                    //charbitmaps[charid] = charbitmaps[charid].Clone(new RectangleF(0, 0, maxx + 2, maxy + 2), charbitmaps[charid].PixelFormat);

                    //charbitmaps[charno * (int)fontsize].Save(charno + " " + (int)fontsize + ".png");

                    // create texture
                    int textureId;
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvModeCombine.Replace);//Important, or wrong color on some computers

                    Bitmap bitmap = charDict[charid].bitmap;
                    GL.GenTextures(1, out textureId);
                    GL.BindTexture(TextureTarget.Texture2D, textureId);

                    BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.Finish();
                    bitmap.UnlockBits(data);

                    charDict[charid].gltextureid = textureId;
                }

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, charDict[charid].gltextureid);

                float scale = 1.0f;

                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0, 0); GL.Vertex2(x, y);
                GL.TexCoord2(1, 0); GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y);
                GL.TexCoord2(1, 1); GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y + charDict[charid].bitmap.Height * scale);
                GL.TexCoord2(0, 1); GL.Vertex2(x + 0, y + charDict[charid].bitmap.Height * scale);
                GL.End();

                //GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.Texture2D);

                x += charDict[charid].width * scale;
            }
        }



        public void drawstring(Graphics e, string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (text == null || text == "")
                return;


            char[] chars = text.ToCharArray();

            float maxy = 0;

            foreach (char cha in chars)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character() { bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb), size = (int)fontsize };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (Graphics gfx = Graphics.FromImage(charDict[charid].bitmap))
                    {
                        pth.Reset();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0), StringFormat.GenericTypographic);

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(P, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);
                }

                // draw it

                float scale = 1.0f;

                DrawImage(charDict[charid].bitmap, (int)x, (int)y, charDict[charid].bitmap.Width, charDict[charid].bitmap.Height);

                x += charDict[charid].width * scale;
            }

        }

        public bool opengl { get; set; }


        public SizeF MeasureString(string p, Font Font)
        {
            return new SizeF();
        }

        public InterpolationMode InterpolationMode
        {
            get
            {
                return InterpolationMode.Default;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        public CompositingMode CompositingMode
        {
            get
            {
                return CompositingMode.SourceOver;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        public void FillRectangle(Brush Fill, Rectangle rect)
        {
            FillRectangle(Fill, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawRectangle(Pen Stroke, Rectangle rect)
        {
            DrawRectangle(Stroke, rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void DrawString(string p, Font Font, Brush Foreground, Rectangle rect, StringFormat Format)
        {
            drawstring(p, Font, Font.Size, (SolidBrush)Foreground, (float)rect.X, (float)rect.Y);
        }

        public void DrawRectangle(Pen EmptyTileBorders, int p1, int p2, int p3, int p4)
        {
            DrawRectangle(EmptyTileBorders, (double)p1, p2, p3, p4);
        }


        public void DrawRectangle(Pen SelectionPen, long x1, long y1, long p1, long p2)
        {
            DrawRectangle(SelectionPen, (double)x1, y1, p1, p2);
        }

        public void DrawLine(Pen HelperLinePen, int p1, int p2, int p3, int Height)
        {
            DrawLine(HelperLinePen, (double)p1, p2, p3, Height);
        }


        public void DrawLine(Pen Stroke, int p1, int p2, float p3, float p4)
        {
            DrawLine(Stroke, (double)p1, p2, p3, p4);
        }

        public void DrawLine(Pen Stroke, float midxstart, float midystart, float p1, float p2)
        {
            DrawLine(Stroke, (double)midxstart, midystart, p1, p2);
        }


        SizeF IGraphics.MeasureString(string p, Font Font)
        {
            return graphicsObjectGDIP.MeasureString(p, Font);
        }

        public void DrawImage(Bitmap backBuffer, int p1, int p2)
        {
            throw new NotImplementedException();
        }


        public Matrix Transform
        {
            get { return _gdiTransform; }
            set
            {
                _gdiTransform = value;
                SetTransform(_gdiTransform);
            }
        }

        void SetTransform(Matrix matrix)
        {
            GL.LoadIdentity();

            float x = matrix.Elements[4];
            float y = matrix.Elements[5];
            float xscale = matrix.Elements[0];
            float yscale = matrix.Elements[3];
            float angle = 0;

            if (opengl)
            {
                GL.Translate(x, y, 0f);
            }
            else
            {
                graphicsObjectGDIP.TranslateTransform(x, y);
            }

            if (opengl)
            {
                GL.Scale(xscale, yscale, 1);
            }
            else
            {
                graphicsObjectGDIP.ScaleTransform(xscale, yscale);
            }
    
            if (opengl)
            {
                GL.Rotate(angle, 0, 0, 1);
            }
            else
            {
                graphicsObjectGDIP.RotateTransform(angle);
            }
        }

        public void ScaleTransform(float sx, float sy)
        {
            _gdiTransform.Scale(sx, sy);
            Transform = _gdiTransform;
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            _gdiTransform.Scale(sx, sy, order);
            Transform = _gdiTransform;
        }


        public void RotateTransform(float angle)
        {
            _gdiTransform.Rotate(angle);
            Transform = _gdiTransform;
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            _gdiTransform.Rotate(angle, order);
            Transform = _gdiTransform;
        }

        public void ResetTransform()
        {
            _gdiTransform.Reset();
            Transform = _gdiTransform;

            if (opengl)
            {
                GL.LoadIdentity();
            }
            else
            {
                graphicsObjectGDIP.ResetTransform();
            }
        }

        public void MultiplyTransform(Matrix matrix)
        {
            _gdiTransform.Multiply(matrix);
            Transform = _gdiTransform;
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            _gdiTransform.Multiply(matrix, order);
            Transform = _gdiTransform;
        }


        public void TranslateTransform(float dx, float dy)
        {
            _gdiTransform.Translate(dx, dy);
            Transform = _gdiTransform;
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            _gdiTransform.Translate(dx, dy, order);
            Transform = _gdiTransform;
        }

        public void FillPie(SolidBrush solidBrush, int x, int y, int widtharc, int heightarc, int p1, int p2)
        {
            //throw new NotImplementedException();
        }

        public void FillEllipse(Brush brush, Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public void DrawImageUnscaled(Bitmap localcache2, int p1, int p2)
        {
            DrawImage(localcache2, p1, p2, localcache2.Width, localcache2.Height);
        }

        public void DrawArc(Pen Pen, Rectangle rectangle, int p1, int p2)
        {
            DrawArc(Pen, new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), p1, p2);
            //throw new NotImplementedException();
        }

        public void DrawArc(Pen pen, float p1, float p2, float p3, float p4, float cog, float alpha)
        {
            throw new NotImplementedException();
        }

        public bool npotSupported { get; set; }
    }
}
