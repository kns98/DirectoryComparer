using System.Collections.Generic;
using DirectoryComparer.Objects;

namespace DirectoryComparer.Interfaces
{
    public interface IResults
    {
        List<CompareResult> LeftResults { get; set; }
        List<CompareResult> RightResults { get; set; }

        List<CompareResult> CoalescedResults();
    }
}