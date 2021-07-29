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
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = System.IServiceProvider;

namespace CodeCaster.SerializeThis.Extension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SerializeThisCommand
    {
        public const int JsonCommandId = 0x0100;
        public const int XmlCommandId = 0x0101;
        public const int CSharpCommandId = 0x0102;

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

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is IMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(CommandSet, JsonCommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);

                menuCommandId = new CommandID(CommandSet, XmlCommandId);
                menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);

                menuCommandId = new CommandID(CommandSet, CSharpCommandId);
                menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);

                // TODO: let other plugins add their own menu item to this plugin's context menu.
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
            try
            {
                var commandName = GetContentType(((MenuCommand)sender).CommandID.ID);
                await DoWorkAsync(commandName);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ServiceProvider, "Error serializing: " + ex);
            }
        }

        private static string GetContentType(int menuCommandId)
        {
            switch (menuCommandId)
            {
                case XmlCommandId: return "xml";
                case JsonCommandId: return "json";
                case CSharpCommandId: return "c#";
                default: throw new ArgumentException(nameof(menuCommandId));
            }
        }

        private async System.Threading.Tasks.Task DoWorkAsync(string commandName)
        {
            if (!(ServiceProvider.GetService(typeof(SComponentModel)) is IComponentModel componentModel))
            {
                return;
            }

            if (!(ServiceProvider.GetService(typeof(SVsTextManager)) is IVsTextManager vsTextManager))
            {
                return;
            }

            ErrorHandler.ThrowOnFailure(vsTextManager.GetActiveView(1, null, out var activeView));

            var editorService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var textView = editorService.GetWpfTextView(activeView);

            var caretPosition = textView.Caret.Position;
            var bufferPosition = caretPosition.BufferPosition;
            var document = bufferPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return;
            }

            var position = bufferPosition.Position;
            var semanticModel = await document.GetSemanticModelAsync();

            var selectedSymbol = await GetSymbolUnderCursorAsync(document, semanticModel, position);

            ITypeSymbol symbolToSerialize;

            switch (selectedSymbol)
            {
                // 'Foo' in `var f = new Foo { ... }`.
                case IMethodSymbol methodSymbol when methodSymbol.MethodKind == MethodKind.Constructor:
                    symbolToSerialize = methodSymbol.ReceiverType;
                    break;
                // 'Foo' in `public class Foo { ... }`, `public Foo F { get; set; }`.
                case ITypeSymbol typeSymbol:
                    symbolToSerialize = typeSymbol;
                    break;
                default:
                    ShowMessageBox(ServiceProvider, "Invoke this menu on a type name.");
                    return;
            }

            // This does the actual magic.
            var classInfo = new Serialization.Roslyn.TypeSymbolParser().GetMemberInfoRecursive(symbolToSerialize, null/*, semanticModel*/);
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

            if (!serializer.CanSerialize(classInfo))
            {
                ShowMessageBox(ServiceProvider, $"Could not serialize {classInfo.Name} to {serializer.DisplayName}");
                return;
            }

            foreach (var handler in _outputHandlers)
            {
                try
                {
                    if (handler.Handle(serializer, classInfo))
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    ShowMessageBox(ServiceProvider, $"Error serializing {classInfo.Name}: {e}");
                    return;
                }
            }

            ShowMessageBox(ServiceProvider, $"Could not find a handler for {classInfo.Name}");

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