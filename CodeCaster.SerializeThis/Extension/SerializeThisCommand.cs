using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeCaster.SerializeThis.OutputHandlers;
using CodeCaster.SerializeThis.Serialization;
using CodeCaster.SerializeThis.Serialization.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace CodeCaster.SerializeThis.Extension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SerializeThisCommand
    {
        /// </summary>
        public const int JsonCommandId = 0x0100;
        public const int XmlCommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c2c4513d-ca4c-4b91-be0d-b797460e7572");

        private readonly Microsoft.VisualStudio.Shell.Package _package;
        private IServiceProvider ServiceProvider => _package;

        private readonly ISerializerFactory _serializerFactory;
        private readonly IOutputHandler[] _outputHandlers;

        public static SerializeThisCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="serializerFactory"></param>
        /// <param name="outputHandlers"></param>
        public static void Initialize(Microsoft.VisualStudio.Shell.Package package, ISerializerFactory serializerFactory, IEnumerable<IOutputHandler> outputHandlers)
        {
            Instance = new SerializeThisCommand(package, serializerFactory, outputHandlers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeThisCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="serializerFactory"></param>
        /// <param name="outputHandlers"></param>
        private SerializeThisCommand(Microsoft.VisualStudio.Shell.Package package, ISerializerFactory serializerFactory, IEnumerable<IOutputHandler> outputHandlers)
        {
            _outputHandlers = outputHandlers.OrderByDescending(o => o.Priority).ToArray();
            _serializerFactory = serializerFactory ?? throw new ArgumentNullException(nameof(serializerFactory));
            _package = package ?? throw new ArgumentNullException(nameof(package));

            IMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (commandService != null)
            {
                var menuCommandId = new CommandID(CommandSet, JsonCommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);

                menuCommandId = new CommandID(CommandSet, XmlCommandId);
                menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);

                // TODO: let plugins add their own menu item.
            }

            InitializeOutputHandlers();
        }

        private void InitializeOutputHandlers()
        {
            foreach (var handler in _outputHandlers)
            {
                handler.Initialize(ServiceProvider);
            }
        }

        private async void MenuItemCallback(object sender, EventArgs e)
        {
            string commandName = GetContentType(((MenuCommand)sender).CommandID.ID);
            await DoWorkAsync(commandName);
        }

        private string GetContentType(int menuCommandId)
        {
            if (menuCommandId == XmlCommandId)
            {
                return "xml";
            }

            return "json";
        }

        private async System.Threading.Tasks.Task DoWorkAsync(string commandName)
        {
            var componentModel = ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            if (componentModel == null)
            {
                return;
            }

            var vsTextManager = ServiceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
            if (vsTextManager == null)
            {
                return;
            }

            IVsTextView activeView;
            ErrorHandler.ThrowOnFailure(vsTextManager.GetActiveView(1, null, out activeView));

            var editorService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var textView = editorService.GetWpfTextView(activeView);

            CaretPosition caretPosition = textView.Caret.Position;
            SnapshotPoint bufferPosition = caretPosition.BufferPosition;
            var document = bufferPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return;
            }

            int position = bufferPosition.Position;
            SemanticModel semanticModel = await document.GetSemanticModelAsync();

            ISymbol selectedSymbol = await GetSymbolUnderCursorAsync(document, semanticModel, position);

            var typeSymbol = selectedSymbol as ITypeSymbol;
            if (typeSymbol == null)
            {
                ShowMessageBox(ServiceProvider, "Invoke this menu on a type name.");
                return;
            }

            // This does the actual magic.
            var classInfo = new TypeSymbolParser().GetMemberInfoRecursive(typeSymbol, semanticModel);

            ShowOutput(classInfo, commandName);
        }

        private void ShowOutput(ClassInfo classInfo, string menuItemName)
        {
            IClassInfoSerializer serializer;
            try
            {
                serializer = _serializerFactory.GetSerializer(menuItemName);
            }
            catch (ArgumentException ex)
            {
                ShowMessageBox(ServiceProvider, $"Error retrieving '{menuItemName}' serializer:" + Environment.NewLine + Environment.NewLine + ex.Message);
                return;
            }

            foreach (var handler in _outputHandlers)
            {
                if (handler.Handle(serializer, classInfo))
                {
                    break;
                }
            }
        }

        private async Task<ISymbol> GetSymbolUnderCursorAsync(TextDocument document, SemanticModel semanticModel, int position)
        {
            Workspace workspace = document.Project.Solution.Workspace;
            var cancellationToken = new CancellationToken();
            ISymbol selectedSymbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace, cancellationToken);
            return selectedSymbol;
        }

        public static bool ShowMessageBox(IServiceProvider serviceProvider, string message)
        {
            string title = "Serialize This";

            return VsShellUtilities.ShowMessageBox(
                            serviceProvider,
                            message,
                            title,
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST) == VSConstants.S_OK;
        }
    }
}