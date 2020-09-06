using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using GitStatsFilter.Utils;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace GitStatsFilter.Filters
{
    public sealed partial class GitStatFilterProvider
    {
        // Implementation of file filtering
        private sealed class GitStatFilter : HierarchyTreeFilter
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

            public GitStatFilter(
                IServiceProvider serviceProvider,
                IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
            {
                _serviceProvider = serviceProvider;
                _hierarchyCollectionProvider = hierarchyCollectionProvider;
            }

            // Gets the items to be included from this filter provider.
            // rootItems is a collection that contains the root of your solution
            // Returns a collection of items to be included as part of the filter
            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                // get default hierarchy
                var root = HierarchyUtilities.FindCommonAncestor(rootItems);
                var sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(
                    root.HierarchyIdentity.NestedHierarchy,
                    CancellationToken);
                // switch thread context
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken);
                // get diff stats
                var stats = GetRepoStats();
                if (stats == null) return sourceItems;
                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(
                    sourceItems,
                    ShouldIncludeInFilter(stats),
                    CancellationToken);
            }

            // Returns true if filters hierarchy item name for given filter; otherwise, false</returns>
            private Predicate<IVsHierarchyItem> ShouldIncludeInFilter(string[] changes)
            {
                return item => changes.Contains(item.CanonicalName, StringComparer.CurrentCultureIgnoreCase);
            }

            private string[]? GetRepoStats()
            {
                var solutionDir = GetSolutionDir();
                using var repo = Git.OpenRepository(solutionDir);
                if (repo == null || solutionDir == null) return null;
                var config = Config.Load(solutionDir);
                return repo.ChangedFiles(StringEx.OrDefault(config.TargetBranch, repo.Head.FriendlyName));
            }

            private string? GetSolutionDir()
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (!(_serviceProvider.GetService(typeof(DTE)) is DTE dte)) return null;
                return Path.GetDirectoryName(dte.Solution.FullName);
            }
        }
    }
}
