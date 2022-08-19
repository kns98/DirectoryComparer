using DirectoryComparer.Interfaces;
using DirectoryComparer.Objects;

namespace DirectoryComparer.Comparers
{
    public class RecursiveDirectoryComparer : IDirectoryComparer
    {
        private readonly ITwoPassComparer _comparer;

        private ComparisonResults _comparisonResults;

        public RecursiveDirectoryComparer(ITwoPassComparer comparer)
        {
            _comparer = comparer;
        }

        public ComparisonResults CompareDirectories()
        {
            _comparisonResults = new ComparisonResults();
            _comparisonResults.LeftResults = _comparer.LeftCompare(DirectoryComparerBaseInfo.LeftPath,
                DirectoryComparerBaseInfo.RightPath);
            _comparisonResults.RightResults = _comparer.RightCompare(DirectoryComparerBaseInfo.RightPath,
                DirectoryComparerBaseInfo.LeftPath);
            return _comparisonResults;
        }
    }
}