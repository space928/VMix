using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VMix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ### Globals ###

        #endregion

        #region ### Utility ###
        //This should only be finite since a visualtree cannot contain loops
        public T FindParent<T>(Visual child, Type type, string name = "") where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !type.GetType().IsInstanceOfType(parent) && (name == "" || ((Control)parent).Name != name))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return (parent as T);
        }
        #endregion

        #region ### Main Program ###
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region ### Event Handlers ###

        #endregion
    }
}
