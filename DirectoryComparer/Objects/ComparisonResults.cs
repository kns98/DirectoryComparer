﻿using System.Collections.Generic;
using System.Linq;
using DirectoryComparer.Interfaces;

namespace DirectoryComparer.Objects
{
    public class ComparisonResults : IResults
    {
        public List<CompareResult> LeftResults { get; set; }
        public List<CompareResult> RightResults { get; set; }

        public List<CompareResult> CoalescedResults()
        {
            return LeftResults
                .Concat(RightResults)
                .ToList();
        }
    }
}