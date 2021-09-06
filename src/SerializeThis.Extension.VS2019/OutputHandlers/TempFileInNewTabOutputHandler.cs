using System;
using System.IO;
using SerializeThis.Serialization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SerializeThis.Extension.VS2019.OutputHandlers
{
    public class TempFileInNewTabOutputHandler : IOutputHandler
    {
        public int Priority => 30;

        private bool _enabled;
        private IServiceProvider _serviceProvider;

        public void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _enabled = true;
        }

        public bool Handle(IClassInfoSerializer serializer, MemberInfo classInfo)
        {
            if (!_enabled)
            {
                return false;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var filename = GenerateUniqueFileName(serializer, classInfo);

            try
            {
                if (TryWriteTempFile(filename, serializer, classInfo))
                {
                    return ShowTempFile(filename);
                }
            }
            catch (UnauthorizedAccessException)
            {
                _enabled = false;
            }
            catch (IOException)
            {
                _enabled = false;
            }
            finally
            {
                File.Delete(filename);
            }

            return false;
        }

        private bool TryWriteTempFile(string filename, IClassInfoSerializer serializer, MemberInfo classInfo)
        {
            string serialized = serializer.Serialize(classInfo);

            File.WriteAllText(filename, serialized);

            return true;
        }

        private string GenerateUniqueFileName(IClassInfoSerializer serializer, MemberInfo classInfo)
        {
            string filename;
            int i = 0;
            do
            {
                filename = Path.Combine(Path.GetTempPath(), ReplaceInvalidFilenameChars(classInfo.Class.TypeName, '_'));
                if (i > 0)
                {
                    filename += $"({i})";
                }
                i++;

                filename += "." + serializer.FileExtension;
            }
            while (File.Exists(filename));

            return filename;
        }

        private string ReplaceInvalidFilenameChars(string filename, char v)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, v);
            }
            return filename;
        }

        private bool ShowTempFile(string filename)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var openDoc = _serviceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            if (openDoc != null)
            {
                Guid logicalView = VSConstants.LOGVIEWID_Primary;

                int result = openDoc.OpenDocumentViaProjectWithSpecific(filename, (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen, DefGuidList.CLSID_TextEditorFactory, null, ref logicalView, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider sppIgnored, out IVsUIHierarchy hier, out uint itemId, out IVsWindowFrame frame);
                if (result == VSConstants.S_OK && frame != null)
                {
                    return frame.Show() == VSConstants.S_OK;
                }
            }

            return false;
        }
    }
}
