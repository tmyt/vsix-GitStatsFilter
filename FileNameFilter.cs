using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using LibGit2Sharp;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace BranchFilter
{
    // Implements ISolutionTreeFilterProvider. The SolutionTreeFilterProvider attribute declares it as a MEF component
    [SolutionTreeFilterProvider(CommandSet, CommandId)]
    public sealed class FileNameFilterProvider : HierarchyTreeFilterProvider
    {
        public const int CommandId = 0x0100;
        public const string CommandSet = "5448d968-2036-4a64-8f30-d2cb9f61d759";

        private readonly SVsServiceProvider _serviceProvider;
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

        // Constructor required for MEF composition
        [ImportingConstructor]
        public FileNameFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _serviceProvider = serviceProvider;
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        // Returns an instance of Create filter class.
        protected override HierarchyTreeFilter CreateFilter()
        {
            return new FileNameFilter(_serviceProvider, _hierarchyCollectionProvider);
        }

        // Implementation of file filtering
        private sealed class FileNameFilter : HierarchyTreeFilter
        {
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
            private readonly Repository _repo;

            public FileNameFilter(
                IServiceProvider serviceProvider,
                IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _hierarchyCollectionProvider = hierarchyCollectionProvider;

                var dte = (DTE)serviceProvider.GetService(typeof(DTE));
                var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                Config.Load(solutionDir);
                _repo = Git.OpenRepository(solutionDir);
            }

            // Gets the items to be included from this filter provider.
            // rootItems is a collection that contains the root of your solution
            // Returns a collection of items to be included as part of the filter
            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                var root = HierarchyUtilities.FindCommonAncestor(rootItems);
                var sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(
                    root.HierarchyIdentity.NestedHierarchy,
                    CancellationToken);
                if (_repo == null) return sourceItems;
                var target = string.IsNullOrEmpty(Config.TargetBranch) ? _repo.Head.FriendlyName : Config.TargetBranch;
                if (target == "(no branch)") return sourceItems;
                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(
                    sourceItems,
                    ShouldIncludeInFilter(_repo.ChangedFiles(target)),
                    CancellationToken);
            }

            // Returns true if filters hierarchy item name for given filter; otherwise, false</returns>
            private Predicate<IVsHierarchyItem> ShouldIncludeInFilter(string[] changes)
            {
                return item => changes.Contains(item.CanonicalName, new IgnoreCaseComparator());
            }

            protected override void DisposeNativeResources()
            {
                _repo?.Dispose();
                base.DisposeNativeResources();
            }
        }
    }
}