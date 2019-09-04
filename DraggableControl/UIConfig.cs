using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class UIConfig : DependencyObject
    {
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.RegisterAttached(
            "Visible", typeof(bool), typeof(UIConfig), new PropertyMetadata(true));

        public static void SetVisible(DependencyObject target, Boolean value)
        {
            target.SetValue(VisibleProperty, value);
        }

        public static bool GetVisible(DependencyObject target)
        {
            return (bool)target.GetValue(VisibleProperty);
        }

        public static readonly DependencyProperty OriginalControlProperty = DependencyProperty.RegisterAttached(
            "OriginalButton", typeof(string), typeof(UIConfig));

        public static void SetOriginalControl(DependencyObject target, string value)
        {
            target.SetValue(OriginalControlProperty, value);
        }

        public static string GetOriginalControl(DependencyObject target)
        {
            return (string)target.GetValue(OriginalControlProperty);
        }

        public static readonly DependencyProperty ParentMarginProperty = DependencyProperty.RegisterAttached(
            "ParentMargin", typeof(string), typeof(UIConfig));

        public static void SetParentMargin(DependencyObject target, string value)
        {
            target.SetValue(ParentMarginProperty, value);
        }

        public static string GetParentMargin(DependencyObject target)
        {
            return (string)target.GetValue(ParentMarginProperty);
        }
    }  
}