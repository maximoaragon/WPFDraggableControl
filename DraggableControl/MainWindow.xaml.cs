using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1;

namespace controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double m_MouseX;
        double m_MouseY;

        string _testFile = "c:\\temp\\configui.xml";

        public MainWindow()
        {
            InitializeComponent();

            panel1Config.AddButtons(new[] { btn1 , btn2 , btn3 });

            if (File.Exists(_testFile))
            {
                string savedConfig = File.ReadAllText(_testFile);

                panel1Config.LoadConfigXML(savedConfig);
            }
        }

        private void TurnDesignModeON(Button[] buttons)
        {
            foreach (var btn in buttons)
            {
                btn.PreviewMouseUp += button1_MouseUp;
                btn.PreviewMouseLeftButtonDown += button1_MouseLeftButtonUp;
                btn.PreviewMouseMove += button1_MouseMove;
            }
        }

        private void button1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get the Position of Window so that it will set margin from this window
            m_MouseX = e.GetPosition(this).X;
            m_MouseY = e.GetPosition(this).Y;
        }

        private void button1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var btn = sender as Button;

                // Capture the mouse for border
                e.MouseDevice.Capture(btn);

                Thickness _margin;

                var parentContainer = btn.Parent as FrameworkElement;

                var mainWIndow  = Window.GetWindow(btn);

                int _tempX = Convert.ToInt32(e.GetPosition(mainWIndow).X);
                int _tempY = Convert.ToInt32(e.GetPosition(mainWIndow).Y);

                _margin = parentContainer.Margin;

                // when While moving _tempX get greater than m_MouseX relative to usercontrol 
                if (m_MouseX > _tempX)
                {
                    // add the difference of both to Left
                    _margin.Left += (_tempX - m_MouseX);
                    // subtract the difference of both to Left
                    _margin.Right -= (_tempX - m_MouseX);
                }
                else
                {
                    _margin.Left -= (m_MouseX - _tempX);
                    _margin.Right -= (_tempX - m_MouseX);
                }
                if (m_MouseY > _tempY)
                {
                    _margin.Top += (_tempY - m_MouseY);
                    _margin.Bottom -= (_tempY - m_MouseY);
                }
                else
                {
                    _margin.Top -= (m_MouseY - _tempY);
                    _margin.Bottom -= (_tempY - m_MouseY);
                }

                parentContainer.Margin = _margin;

                m_MouseX = _tempX;
                m_MouseY = _tempY;
            }
        }

        private void button1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.MouseDevice.Capture(null);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            panel1Config.EditMode(false);

            var uiConfigXML = panel1Config.GetConfigXML();

            File.WriteAllText(_testFile, uiConfigXML);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            
            if (tb.IsChecked == true)
            {
                btn1.EditMode(true);
                btn2.EditMode(true);
                btn3.EditMode(true);
            }
            else
            {
                btn1.EditMode(false);
                btn2.EditMode(false);
                btn3.EditMode(false);
            }
        }
    }
   
}