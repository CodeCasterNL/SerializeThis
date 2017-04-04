﻿using System;
using System.IO;
using CodeCaster.SerializeThis.Serialization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodeCaster.SerializeThis.OutputHandlers
{
    public class TempFileInNewTabOutputHandler : IOutputHandler
    {
        private bool _enabled;
        public int Priority => 30;
        private IServiceProvider _serviceProvider;

        public void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _enabled = true;
        }

        public bool Handle(IClassInfoSerializer serializer, ClassInfo classInfo)
        {
            if (!_enabled)
            {
                return false;
            }

            try
            {
                string filename;
                if (TryWriteTempFile(serializer, classInfo, out filename))
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

            return false;
        }

        private bool TryWriteTempFile(IClassInfoSerializer serializer, ClassInfo classInfo, out string filename)
        {
            filename = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            string serialized = serializer.Serialize(classInfo);

            File.WriteAllText(filename, serialized);

            return true;
        }

        private bool ShowTempFile(string filename)
        {
            IVsUIShellOpenDocument openDoc = _serviceProvider.GetService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;

            if (openDoc != null)
            {
                IVsWindowFrame frame;
                uint itemId;
                IVsUIHierarchy hier;
                Guid logicalView = VSConstants.LOGVIEWID_Primary;
                Microsoft.VisualStudio.OLE.Interop.IServiceProvider sppIgnored;

                int result = openDoc.OpenDocumentViaProjectWithSpecific(filename, (uint) __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen, DefGuidList.CLSID_TextEditorFactory, null, ref logicalView, out sppIgnored, out hier, out itemId, out frame);
                if (result == VSConstants.S_OK && frame != null)
                {
                    return frame.Show() == VSConstants.S_OK;
                }
            }

            return false;
        }
    }
}
