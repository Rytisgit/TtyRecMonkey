﻿using System;
using System.Text;
using TtyRecMonkey;

namespace InputParse
{
    public class Parser
    {
        private static char GetCharacter(Putty.TerminalCharacter character)
        {
            return character.Character == 55328 ? ' ' : character.Character;
        }
        public static LayoutType GetLayoutType(Putty.TerminalCharacter[,] characters)
        {
            StringBuilder place = new StringBuilder();
            bool found = false;

            string sideLocation;
            for (int i = 61; i < 75; i++)
            {
                place.Append(GetCharacter(characters[i, 7]));
            }
            sideLocation = place.ToString();
            foreach (var location in Locations.locations)
            {
                if (sideLocation.Contains(location.Substring(0, 3)))
                {
                    sideLocation = location;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return LayoutType.Normal;
            }
            place = new StringBuilder();
            string mapLocation;
            for (int i = 0; i < 30; i++)
            {
                place.Append(GetCharacter(characters[i, 7]));
            }
            mapLocation = place.ToString();
            
            foreach (var location in Locations.locations)
            {
                if(mapLocation.Contains(location.Substring(0, 3)))
                {
                    mapLocation = location;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                return LayoutType.MapOnly;
            }
            return LayoutType.TextOnly;
        }
        const int FullWidth = 75;
        const int GameViewWidth = 33;
        const int GameViewHeight = 17;
        public static Model ParseData(Putty.TerminalCharacter[,] chars)
        {
            var characters = chars;
            if (characters == null) return null;
            
            var layout = GetLayoutType(characters);
            switch (layout)
            {
                case LayoutType.Normal:
                    return parseNormalLayout(characters);
                case LayoutType.TextOnly:
                    return parseTextLayout(characters);
                case LayoutType.MapOnly:
                    return parseMapLayout(characters);
            }
            return new Model();          
            //foreach (var item in coloredStrings)
            //{
            //    Console.Write('"' + item + "\", ");
            //}
            //Console.WriteLine();
            //model.HighlightColors = highlightColorStrings;
            //foreach (var item in highlightColorStrings)
            //{
            //    Console.Write('"' + item + "\", ");
            //}
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();
            }

        private static Model parseNormalLayout(Putty.TerminalCharacter[,] characters)
        {
            Model model = new Model();
            model.Layout = LayoutType.Normal;
            model.LineLength = GameViewWidth;
            var coloredStrings = new string[model.LineLength * GameViewHeight];
            var highlightColorStrings = new string[model.LineLength * GameViewHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < GameViewHeight; j++)
                    for (int i = 0; i < model.LineLength; i++)
                    {
                        coloredStrings[curentChar] = GetCharacter(characters[i, j]) + Enum.GetName(typeof(ColorList2), characters[i, j].ForegroundPaletteIndex);
                        highlightColorStrings[curentChar] = Enum.GetName(typeof(ColorList2), characters[i, j].BackgroundPaletteIndex);
                        curentChar++;
                    }
                model.TileNames = coloredStrings;

                model.SideData = ParseSideData(characters);

                StringBuilder logLine1 = new StringBuilder();
                StringBuilder logLine2 = new StringBuilder();
                StringBuilder logLine3 = new StringBuilder();
                StringBuilder logLine4 = new StringBuilder();
                StringBuilder logLine5 = new StringBuilder();
                StringBuilder logLine6 = new StringBuilder();
                for (int i = 0; i < 75; i++)
                {
                    logLine1.Append(GetCharacter(characters[i, 17]));
                    logLine2.Append(GetCharacter(characters[i, 18]));
                    logLine3.Append(GetCharacter(characters[i, 19]));
                    logLine4.Append(GetCharacter(characters[i, 20]));
                    logLine5.Append(GetCharacter(characters[i, 21]));
                    logLine6.Append(GetCharacter(characters[i, 22]));
                }

                model.FullLengthStrings = new string[6] { logLine1.ToString(), logLine2.ToString(), logLine3.ToString(), logLine4.ToString(), logLine5.ToString(), logLine6.ToString() };
                model.FullLengthStringColors = new string[6] { Enum.GetName(typeof(ColorList2), characters[1, 18].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2), characters[1, 19].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2), characters[1, 20].ForegroundPaletteIndex),
                Enum.GetName(typeof(ColorList2),characters[1, 21].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2),characters[1, 22].ForegroundPaletteIndex), Enum.GetName(typeof(ColorList2),characters[1, 23].ForegroundPaletteIndex), };


            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }

        private static Model parseMapLayout(Putty.TerminalCharacter[,] characters)
        {
            Model model = new Model();
            model.Layout = LayoutType.MapOnly;
            model.LineLength = FullWidth;
            var coloredStrings = new string[model.LineLength * GameViewHeight];
            var curentChar = 0;
            try
            {

                for (int j = 0; j < GameViewHeight; j++)
                    for (int i = 0; i < model.LineLength; i++)
                    {
                        coloredStrings[curentChar] = GetCharacter(characters[i, j]) + Enum.GetName(typeof(ColorList2), characters[i, j].ForegroundPaletteIndex);
                        curentChar++;
                    }
                model.TileNames = coloredStrings;

                model.SideData = new SideData();
                StringBuilder place = new StringBuilder();
                for (int i = 61; i < 75; i++)
                {
                    place.Append(GetCharacter(characters[i, 7]));

                }
                model.SideData.Place = place.ToString().Trim();

            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }

        private static Model parseTextLayout(Putty.TerminalCharacter[,] characters)
        {
            Model model = new Model();
            model.Layout = LayoutType.TextOnly;
            model.LineLength = FullWidth;
            var coloredStrings = new string[model.LineLength * GameViewHeight];
            var curentChar = 0;
            try
            {
                for (int j = 0; j < GameViewHeight; j++)
                    for (int i = 0; i < model.LineLength; i++)
                    {
                        coloredStrings[curentChar] = GetCharacter(characters[i, j]) + Enum.GetName(typeof(ColorList2), characters[i, j].ForegroundPaletteIndex);
                        curentChar++;
                    }
                model.TileNames = coloredStrings;

            }
            catch (Exception)
            {
                foreach (var item in characters)
                {
                    if (item.ForegroundPaletteIndex > 15) Console.WriteLine(item.ForegroundPaletteIndex + item.ForegroundPaletteIndex);
                }

                return new Model();
            }
            return model;
        }


        private static SideData ParseSideData(Putty.TerminalCharacter[,] characters)
        {
            var sideData = new SideData();
            StringBuilder name = new StringBuilder();
            StringBuilder race = new StringBuilder();
            StringBuilder weapon = new StringBuilder();
            StringBuilder quiver = new StringBuilder();
            StringBuilder status = new StringBuilder();
            StringBuilder ac = new StringBuilder();
            StringBuilder ev = new StringBuilder();
            StringBuilder sh = new StringBuilder();
            StringBuilder xl = new StringBuilder();
            StringBuilder str = new StringBuilder();
            StringBuilder @int = new StringBuilder();
            StringBuilder dex = new StringBuilder();
            StringBuilder hp = new StringBuilder();
            StringBuilder mp = new StringBuilder();
            StringBuilder place = new StringBuilder();
            StringBuilder time = new StringBuilder();
            for (int i = 37; i < 75; i++)
            {
                name.Append(GetCharacter(characters[i, 0]));
                race.Append(GetCharacter(characters[i, 1]));
                weapon.Append(GetCharacter(characters[i, 9]));
                quiver.Append(GetCharacter(characters[i, 10]));
                status.Append(GetCharacter(characters[i, 11]));
            }
            for (int i = 40; i < 44; i++)
            {
                ac.Append(GetCharacter(characters[i, 4]));
                ev.Append(GetCharacter(characters[i, 5]));
                sh.Append(GetCharacter(characters[i, 6]));
                xl.Append(GetCharacter(characters[i, 7]));

            }
            for (int i = 59; i < 63; i++)
            {
                str.Append(GetCharacter(characters[i, 4]));
                @int.Append(GetCharacter(characters[i, 5]));
                dex.Append(GetCharacter(characters[i, 6]));

            }
            for (int i = 37; i < 52; i++)
            {
                hp.Append(GetCharacter(characters[i + 1, 2]));
                mp.Append(GetCharacter(characters[i, 3]));

            }
            for (int i = 60; i < 75; i++)
            {
                place.Append(GetCharacter(characters[i + 1, 7]));
                time.Append(GetCharacter(characters[i, 8]));

            }

            var splithp = hp.ToString().Split(':').Length > 1 ? hp.ToString().Split(':')[1].Split('/') : new string[] {"1","1"};
            var splitmp = mp.ToString().Split(':').Length > 1 ? mp.ToString().Split(':')[1].Split('/') : new string[] { "1", "1" };


            if (splithp.Length > 1)
            {
                sideData.Health = int.Parse(splithp[0]);
                var truehp = splithp[1].Split(' ');
                sideData.MaxHealth = int.Parse(truehp[0]);
                sideData.TrueHealth = truehp.Length > 1 ? truehp[1] : "";
            }
            if (splitmp.Length > 1)
            {
                sideData.Magic = int.Parse(splitmp[0]);
                var truemp = splitmp[1].Split(' ');
                sideData.MaxMagic = int.Parse(truemp[0]);
                sideData.TrueHealth = truemp.Length > 1 ? truemp[1] : "";

            }
            sideData.Name = name.ToString();
            sideData.Race = race.ToString();
            sideData.Weapon = weapon.ToString();
            sideData.Quiver = quiver.ToString();
            sideData.Statuses1 = status.ToString();
            sideData.Statuses2 = "";
            sideData.ArmourClass = ac.ToString();
            sideData.Evasion = ev.ToString();
            sideData.Shield = sh.ToString();
            sideData.ExperienceLevel = xl.ToString();
            sideData.Strength = str.ToString();
            sideData.Inteligence = @int.ToString();
            sideData.Dexterity = dex.ToString();
            sideData.Place = place.ToString().Trim();
            sideData.Time = time.ToString();

            return sideData;
        }
    }
}
