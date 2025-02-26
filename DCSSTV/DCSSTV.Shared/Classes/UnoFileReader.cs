﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using FrameGenerator.FileReading;
using FrameGenerator.Models;

namespace DCSSTV
{
    class UnoFileReader : IReadFromFileAsync
    {
        public async Task<List<StorageFile>> GetPngsFromFolderAndSubfoldersRecursively(StorageFolder folder)
        {
            var files = new List<StorageFile>();
            var subfolders = await folder.GetFoldersAsync();
            foreach (var subFolder in subfolders)
            {
               files.AddRange(await GetPngsFromFolderAndSubfoldersRecursively(subFolder));
            }
            files.AddRange((await folder.GetFilesAsync()).Where(
                file => file.Name.EndsWith("png", true, CultureInfo.InvariantCulture)));
#if DEBUG
            Console.WriteLine($"loaded {files.Count} files from {folder.Name}");
#endif
            return files;
        }

        public async Task<List<StorageFile>> GetFilesFromFolder(string foldername)
        {
            var files = new List<StorageFile>();
            try
            {

                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var folder = await localFolder.GetFolderAsync(foldername);
                files.AddRange((await folder.GetFilesAsync()).Where(
                    file => file.Name.EndsWith("png", true, CultureInfo.InvariantCulture)));
#if DEBUG
                Console.WriteLine($"loaded {files.Count} files from {foldername}");
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return files;
        }


        public async Task<Dictionary<string, string>> GetDictionaryFromFile(string path)
        {

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(path);

            var lines = await FileIO.ReadLinesAsync(file);
            var lineArray = lines.ToArray();

            var dict = new Dictionary<string, string>();

            for (var i = 0; i < lineArray.ToArray().Length; i += 2)
            {
                dict[lineArray[i]] = lineArray[i + 1];
            }

            return dict;
        }

        public async Task<Dictionary<string, string>> GetMonsterData(string filename, string monsterOverrideFile)
        {
            var monster = new Dictionary<string, string>();

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(filename);

            var liness = await FileIO.ReadLinesAsync(file);
            var lines = liness.ToArray();

            foreach (var line in lines)
            {
                if (line.Contains("  MONS_"))
                {
                    string[] tokens = line.Split(',');
                    tokens[1] = tokens[1].Replace("'", "").Replace(" ", "");
                    tokens[2] = tokens[2].Replace(" ", "");
                    tokens[0] = tokens[0].Replace("MONS_", "").Replace(" ", "").ToLower();
                    //if(!Enum.TryParse(tokens[2], out ColorList2 res)) Console.WriteLine(tokens[1] + tokens[2] + " badly colored: " + tokens[0]);
                    //if (monster.TryGetValue(tokens[1] + tokens[2], out var existing))
                    //{
                    //    Console.WriteLine(tokens[1] + tokens[2] + "exist: " + existing + " new: " + tokens[0]); 
                    //}
                    monster[tokens[1] + tokens[2]] = tokens[0];
                }
            }

            //Overrides for duplicates, others handled by name from monster log

            file = await localFolder.GetFileAsync(monsterOverrideFile);
            

            liness = await FileIO.ReadLinesAsync(file);
            lines = liness.ToArray();

            foreach (var line in lines)
            {
                var keyValue = line.Split(' ');
                monster[keyValue[0]] = keyValue[1];
            }

            monster.Remove("8BLUE"); //remove roxanne impersonating statue
            return monster;
        }

        public Task<Dictionary<string, string>> GetWeaponData(string file)
        {
            var weapon = new Dictionary<string, string>();
            return null;
        }


        public async Task<List<NamedMonsterOverride>> GetNamedMonsterOverrideData(string monsterOverrideFile)
        {
            var monster = new List<NamedMonsterOverride>();

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(monsterOverrideFile);


            var liness = await FileIO.ReadLinesAsync(file);
            var lines = liness.ToArray();

            var name = "";
            var location = "";
            var tileNameOverrides = new Dictionary<string, string>(20);

            bool pngParse = false;

            for (var i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    monster.Add(new NamedMonsterOverride(name, location, tileNameOverrides));
                    name = "";
                    location = "";
                    tileNameOverrides = new Dictionary<string, string>(20);
                    pngParse = false;
                    continue;
                }

                if (pngParse)
                {
                    string[] tokens = lines[i].Split(' ');
                    tileNameOverrides.Add(tokens[0], tokens[1]);
                }
                else
                {
                    string[] tokens = lines[i].Split(';');
                    name = tokens[0];
                    location = tokens.Length > 1 ? tokens[1] : "";
                    pngParse = true;
                }
            }

            return monster;
        }


        public async Task<Dictionary<string, Tuple<List<string>, List<string>>>> GetFloorAndWallColours(string filename)
        {
            var floorandwall = new Dictionary<string, Tuple<List<string>, List<string>>>();
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(filename);

            var liness = await FileIO.ReadLinesAsync(file);
            var lines = liness.ToArray();
            var colorListFloor = new List<string>();
            var readingList = new List<string>();
            string name = "";
            for (var i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(name))
                {
                    name = lines[i];
                    continue;
                }
                if (string.IsNullOrWhiteSpace(lines[i]) && colorListFloor.Count < 1)
                {
                    colorListFloor.AddRange(readingList);
                    readingList.Clear();
                    continue;
                }
                if (string.IsNullOrWhiteSpace(lines[i]) && !string.IsNullOrWhiteSpace(name) && colorListFloor.Count > 0)
                {
                    floorandwall[name] = new Tuple<List<string>, List<string>>(new List<string>(colorListFloor), new List<string>(readingList));
                    name = "";
                    colorListFloor.Clear();
                    readingList.Clear();
                    continue;
                }
                readingList.Add(lines[i]);
            }

            return floorandwall;
        }

        public async Task<Dictionary<string, string[]>> GetFloorAndWallNamesForDungeons(string filename)
        {
            var floorandwall = new Dictionary<string, string[]>();
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localFolder.GetFileAsync(filename);

            var liness = await FileIO.ReadLinesAsync(file);
            var lines = liness.ToArray();

            for (var i = 0; i < lines.Length; i += 3)
            {
                string[] temp = new string[2];
                temp[0] = lines[i + 1];
                temp[1] = lines[i + 2];
                floorandwall[lines[i].ToUpper()] = temp;
            }

            return floorandwall;
        }

        public async Task<Dictionary<string, SkiaSharp.SKBitmap>> GetSKBitmapDictionaryFromFolder(string folder, string extraFolder)
        {
            var dict = new Dictionary<string, SkiaSharp.SKBitmap>();
            
            List<StorageFile> pngFiles = await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(folder));
            //Add pngs in Extra folder
            var files = await GetFilesFromFolder(extraFolder);
            pngFiles.AddRange(files);

            foreach (var file in pngFiles)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(buffer.AsStream());

                dict[file.Name.Replace(".png", "")] = SKBitmap;
            }

            return dict;
        }


        public async Task<Dictionary<string, SkiaSharp.SKBitmap>> GetCharacterPNG(string gameLocation)
        {

            var GetCharacterPNG = new Dictionary<string, SkiaSharp.SKBitmap>();

            List<StorageFile> allpngfiles = await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(Path.Combine(gameLocation, "rltiles", "player", "base")));
            allpngfiles.AddRange(await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(Path.Combine(gameLocation, "rltiles", "player", "felids"))));
            foreach (var file in allpngfiles)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(buffer.AsStream());

                GetCharacterPNG[file.Name.Replace(".png", "")] = SKBitmap;

            }
            return GetCharacterPNG;
        }

        public async Task<Dictionary<string, SkiaSharp.SKBitmap>> GetMonsterPNG(string gameLocation)
        {
            var monsterPNG = new Dictionary<string, SkiaSharp.SKBitmap>();
            List<StorageFile> allpngfiles = await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(Path.Combine(gameLocation, "rltiles", "mon")));

            foreach (var file in allpngfiles)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(buffer.AsStream());
                monsterPNG[file.Name.Replace(".png", "")] = SKBitmap;
            }

            return monsterPNG;
        }

        public async Task<Dictionary<string, SkiaSharp.SKBitmap>> GetWeaponPNG(string gameLocation)
        {
            var GetWeaponPNG = new Dictionary<string, SkiaSharp.SKBitmap>();

            List<StorageFile> allpngfiles = await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(Path.Combine(gameLocation, "rltiles", "player", "hand1")));
            allpngfiles.AddRange(await GetPngsFromFolderAndSubfoldersRecursively(
                await ApplicationData.Current.LocalFolder.GetFolderAsync(Path.Combine(gameLocation, "rltiles", "player", "transform"))));

            foreach (var file in allpngfiles)
            {
                var buffer = await FileIO.ReadBufferAsync(file);
                SkiaSharp.SKBitmap SKBitmap = SkiaSharp.SKBitmap.Decode(buffer.AsStream());

                GetWeaponPNG[file.Name.Replace(".png", "")] = SKBitmap;
            }

            return GetWeaponPNG;
        }

        public async Task<MemoryStream> GetFontStream(string name)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
            var bytes = await FileIO.ReadBufferAsync(file);
            return new MemoryStream(bytes.ToArray());
        }
    }
}
