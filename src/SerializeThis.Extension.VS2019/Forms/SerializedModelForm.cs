using System.Windows.Forms;
using SerializeThis.Serialization;

namespace SerializeThis.Forms
{
    public partial class SerializedModelForm : Form
    {
        public SerializedModelForm()
        {
            InitializeComponent();
        }

        public void UpdateModel(IClassInfoSerializer serializer, MemberInfo classInfo)
        {
            this.serializedModelTextBox.Text = serializer.Serialize(classInfo);
        }
    }
}
