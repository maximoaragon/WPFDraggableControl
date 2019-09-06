using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace WpfApp1
{
    public static class DraggableControlExtensions
    {
        static double _mouseX;
        static double _mouseY;

        public static Thickness GetParentMargin(this Control ctr)
        {
            var parentContainer = ctr.Parent as FrameworkElement;

            return parentContainer.Margin;
        }

        public static void EditMode(this Control ctr, bool editMode)
        {
            if (editMode)
            {
                StartEditMode(ctr);
            }
            else
            {
                StopEditMode(ctr);
            }
        }

        public static void LoadConfiguration(this Control configuredControl, Window win)
        {
            //find matching element in window
            var matchedElement = win.FindName(configuredControl.Name) as FrameworkElement;

            if (matchedElement != null)
            {
                //set configured margin

                var parentElement = matchedElement.Parent as FrameworkElement;

                if (!parentElement.AllowDrop)
                {
                    //the parent element element must allow drop 

                    //so add a simple grid with it if not available
                    Grid g = new Grid() { AllowDrop = true };

                    if (parentElement is ContentControl)
                    {
                        //buttons will be either in a ContentControl
                        ((ContentControl)parentElement).Content = null;

                        g.Children.Add(matchedElement);

                        ((ContentControl)parentElement).Content = g;
                    }
                    else if (parentElement is Panel)
                    {
                        //or a Panel
                        ((Panel)parentElement).Children.Remove(matchedElement);

                        g.Children.Add(matchedElement);

                        ((Panel)parentElement).Children.Add(g);
                    }

                    parentElement = g;
                }

                var configuredMargin = configuredControl.GetValue(UIConfig.ParentMarginProperty) as string;

                if (!string.IsNullOrEmpty(configuredMargin))
                {
                    var mArray = configuredMargin.Split(',');

                    Thickness parentMargin = new Thickness(double.Parse(mArray[0]), double.Parse(mArray[1]), double.Parse(mArray[2]), double.Parse(mArray[3]));

                    parentElement.Margin = parentMargin;
                }

                var configVisible = (bool)configuredControl.GetValue(UIConfig.VisibleProperty);

                if (!configVisible)
                {
                    matchedElement.Visibility = Visibility.Hidden;
                }
            }
        }

        private static void BuildContextMenu(Control ctr)
        {
            var cm = new ContextMenu();

            var configVisible = (bool)ctr.GetValue(UIConfig.VisibleProperty);

            var item = new MenuItem() { Header = "Visible", IsCheckable = true, IsChecked = configVisible };

            item.Click += (s, e) =>
            {
                ctr.SetValue(UIConfig.VisibleProperty, item.IsChecked);

                SetVisibilityStyle(ctr, item.IsChecked);
            };

            cm.Items.Add(item);

            ctr.ContextMenu = cm;
        }

        private static void StartEditMode(Control ctrl)
        {
            var parentElement = ctrl.Parent as FrameworkElement;

            string controlXML = XamlWriter.Save(ctrl);

            ctrl.SetValue(UIConfig.OriginalControlProperty, controlXML);

            //attach mouse move events
            ctrl.PreviewMouseUp += ctrl_MouseUp;
            ctrl.PreviewMouseLeftButtonDown += ctrl_MouseLeftButtonUp;
            ctrl.PreviewMouseMove += ctrl_MouseMove;

            ctrl.Cursor = Cursors.SizeAll;

            if (!parentElement.AllowDrop)
            {
                //the parent element element must allow drop 

                //so add a simple grid with it if not available
                Grid g = new Grid() { AllowDrop = true };

                if (parentElement is ContentControl)
                {
                    //control will be either in a ContentControl
                    ((ContentControl)parentElement).Content = null;

                    g.Children.Add(ctrl);

                    ((ContentControl)parentElement).Content = g;
                }
                else if (parentElement is Panel)
                {
                    //or a Panel
                    ((Panel)parentElement).Children.Remove(ctrl);

                    g.Children.Add(ctrl);

                    ((Panel)parentElement).Children.Add(g);
                }
            }

            //the button should always be visible so it can be edited
            ctrl.Visibility = Visibility.Visible;

            //set a style for edit mode
            ctrl.BorderBrush = Brushes.Green;
            ctrl.BorderThickness = new Thickness(3);

            BuildContextMenu(ctrl);

            //and set the visibility style
            var configVisible = (bool)ctrl.GetValue(UIConfig.VisibleProperty);

            if (!configVisible)
            {
                //invisible edit mode style
                ctrl.BorderBrush = Brushes.Brown;
                //ctrl.Foreground = Brushes.LightGray;
                //ctrl.Background = Brushes.WhiteSmoke;
            }
        }

        private static void StopEditMode(Control ctrl)
        {
            var controlXAML = ctrl.GetValue(UIConfig.OriginalControlProperty) as String;

            if (controlXAML == null) return;

            Control originalControl = XamlReader.Parse(controlXAML) as Control;

            //we could remove the grid added above

            //roll-back edit-mode styles
            ctrl.BorderBrush = originalControl.BorderBrush;
            ctrl.BorderThickness = originalControl.BorderThickness;
            ctrl.Foreground = originalControl.Foreground;
            ctrl.Background = originalControl.Background;

            ctrl.Cursor = Cursors.Arrow;

            ctrl.ContextMenu = null;

            ctrl.PreviewMouseUp -= ctrl_MouseUp;
            ctrl.PreviewMouseLeftButtonDown -= ctrl_MouseLeftButtonUp;
            ctrl.PreviewMouseMove -= ctrl_MouseMove;

            var configVisible = (bool)ctrl.GetValue(UIConfig.VisibleProperty);

            ctrl.SetValue(UIConfig.ParentMarginProperty, ctrl.GetParentMargin().ToString());

            if (!configVisible)
            {
                //make invisible
                ctrl.Visibility = Visibility.Hidden;
            }
        }

        private static void SetVisibilityStyle(Control ctrl, bool visible)
        {
            //var parentElement = ctrl.Parent as FrameworkElement;

            if (!visible)
            {
                //invisible style
                //ctrl.Foreground = Brushes.LightGray;
                //ctrl.Background = Brushes.WhiteSmoke;

                ctrl.BorderBrush = Brushes.Brown;
            }
            else
            {
                var controlXAML = ctrl.GetValue(UIConfig.OriginalControlProperty) as string;
                Control originalControl = XamlReader.Parse(controlXAML) as Control;

                //original style

                //ctrl.Foreground = originalControl.Foreground;
                //ctrl.Background = originalControl.Background;
                ctrl.BorderBrush = Brushes.Green;
             
            }
        }

        private static void ctrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var ctrl = sender as Control;

            var mainWIndow = Window.GetWindow(ctrl);

            // Get the Position of Window so that it will set margin from this window
            _mouseX = e.GetPosition(mainWIndow).X;
            _mouseY = e.GetPosition(mainWIndow).Y;
        }

        private static void ctrl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.MouseDevice.Capture(null);
        }

        private static void ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var ctrl = sender as Control;

                // Capture the mouse for border
                e.MouseDevice.Capture(ctrl);

                var mainWIndow = Window.GetWindow(ctrl);

                int tempX = Convert.ToInt32(e.GetPosition(mainWIndow).X);
                int tempY = Convert.ToInt32(e.GetPosition(mainWIndow).Y);

                var parentContainer = ctrl.Parent as FrameworkElement;

                Thickness mragin = parentContainer.Margin;

                // when While moving _tempX get greater than m_MouseX relative to usercontrol 
                if (_mouseX > tempX)
                {
                    // add the difference of both to Left
                    mragin.Left += (tempX - _mouseX);
                    // subtract the difference of both to Left
                    mragin.Right -= (tempX - _mouseX);
                }
                else
                {
                    mragin.Left -= (_mouseX - tempX);
                    mragin.Right -= (tempX - _mouseX);
                }
                if (_mouseY > tempY)
                {
                    mragin.Top += (tempY - _mouseY);
                    mragin.Bottom -= (tempY - _mouseY);
                }
                else
                {
                    mragin.Top -= (_mouseY - tempY);
                    mragin.Bottom -= (tempY - _mouseY);
                }

                parentContainer.Margin = mragin;

                _mouseX = tempX;
                _mouseY = tempY;
            }
        }
    }
}
