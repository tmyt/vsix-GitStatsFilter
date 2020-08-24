using System;
using System.Windows;
using System.Windows.Controls;

namespace BranchFilter
{
    public class DynamicMenu
    {
        private readonly ContextMenu _menu;
        private Window _control;

        public ItemCollection Items => _menu.Items;
        public event EventHandler<RoutedEventArgs> BranchSelected;

        public DynamicMenu()
        {
            _menu = new ContextMenu();
            _menu.Closed += _menu_Closed;
            _menu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(_menu_Click));
        }

        public void Show()
        {
            _control = CreateHost();
            _control.ContextMenu = _menu;
            _menu.IsOpen = true;
        }

        private void _menu_Closed(object sender, RoutedEventArgs e)
        {
            Cleanup();
        }

        private void _menu_Click(object sender, RoutedEventArgs e)
        {
            BranchSelected?.Invoke(e.Source, e);
        }

        private Window CreateHost()
        {
            return new Window
            {
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Width = 0,
                Height = 0,
                Top = 0,
                Left = 0,
                Visibility = Visibility.Hidden,
            };
        }

        private void Cleanup()
        {
            _control.Close();
        }
    }
}