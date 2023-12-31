﻿using System.IO;

namespace Peernet.Browser.Application.Utilities
{
    public static class UtilityHelper
    {
        public static string StripInvalidWindowsCharactersFromFileName(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c.ToString(), "");
            }

            return s;
        }

        public static string StripInvalidWindowsCharactersFromPath(string s)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                s = s.Replace(c.ToString(), "");
            }

            return s;
        }
    }
}