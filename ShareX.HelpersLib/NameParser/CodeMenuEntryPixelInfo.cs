﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2020 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShareX.HelpersLib
{
    public class CodeMenuEntryPixelInfo : CodeMenuEntry
    {
        // We need access to the prefix later on
        private static readonly string _prefix = "$";
        protected override string Prefix { get; } = _prefix;


        // This shouldn't show up in the list of options, but will continue to work for backwards compatibility's sake.
        //public static readonly CodeMenuEntryPixelInfo r = new CodeMenuEntryPixelInfo("r", "Red color (0-255)");
        //public static readonly CodeMenuEntryPixelInfo g = new CodeMenuEntryPixelInfo("g", "Green color (0-255)");
        //public static readonly CodeMenuEntryPixelInfo b = new CodeMenuEntryPixelInfo("b", "Blue color (0-255)");

        public static readonly CodeMenuEntryPixelInfo r255 = new CodeMenuEntryPixelInfo("r255", "Red color (0-255)");
        public static readonly CodeMenuEntryPixelInfo g255 = new CodeMenuEntryPixelInfo("g255", "Green color (0-255)");
        public static readonly CodeMenuEntryPixelInfo b255 = new CodeMenuEntryPixelInfo("b255", "Blue color (0-255)");
        public static readonly CodeMenuEntryPixelInfo r1 = new CodeMenuEntryPixelInfo("r1", "Red color (0-1). Specify decimal precison with {n}, defaults to 3.");
        public static readonly CodeMenuEntryPixelInfo g1 = new CodeMenuEntryPixelInfo("g1", "Green color (0-1). Specify decimal precison with {n}, defaults to 3.");
        public static readonly CodeMenuEntryPixelInfo b1 = new CodeMenuEntryPixelInfo("b1", "Blue color (0-1). Specify decimal precison with {n}, defaults to 3.");
        public static readonly CodeMenuEntryPixelInfo hex = new CodeMenuEntryPixelInfo("hex", "Hex color value (Lowercase)");
        public static readonly CodeMenuEntryPixelInfo HEX = new CodeMenuEntryPixelInfo("HEX", "Hex color value (Uppercase)");
        public static readonly CodeMenuEntryPixelInfo x = new CodeMenuEntryPixelInfo("x", "X position");
        public static readonly CodeMenuEntryPixelInfo y = new CodeMenuEntryPixelInfo("y", "Y position");
        public static readonly CodeMenuEntryPixelInfo n = new CodeMenuEntryPixelInfo("n", "New line");

        public CodeMenuEntryPixelInfo(string value, string description) : base(value, description)
        {
        }

        public static string Parse(string input, Color color, Point position)
        {
            input = input.Replace(r255.ToPrefixString(), color.R.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(g255.ToPrefixString(), color.G.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(b255.ToPrefixString(), color.B.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(HEX.ToPrefixString(), ColorHelpers.ColorToHex(color), StringComparison.InvariantCulture).
                Replace(hex.ToPrefixString(), ColorHelpers.ColorToHex(color).ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase).
                Replace(x.ToPrefixString(), position.X.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(y.ToPrefixString(), position.Y.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(n.ToPrefixString(), Environment.NewLine, StringComparison.InvariantCultureIgnoreCase);
            
            // Special cases for r1{n} that includes variable length rounding.
            // Turns out the rounding must be between 0 and 15 inclusive. I will force all numbers to that range.
            foreach (Tuple<string, int> entry in ListEntryWithValue(input, r1.ToPrefixString()))
            {
                input = input.Replace(entry.Item1, Math.Round(color.B / 255.0, Math.Min(Math.Max(entry.Item2, 0), 15), MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (Tuple<string, int> entry in ListEntryWithValue(input, g1.ToPrefixString()))
            {
                input = input.Replace(entry.Item1, Math.Round(color.B / 255.0, Math.Min(Math.Max(entry.Item2, 0), 15), MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            foreach (Tuple<string, int> entry in ListEntryWithValue(input, b1.ToPrefixString()))
            {
                input = input.Replace(entry.Item1, Math.Round(color.B / 255.0, Math.Min(Math.Max(entry.Item2, 0), 15), MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase);
            }

            // Case for r1 that does not include variable lenthg rounding, defaults to 3 didgets
            input = input.Replace(r1.ToPrefixString(), Math.Round(color.B / 255.0, 3, MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(g1.ToPrefixString(), Math.Round(color.B / 255.0, 3, MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(b1.ToPrefixString(), Math.Round(color.B / 255.0, 3, MidpointRounding.AwayFromZero).ToString(), StringComparison.InvariantCultureIgnoreCase);

            // This is here for backwards compatibility. These do not show up in the list, so they have to be done slightly differently,
            // and have to be done last so they don't munch the `$r` from other entries.
            input = input.Replace(_prefix + "r", color.R.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(_prefix + "g", color.G.ToString(), StringComparison.InvariantCultureIgnoreCase).
                Replace(_prefix + "b", color.B.ToString(), StringComparison.InvariantCultureIgnoreCase);

            return input;
        }

        private static IEnumerable<Tuple<string, string[]>> ListEntryWithArguments(string text, string entry, int elements)
        {
            foreach (Tuple<string, string> o in text.ForEachBetween(entry + "{", "}"))
            {
                string[] s = o.Item2.Split(',');
                if (elements > s.Length)
                {
                    Array.Resize(ref s, elements);
                }
                yield return new Tuple<string, string[]>(o.Item1, s);
            }
        }

        private static IEnumerable<Tuple<string, int[]>> ListEntryWithValues(string text, string entry, int elements)
        {
            foreach (Tuple<string, string[]> o in ListEntryWithArguments(text, entry, elements))
            {
                int[] a = new int[o.Item2.Length];
                for (int i = o.Item2.Length - 1; i >= 0; --i)
                {
                    int n = 0;
                    if (int.TryParse(o.Item2[i], out n))
                    {
                        a[i] = n;
                    }
                }
                yield return new Tuple<string, int[]>(o.Item1, a);
            }
        }

        private static IEnumerable<Tuple<string, int>> ListEntryWithValue(string text, string entry)
        {
            foreach (Tuple<string, int[]> o in ListEntryWithValues(text, entry, 1))
            {
                yield return new Tuple<string, int>(o.Item1, o.Item2[0]);
            }
        }
    }
}