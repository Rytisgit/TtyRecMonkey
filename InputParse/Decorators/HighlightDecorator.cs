﻿using System;
using InputParser.Abstract;
using InputParser.Constant;
using Putty;
using static InputParser.Constant.Helpers;

namespace InputParser.Decorators
{
    public class HighLightDecorator : AbstractDecorator
    {
        public HighLightDecorator(IParser model) : base(model) { }
        public override Model ParseData(TerminalCharacter[,] chars)
        {
            var parsedModel = base.ParseData(chars);
            parsedModel.Layout = LayoutType.ConsoleFull;
            parsedModel.LineLength = FullWidth;

            var highlight = new string[FullWidth * FullHeight];
            var curentChar = 0;
            try
            {
                for (int j = 0; j < FullHeight; j++)
                {
                    for (int i = 0; i < FullWidth; i++)
                    {
                        highlight[curentChar] = GetBackgroundColor(chars[i, j]);
                        curentChar++;
                    }
                }

                parsedModel.HighlightColors = highlight;
            }
            catch (Exception)
            {
                foreach (var item in chars)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return parsedModel;
        }
    }
}
