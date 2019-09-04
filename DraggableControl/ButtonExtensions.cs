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

        private static void StartEditMode(Control ctr)
        {
            var parentElement = ctr.Parent as FrameworkElement;

            string controlXML = XamlWriter.Save(ctr);

            ctr.SetValue(UIConfig.OriginalControlProperty, controlXML);

            //attach mouse move events
            ctr.PreviewMouseUp += btn_MouseUp;
            ctr.PreviewMouseLeftButtonDown += btn_MouseLeftButtonUp;
            ctr.PreviewMouseMove += btn_MouseMove;

            if (!parentElement.AllowDrop)
            {
                //the parent element element must allow drop 

                //so add a simple grid with it if not available
                Grid g = new Grid() { AllowDrop = true };

                if (parentElement is ContentControl)
                {
                    //buttons will be either in a ContentControl
                    ((ContentControl)parentElement).Content = null;

                    g.Children.Add(ctr);

                    ((ContentControl)parentElement).Content = g;
                }
                else if (parentElement is Panel)
                {
                    //or a Panel
                    ((Panel)parentElement).Children.Remove(ctr);

                    g.Children.Add(ctr);

                    ((Panel)parentElement).Children.Add(g);
                }
            }

            //the button should always be visible so it can be edited
            ctr.Visibility = Visibility.Visible;

            //set a style for edit mode
            ctr.BorderBrush = Brushes.Brown;
            ctr.BorderThickness = new Thickness(3);

            BuildContextMenu(ctr);

            //and set the visibility style
            var configVisible = (bool)ctr.GetValue(UIConfig.VisibleProperty);

            if (!configVisible)
            {
                //invisible edit mode style
                ctr.Foreground = Brushes.LightGray;
                ctr.Background = Brushes.WhiteSmoke;
            }
        }

        private static void StopEditMode(Control ctr)
        {
            var controlXAML = ctr.GetValue(UIConfig.OriginalControlProperty) as String;

            if (controlXAML == null) return;

            Control originalButton = XamlReader.Parse(controlXAML) as Control;

            //we could remove the grid added above

            //roll-back edit-mode styles
            ctr.BorderBrush = originalButton.BorderBrush;
            ctr.BorderThickness = originalButton.BorderThickness;
            ctr.Foreground = originalButton.Foreground;
            ctr.Background = originalButton.Background;

            ctr.ContextMenu = null;

            ctr.PreviewMouseUp -= btn_MouseUp;
            ctr.PreviewMouseLeftButtonDown -= btn_MouseLeftButtonUp;
            ctr.PreviewMouseMove -= btn_MouseMove;

            var configVisible = (bool)ctr.GetValue(UIConfig.VisibleProperty);

            ctr.SetValue(UIConfig.ParentMarginProperty, ctr.GetParentMargin().ToString());

            if (!configVisible)
            {
                //make invisible
                ctr.Visibility = Visibility.Hidden;
            }
        }

        private static void SetVisibilityStyle(Control btn, bool visible)
        {
            if (!visible)
            {
                //invisible style
                btn.Foreground = Brushes.LightGray;
                btn.Background = Brushes.WhiteSmoke;
            }
            else
            {
                var buttonXAML = btn.GetValue(UIConfig.OriginalControlProperty) as String;
                Button originalButton = XamlReader.Parse(buttonXAML) as Button;

                //original style

                btn.Foreground = originalButton.Foreground;
                btn.Background = originalButton.Background;
            }
        }

        private static void btn_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var btn = sender as Button;

            var mainWIndow = Window.GetWindow(btn);

            // Get the Position of Window so that it will set margin from this window
            _mouseX = e.GetPosition(mainWIndow).X;
            _mouseY = e.GetPosition(mainWIndow).Y;
        }

        private static void btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.MouseDevice.Capture(null);
        }

        private static void btn_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var btn = sender as Button;

                // Capture the mouse for border
                e.MouseDevice.Capture(btn);

                Thickness _margin = new System.Windows.Thickness();

                var mainWIndow = Window.GetWindow(btn);

                int _tempX = Convert.ToInt32(e.GetPosition(mainWIndow).X);
                int _tempY = Convert.ToInt32(e.GetPosition(mainWIndow).Y);

                var parentContainer = btn.Parent as FrameworkElement;

                _margin = parentContainer.Margin;

                // when While moving _tempX get greater than m_MouseX relative to usercontrol 
                if (_mouseX > _tempX)
                {
                    // add the difference of both to Left
                    _margin.Left += (_tempX - _mouseX);
                    // subtract the difference of both to Left
                    _margin.Right -= (_tempX - _mouseX);
                }
                else
                {
                    _margin.Left -= (_mouseX - _tempX);
                    _margin.Right -= (_tempX - _mouseX);
                }
                if (_mouseY > _tempY)
                {
                    _margin.Top += (_tempY - _mouseY);
                    _margin.Bottom -= (_tempY - _mouseY);
                }
                else
                {
                    _margin.Top -= (_mouseY - _tempY);
                    _margin.Bottom -= (_tempY - _mouseY);
                }

                parentContainer.Margin = _margin;

                _mouseX = _tempX;
                _mouseY = _tempY;
            }
        }
    }
}
