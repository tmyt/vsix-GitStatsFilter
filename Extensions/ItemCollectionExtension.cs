using System.Windows.Controls;

namespace GitStatFilter.Extensions
{
    static class ItemCollectionExtension
    {
        public static void AddRange(this ItemCollection target, params MenuItem[] items)
        {
            foreach (var i in items)
            {
                target.Add(i);
            }
        }
    }
}