﻿using System.Collections.Generic;
using DirectoryComparer.Objects;

namespace DirectoryComparer.Interfaces
{
    public interface ITwoPassComparer
    {
        List<CompareResult> LeftCompare(string leftFolder, string rightFolder);
        List<CompareResult> RightCompare(string rightFolder, string leftFolder);
    }
}