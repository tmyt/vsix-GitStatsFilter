using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using EnvDTE;
using GitStatsFilter.Extensions;
using GitStatsFilter.UI;
using GitStatsFilter.Utils;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitStatsFilter.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ChooseBranchCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChooseBranchCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ChooseBranchCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(PackageConsts.CommandSetGuid, PackageConsts.ChooseBranchCommandId);
            var menuItem = new MenuCommand(ChooseSourceBranch, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        private async void ChooseSourceBranch(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!(await _package.GetServiceAsync(typeof(DTE)) is DTE dte)) return;
            if (string.IsNullOrEmpty(dte.Solution.FullName)) return;
            var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
            var menu = new DynamicMenu();
            menu.Items.AddRange(GenerateMenuItems(solutionDir));
            menu.ItemClick += (o, s) =>
            {
                var config = new Config { TargetBranch = (string)o.Tag };
                config.Save(solutionDir);
            };
            menu.Show();
        }

        private MenuItem[] GenerateMenuItems(string solutionDir)
        {
            using var repo = Git.OpenRepository(solutionDir);
            if (repo == null) return MakeError(GitStatFilter.Resources.Strings.could_not_find_valid_git_repository);
            var config = Config.Load(solutionDir);
            var current = StringEx.OrDefault(config.TargetBranch, repo.Head.FriendlyName);
            if (current == null) return MakeError(GitStatFilter.Resources.Strings.could_not_find_valid_git_repository);
            var tree = BranchTree.From(repo.Branches.Select(a => a.FriendlyName));
            return tree.ToMenuItems(current);
        }

        private MenuItem[] MakeArray(params MenuItem[] items) => items;

        private MenuItem[] MakeError(string message) => MakeArray(new MenuItem
        {
            Header = message,
            IsEnabled = false,
        });

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ChooseBranchCommand? Instance { get; private set; }

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
            Instance = new ChooseBranchCommand(package, commandService);
        }
    }
}
