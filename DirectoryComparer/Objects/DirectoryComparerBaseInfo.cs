﻿namespace DirectoryComparer.Objects
{
    public static class DirectoryComparerBaseInfo
    {
        public static string LeftPath { get; set; }
        public static string RightPath { get; set; }
        public static bool Recursive { get; set; }
        public static CompareResultsPreferences Preferences { get; set; }
    }
}