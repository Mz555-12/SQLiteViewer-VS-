using SQLiteViewer.Models;
using SQLiteViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SQLiteViewer.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool is_top = false;
        public MainWindow()
        {
            InitializeComponent();

            MainWindowModel.Instance.currentFolder_text = this.currentFolder_text;

            this.DataContext = new MainWindowViewModel();

        }

        private void RowDefinition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Minimum_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TopWindow_Click(object sender, RoutedEventArgs e)
        {
            is_top = !is_top;
            this.Topmost = !this.Topmost;
            this.border_top.BorderThickness = is_top ? new Thickness(0, 0, 0, 2) : new Thickness(0);
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem)
            {
                if (treeViewItem.DataContext is SQLiteViewer.Models.FileSystemItem item)
                {
                    var vm = (this.DataContext as MainWindowViewModel)?.FileExplorerViewModel;
                    vm?.OnItemExpanded(item);

                    // 添加调试信息
                    System.Diagnostics.Debug.WriteLine($"展开文件夹: {item.Name}, 路径: {item.FullPath}");
                }
            }
            e.Handled = true;
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem)
            {
                if (treeViewItem.DataContext is SQLiteViewer.Models.FileSystemItem item)
                {
                    // 可以在这里处理折叠事件
                    System.Diagnostics.Debug.WriteLine($"折叠文件夹: {item.Name}");
                }
            }
            e.Handled = true;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取触发事件的元素
            var element = e.OriginalSource as FrameworkElement;

            // 向上查找 TreeViewItem
            while (element != null && !(element is TreeViewItem))
            {
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }

            if (element is TreeViewItem treeViewItem &&
                treeViewItem.DataContext is SQLiteViewer.Models.FileSystemItem item)
            {
                var vm = (this.DataContext as MainWindowViewModel)?.FileExplorerViewModel;
                vm?.OnFileDoubleClicked(item);
                e.Handled = true;
            }
        }



        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取点击的元素
            var element = e.OriginalSource as FrameworkElement;

            // 向上查找 TreeViewItem
            TreeViewItem treeViewItem = null;
            while (element != null && treeViewItem == null)
            {
                if (element is TreeViewItem tvItem) // 修改变量名，避免冲突
                {
                    treeViewItem = tvItem;
                }
                else
                {
                    element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                }
            }

            if (treeViewItem != null &&
                treeViewItem.DataContext is SQLiteViewer.Models.FileSystemItem fileSystemItem) // 修改变量名，避免冲突
            {
                var vm = (this.DataContext as MainWindowViewModel)?.FileExplorerViewModel;
                vm?.OnFileDoubleClicked(fileSystemItem);

                // 添加调试信息
                System.Diagnostics.Debug.WriteLine($"TreeView_MouseDoubleClick: {fileSystemItem.Name}, 路径: {fileSystemItem.FullPath}, 是文件夹: {fileSystemItem.IsDirectory}");

                e.Handled = true;
            }
        }



     


    }
}
