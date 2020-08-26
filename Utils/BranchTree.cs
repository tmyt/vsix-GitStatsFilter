using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using GitStatFilter.Extensions;

namespace GitStatFilter.Utils
{
    public class BranchTree
    {
        private readonly Dictionary<string, BranchTree> _children = new Dictionary<string, BranchTree>();

        public IEnumerable<string> Keys => _children.Keys;
        public bool IsLeaf => _children.Count == 0;
        public string CanonicalName { get; private set; } = "";
        public BranchTree this[string name] => _children[name];

        public void Add(string s)
        {
            var parts = s.Split('/');
            var root = _children;
            var canonicalName = "";
            foreach (var p in parts)
            {
                canonicalName = string.IsNullOrEmpty(canonicalName) ? p : string.Join("/", canonicalName, p);
                if (!root.ContainsKey(p))
                {
                    root.Add(p, new BranchTree { CanonicalName = canonicalName });
                }
                root = root[p]._children;
            }
        }

        public MenuItem[] ToMenuItems(string currentSelection)
        {
            return ToMenuItems(currentSelection, out _);
        }

        private MenuItem[] ToMenuItems(string currentSelection, out bool isChecked)
        {
            var items = new List<MenuItem>();
            isChecked = false;
            foreach (var key in Keys)
            {
                var item = _children[key];
                var childItems = item.ToMenuItems(currentSelection, out var childrenHasChecked);
                var newItem = new MenuItem { Header = key };
                if (item.IsLeaf)
                {
                    childrenHasChecked = item.CanonicalName == currentSelection;
                    newItem.Tag = item.CanonicalName;
                }
                else
                {
                    newItem.Header = $"{newItem.Header}/";
                    newItem.Items.AddRange(childItems);
                }
                isChecked |= (newItem.IsChecked = childrenHasChecked);
                items.Add(newItem);
            }
            return items
                .OrderByDescending(a => Math.Min(1, a.Items.Count))
                .ThenBy(a => (string)a.Header)
                .ToArray();
        }

        public static BranchTree From(IEnumerable<string> branches)
        {
            var root = new BranchTree();
            foreach (var b in branches)
            {
                root.Add(b);
            }
            return root;
        }
    }
}