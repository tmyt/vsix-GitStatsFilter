using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace GitStatsFilter.Filters
{
    // Implements ISolutionTreeFilterProvider. The SolutionTreeFilterProvider attribute declares it as a MEF component
    [SolutionTreeFilterProvider(PackageGuids.guidFileFilterCommandString, PackageIds.ApplyFilterCommandID)]
    public sealed partial class GitStatsFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

        // Constructor required for MEF composition
        [ImportingConstructor]
        public GitStatsFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _serviceProvider = serviceProvider;
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        // Returns an instance of Create filter class.
        protected override HierarchyTreeFilter CreateFilter()
        {
            return new GitStatsFilterProvider.GitStatsFilter(_serviceProvider, _hierarchyCollectionProvider);
        }
    }
}