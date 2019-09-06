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
        string _testFile = ".\\configui.xml";

        public MainWindow()
        {
            InitializeComponent();

            panel1Config.AddControls(new[] { btn1 , btn2 , btn3 });

            panel1Config.AddControls(new[] { groupBox1, groupBox2, groupBox3 });


            if (File.Exists(_testFile))
            {
                string savedConfig = File.ReadAllText(_testFile);

                panel1Config.LoadConfigXML(savedConfig);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            panel1Config.EditMode(false);

            var uiConfigXML = panel1Config.GetConfigXML();

            File.WriteAllText(_testFile, uiConfigXML);
        }

    }
}