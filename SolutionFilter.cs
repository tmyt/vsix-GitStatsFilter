using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace BranchFilter
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SolutionFilter
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("5448d968-2036-4a64-8f30-d2cb9f61d759");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFilter"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SolutionFilter(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ChooseSourceBranch, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        private async void ChooseSourceBranch(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = (DTE)await _package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true);
            if (dte == null) return;
            if (string.IsNullOrEmpty(dte.Solution.FullName)) return;
            var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
            Config.Load(solutionDir);
            using var repo = Git.OpenRepository(solutionDir);
            var menu = new DynamicMenu();
            if (repo == null)
            {
                menu.Items.Add(new MenuItem
                {
                    Header = "有効なGitリポジトリが見つかりません",
                    IsEnabled = false,
                });
            }
            else
            {
                var tree = BranchTree.From(repo.Branches.Select(a => a.FriendlyName));
                var current = string.IsNullOrEmpty(Config.TargetBranch)
                    ? repo.Head.FriendlyName
                    : Config.TargetBranch;
                menu.Items.AddRange(tree.ToMenuItems(current));
            }
            menu.BranchSelected += (o, s) =>
            {
                Config.TargetBranch = (string)((MenuItem)o).Tag;
                Config.Save(solutionDir);
            };
            menu.Show();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SolutionFilter Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider => this._package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SolutionFilter's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SolutionFilter(package, commandService);
        }
    }
}
