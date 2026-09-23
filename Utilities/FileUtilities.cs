/*
 * United Goyim College Fund  Utilities/FileUtilities.cs
 * A stupid shit hole i fuckjing hate this game with over 1000+ mods
 *
 * Copyright (C) 2026  robin williams
 * https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Seralyth.Utilities
{
    public class FileUtilities
    {
        public static string GetFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var cleanName = fileName.Split('?')[0];
            return Path.GetExtension(cleanName).TrimStart('.').ToLowerInvariant();
        }

        public static string RemoveLastDirectory(string directory) =>
            string.IsNullOrEmpty(directory) || directory.LastIndexOf('/') <= 0
                ? string.Empty
                : directory[..directory.LastIndexOf('/')];

        public static string RemoveFileExtension(string file)
        {
            if (string.IsNullOrEmpty(file))
                return string.Empty;

            int index = 0;
            string output = "";
            string[] split = file.Split(".");
            foreach (string data in split)
            {
                index++;
                if (index == split.Length) continue;
                if (index > 1)
                    output += ".";

                output += data;
            }
            return output;
        }

        public static string GetFullPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = "";
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = path == "" ? transform.name : transform.name + "/" + path;
            }
            return path;
        }

        public static string GetGamePath() =>
            AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        public static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "file";

            input = input.Trim();
            char[] illegalChars = Path.GetInvalidFileNameChars();
            input = illegalChars.Aggregate(input, (current, c) => current.Replace(c, '_'));

            input = input.Replace("../", "")
                         .Replace("..\\", "")
                         .Replace("./", "")
                         .Replace(".\\", "");

            input = input.Replace(":", "")
                         .Replace("\\", "")
                         .Replace("/", "");

            if (input.Length > 64)
                input = input[..64];

            if (string.IsNullOrWhiteSpace(input))
                input = "file";

            return input;
        }
    }
}
