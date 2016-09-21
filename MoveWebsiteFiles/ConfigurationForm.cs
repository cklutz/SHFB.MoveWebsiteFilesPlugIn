using System;
using System.Windows.Forms;

namespace MoveWebsiteFiles
{
    internal sealed partial class ConfigurationForm : Form
    {
        public ConfigurationForm(ConfigurationData configurationData)
        {
            InitializeComponent();
            NewConfiguration = configurationData.Clone();
            _propertyGrid.SelectedObject = NewConfiguration;
        }

        public ConfigurationData NewConfiguration { get; private set; }
    }
}
