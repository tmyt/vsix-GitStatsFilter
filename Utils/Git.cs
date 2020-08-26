using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitStatFilter.Utils
{
    internal static class Git
    {
        public static Repository? TryOpenRepository(string repoDir)
        {
            try
            {
                return new Repository(repoDir);
            }
            catch (RepositoryNotFoundException)
            {
                return null;
            }
        }

        public static Repository? OpenRepository(string? dir)
        {
            while (dir != null)
            {
                if (dir.EndsWith("\\")) dir = Path.GetDirectoryName(dir);
                if (dir == null) break;
                var repo = TryOpenRepository(dir);
                if (repo != null) return repo;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        public static string[]? ChangedFiles(this Repository repo, string sourceBranch)
        {
            // check `(no branch)`
            if (sourceBranch == "(no branch)") return null;
            // compare with sourceBranch
            var origin = repo.Branches[sourceBranch];
            var changes = repo.Diff.Compare<TreeChanges>(origin.Commits.First().Tree, DiffTargets.WorkingDirectory);
            return changes
                .Select(a => a.Path)
                .Select(a => (repo.Info.WorkingDirectory + a).Replace("/", "\\"))
                .ToArray();
        }
    }
}
