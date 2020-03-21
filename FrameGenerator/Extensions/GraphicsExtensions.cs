﻿using InputParse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FrameGenerator.Extensions
{
    public static class GraphicsExtensions
    {
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y)
        {
            var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }
        public static void WriteCharacter(this Graphics g, string coloredCharacter, Font font, float x, float y, string backgroundColor)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            var brush = new SolidBrush(ColorList.GetColor(coloredCharacter.Substring(1)));
            var color = ColorList.GetColor(backgroundColor);
            if (color.ToArgb() != black.ToArgb())
            {
                if (color.ToArgb() == brown.ToArgb())
                {
                    color = yellow;
                }
                Bitmap backgroundColorbmp = new Bitmap(font.Height, (int)font.Size);
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y);
            }
            g.DrawString(coloredCharacter[0].ToString(), font, brush, x, y);
        }
        public static void PaintBackground(this Graphics g, string backgroundColor, Font font, float x, float y)
        {
            var black = ColorList.GetColor("BLACK");
            var yellow = ColorList.GetColor("YELLOW");
            var brown = ColorList.GetColor("BROWN");
            var color = ColorList.GetColor(backgroundColor);
            if (color.ToArgb() != black.ToArgb())
            {
                if (color.ToArgb() == brown.ToArgb())
                {
                    color = yellow;
                }
                Bitmap backgroundColorbmp = new Bitmap(font.Height, (int)font.Size);//wrong
                Graphics.FromImage(backgroundColorbmp).Clear(color);
                g.DrawImage(backgroundColorbmp, x, y);
            }
        }

        public static Graphics WriteSideDataInfo(this Graphics g, string title, string info, Font font, float x, float y)
        {
            var brown = new SolidBrush(Color.FromArgb(143, 89, 2));
            var gray = new SolidBrush(Color.FromArgb(186, 189, 182));

            g.DrawString(title, font, brown, x, y);
            g.DrawString(info, font, gray, x + g.MeasureString(title, font).Width, y);

            return g;
        }

        public static Graphics DrawPercentageBar(this Graphics g, int amount, int maxAmount, Color barColor, float x, float y)
        {
            Bitmap bar = new Bitmap(250, 16);
            Graphics temp = Graphics.FromImage(bar);
            temp.Clear(Color.Gray);
            g.DrawImage(bar, x, y);

            if (amount > 0)
            {
                int barLength = (int)(250 * ((float)amount / maxAmount));
                bar = new Bitmap(barLength, 16);
                temp = Graphics.FromImage(bar);
                temp.Clear(barColor);
                g.DrawImage(bar, x, y);
                //if (barLength != 250 && prevHP - model.SideData.Health > 0)
                //{
                //    int prevBarLength = (int)(250 * ((float)(prevHP - model.SideData.Health) / model.SideData.Health));
                //    Bitmap losthealthbar = new Bitmap(prevBarLength, 16);
                //    Graphics.FromImage(losthealthbar).Clear(Color.Red);
                //    g.DrawImage(losthealthbar, x + barLength, 40);
                //}
            }

            return g;
        }

        public static bool TryDrawWallOrFloor(this Graphics g, string tile, Bitmap wall, Bitmap floor, float x, float y)
        {
            if (tile == "#BLUE")
            {
                g.DrawImage(wall, x, y, wall.Width, wall.Height);
                var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                g.FillRectangle(blueTint, x, y, wall.Width, wall.Height);
                return true;
            }

            if (tile[0] == '#' && !tile.Equals("#LIGHTCYAN"))
            {
                g.DrawImage(wall, x, y, wall.Width, wall.Height);
                return true;
            }

            if (tile == ".BLUE")
            {
                g.DrawImage(floor, x, y, floor.Width, floor.Height);
                var blueTint = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                g.FillRectangle(blueTint, x, y, floor.Width, floor.Height);
                return true;
            }

            if (tile[0] == '.')
            {
                g.DrawImage(floor, x, y, floor.Width, floor.Height);
                return true;
            }

             if (tile == "*BLUE")
            {
                g.DrawImage(wall, x, y, wall.Width, wall.Height);
                var blueTint = new SolidBrush(Color.FromArgb(40, 30, 30, 200));
                g.FillRectangle(blueTint, x, y, wall.Width, wall.Height);
                return true;
            }
            if (tile == ",BLUE")
            {
                g.DrawImage(floor, x, y, floor.Width, floor.Height);
                var blueTint = new SolidBrush(Color.FromArgb(40, 20, 20, 200));
                g.FillRectangle(blueTint, x, y, floor.Width, floor.Height);
                return true;
            }

            return false;
        }
    }
}
