using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;


namespace BroadFileSystemAccess
{
    public class TreeViewBehavior : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            AssociatedObject.ItemInvoked += AssociatedObject_ItemInvoked;
        }

        private void AssociatedObject_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            SelectedItem = args.InvokedItem;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ItemInvoked -= AssociatedObject_ItemInvoked;
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewBehavior), new PropertyMetadata(null, SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewBehavior behavior = (TreeViewBehavior)d;
            behavior.OnSelectedItem();
        }

        private void OnSelectedItem()
        {
            if (AssociatedObject.SelectedItem != SelectedItem)
            {
                AssociatedObject.SelectedItem = SelectedItem;
            }
        }
    }
}
