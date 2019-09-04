using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for DraggableButton.xaml
    /// </summary>
    public partial class ConfigUIButton : UserControl
    {
        public ConfigUIButton()
        {
            InitializeComponent();
        }

        private Button[] _buttons;

        internal void AddButtons(Button[] button)
        {
            _buttons = button;
        }

        private void TbtnConfig_Click(object sender, RoutedEventArgs e)
        {
            tbtnConfig.ContextMenu.IsOpen = true;
        }

        public void LoadConfigXML(string configXML)
        {
            var element = XElement.Parse(configXML);

            foreach(var el in element.Elements())
            {
                //this should be 1st-level decendants
                Control configuredControl  = XamlReader.Parse(el.ToString()) as Control;

                var win = Window.GetWindow(this);

                configuredControl.LoadConfiguration(win);
            }
        }

        public string GetConfigXML()
        {
            if (_buttons == null) return null;

            var configElement = new XElement("ConfiguredControl", new XAttribute("name", Name), new XAttribute("type", "xaml"));

            foreach (var btn in _buttons)
            {
                var parentElement = btn.Parent as FrameworkElement;
              
                if (!parentElement.AllowDrop)
                {
                    continue; // not configured
                }

                string buttonXML = XamlWriter.Save(btn); //parent contains the margins needed in in the positioning

                configElement.Add(XElement.Parse(buttonXML));
            }

            return configElement.ToString();
        }

        private void editMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_buttons == null) return;

            bool editing = false;

            if (editMenuItem.Header.ToString() == "Edit Layout")
            {
                editMenuItem.Header = "Stop Editing";
                editing = true;
            }
            else
            {
                editMenuItem.Header = "Edit Layout";
                editing = false;
            }

            EditMode(editing);
        }

        public void EditMode(bool editing)
        {
            foreach (var btn in _buttons)
            {
                btn.EditMode(editing);
            }
        }

    }
}
