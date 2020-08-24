﻿using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace BranchFilter
{
    static class Git
    {
        public static Repository OpenNearestRepository(string dir)
        {
            while (dir != null)
            {
                if (dir.EndsWith("\\")) dir = Path.GetDirectoryName(dir);
                if (dir == null) break;
                try
                {
                    return new Repository(dir);
                }
                catch (RepositoryNotFoundException)
                {
                    dir = Path.GetDirectoryName(dir);
                }
            }
            return null;
        }

        public static string[] ChangedFiles(this Repository repo, string sourceBranch)
        {
            var head = repo.Head;
            var origin = repo.Branches[sourceBranch];
            var changes = repo.Diff.Compare<TreeChanges>(origin.Commits.First().Tree, head.Commits.First().Tree);
            var status = repo.RetrieveStatus();
            return status
                .Select(a => a.FilePath)
                .Concat(changes.Select(a => a.Path))
                .Select(a => (repo.Info.WorkingDirectory + a).Replace("/", "\\"))
                .ToArray();
        }
    }
}
