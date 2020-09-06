using System;

namespace GitStatFilter
{
    internal static class PackageConsts
    {
        public const string PackageGuidString = "5d00e368-00a1-4c6c-8165-837abba71fb3";
        public const string CommandSetGuidString = "5448d968-2036-4a64-8f30-d2cb9f61d759";

        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);

        public const int FilterCommandId = 0x0100;
        public const int ChooseBranchCommandId = 0x0101;
    }
}
