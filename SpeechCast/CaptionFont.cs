using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;

namespace SpeechCast
{
    [Serializable]
    public class CaptionFont
    {
        [Serializable]
        public struct CaptionColors
        {
            public Color HighForeColor;
            public Color LowForeColor;
            public Color BorderColor;
            public Color TransparentColor;
            public void Serialize()
            {
                ColorConverter cc = new ColorConverter();

                HighForeColorString = cc.ConvertToString(HighForeColor);
                LowForeColorString = cc.ConvertToString(LowForeColor);
                BorderColorString = cc.ConvertToString(BorderColor);
                TransparentColorString = cc.ConvertToString(TransparentColor);
            }

            public void Deserialize()
            {
                ColorConverter cc = new ColorConverter();

                if (!string.IsNullOrEmpty(HighForeColorString))
                {
                    HighForeColor = (Color)cc.ConvertFromString(HighForeColorString);
                }

                if (!string.IsNullOrEmpty(LowForeColorString))
                {
                    LowForeColor = (Color)cc.ConvertFromString(LowForeColorString);
                }

                if (!string.IsNullOrEmpty(BorderColorString))
                {
                    BorderColor = (Color)cc.ConvertFromString(BorderColorString);
                }

                if (!string.IsNullOrEmpty(TransparentColorString))
                {
                    TransparentColor = (Color)cc.ConvertFromString(TransparentColorString);
                }
            }


            public string HighForeColorString;
            public string LowForeColorString;
            public string BorderColorString;
            public string TransparentColorString;

        }

        public string Name = "MS UI Gothic";
        public float Size = 30;
        public int BorderSize = 3;
        public bool Italic = false;
        public bool Bold = false;
        public bool IsDirectionVertical = false;
        public bool IsInitialized = false;
        public CaptionColors Colors;


        public CaptionFont()
        {
        }

        public CaptionFont(CaptionFont captionFont)
        {
            Name = captionFont.Name;
            Size = captionFont.Size;
            BorderSize = captionFont.BorderSize;
            Italic = captionFont.Italic;
            Bold = captionFont.Bold;
            IsDirectionVertical = captionFont.IsDirectionVertical;
            IsInitialized = captionFont.IsInitialized;
            //HighForeColor = captionFont.HighForeColor;
            //LowForeColor = captionFont.LowForeColor;
            //BorderColor = captionFont.HighForeColor;
            //this.Colors = new CaptionColors(captionFont.Colors);
            this.Colors = captionFont.Colors;
            foreBrush = captionFont.foreBrush;
            borderBrush = captionFont.BorderBrush;
        }

        public Font CreateFont()
        {
            FontStyle fs = FontStyle.Regular;

            if (Italic)
            {
                fs |= FontStyle.Italic;
            }
            if (Bold)
            {
                fs |= FontStyle.Bold;
            }

            //Size = (float)((int)Size);
            return new Font(Name, Size, fs);
        }

        private Brush borderBrush = null;
        private Brush foreBrush = null;

        public Brush BorderBrush
        {
            get
            {
                if (borderBrush == null)
                {
                    borderBrush = new SolidBrush(Colors.BorderColor);
                }
                return borderBrush;

            }
        }

        public Brush GetForeBrush(Rectangle captionRect)
        {
            if (foreBrush == null)
            {
                RectangleF rect = new RectangleF();
                LinearGradientMode mode;
                rect.X = (float)captionRect.X;
                rect.Y = (float)captionRect.Y - 1.0f;
                if (this.IsDirectionVertical)
                {
                    rect.Width = (float)captionRect.Width;
                    rect.Height = (float)captionRect.Height;
                    mode = LinearGradientMode.Vertical;
                }
                else
                {
                    rect.Width = (float)captionRect.Width;
                    rect.Height = CreateFont().GetHeight();
                    mode = LinearGradientMode.Vertical;
                }
                foreBrush = new LinearGradientBrush(rect, Colors.HighForeColor, Colors.LowForeColor, mode);
            }
            return foreBrush;
        }

        public void ClearBrushCache()
        {
            borderBrush = null;
            foreBrush = null;
        }

    }
}
