// Copyright 2017 Louis S. Berman.
//
// This file is part of OandaTicks.
//
// OandaTicks is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OandaTicks is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OandaTicks.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OandaTicks.Common.Helpers
{
    public static class StringExtenders
    {
        public static bool IsFileName(this string value, bool mustBeRooted = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                var dummy = new FileInfo(value);

                return !mustBeRooted || Path.IsPathRooted(value);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }


        public static List<string> Wrap(this string text, int margin)
        {
            int start = 0;

            int end;

            var lines = new List<string>();

            text = text.Trim();

            while ((end = start + margin) < text.Length)
            {
                while ((text[end]) != ' ' && (end > start))
                    end -= 1;

                if (end == start)
                    end = start + margin;

                lines.Add(text.Substring(start, end - start));

                start = end + 1;
            }

            if (start < text.Length)
                lines.Add(text.Substring(start));

            return lines;
        }

        public static void EnsurePathExists(this string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static T ToEnum<T>(this string value) =>
            (T)Enum.Parse(typeof(T), value);

        public static bool IsGuid(this string value) =>
            Guid.TryParse(value, out Guid guid);

        public static bool InChars(this string value, string chars) =>
            value.All(c => chars.Contains(c));

        public static bool IsTrimmed(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            if (char.IsWhiteSpace(value[0]))
                return false;

            if (char.IsWhiteSpace(value[value.Length - 1]))
                return false;

            return true;
        }

        public static string Funcify(
            this string value, Func<string, string> func) => func(value);
    }
}
