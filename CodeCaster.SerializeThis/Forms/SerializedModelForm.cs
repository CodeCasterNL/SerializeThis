using System.Windows.Forms;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis.Forms
{
    public partial class SerializedModelForm : Form
    {
        public SerializedModelForm()
        {
            InitializeComponent();
        }

        public void UpdateModel(IClassInfoSerializer serializer, ClassInfo classInfo)
        {
            this.serializedModelTextBox.Text = serializer.Serialize(classInfo);
        }
    }
}
