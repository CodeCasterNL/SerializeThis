using System.Windows.Forms;
using CodeCaster.SerializeThis.Serialization;
using CodeCaster.SerializeThis.Serialization.Json;

namespace CodeCaster.SerializeThis.Forms
{
    public partial class SerializedModelForm : Form
    {
        public SerializedModelForm()
        {
            InitializeComponent();
        }

        public void UpdateModel(ClassInfo classInfo, string menuItemName)
        {
            var serializer = GetSerializer(menuItemName);
            this.Text = $"Serialized Model: {menuItemName}";
            this.serializedModelTextBox.Text = serializer.Serialize(classInfo);
        }

        private IClassInfoSerializer GetSerializer(string menuItemName)
        {
            // TODO: IPluginFactory? Injection? Plugins?
            return new JsonSerializer();
        }
    }
}
