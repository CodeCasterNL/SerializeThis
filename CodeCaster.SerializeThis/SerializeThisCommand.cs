//------------------------------------------------------------------------------
// <copyright file="SerializeThisCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeCaster.SerializeThis
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SerializeThisCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c2c4513d-ca4c-4b91-be0d-b797460e7572");

        private readonly DTE2 DTE;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeThisCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private SerializeThisCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            DTE = (DTE2)ServiceProvider.GetService(typeof(DTE));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SerializeThisCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new SerializeThisCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void MenuItemCallback(object sender, EventArgs e)
        {
            await DoWorkAsync();
        }

        private async System.Threading.Tasks.Task DoWorkAsync()
        {

            var componentModel = ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            var visualStudioWorkspace = componentModel.GetService<VisualStudioWorkspace>();
            IVsTextView activeView = null;
            ErrorHandler.ThrowOnFailure((Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager).GetActiveView(1, null, out activeView));

            var textView = componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(activeView);

            CaretPosition caretPosition = textView.Caret.Position;
            SnapshotPoint bufferPosition = caretPosition.BufferPosition;
            var document = bufferPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return;
            }

            caretPosition = textView.Caret.Position;
            bufferPosition = caretPosition.BufferPosition;
            int position = bufferPosition.Position;
            CancellationToken cancellationToken = new CancellationToken();
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            ISymbol selectedSymbol = await GetSymbolUnderCursorAsync(document, semanticModel, position);

            var typeSymbol = selectedSymbol as ITypeSymbol;
            if (typeSymbol == null)
            {
                this.ShowMessageBox("Invoke this menu on a type name.");
                return;
            }

            GetMemberInfoRecursive(typeSymbol, semanticModel);
        }

        private void GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            string memberInfo = GetMemberInfoRecursive(typeSymbol);
            ShowMessageBox(memberInfo);
        }

        private string GetMemberInfoRecursive(ITypeSymbol typeSymbol)
        {
            string memberInfo = "";

            if (typeSymbol.BaseType != null)
            {
                memberInfo += GetMemberInfoRecursive(typeSymbol.BaseType);
            }

            memberInfo += $"Public properties of {typeSymbol.Name}:{Environment.NewLine}";

            // TOOD: get members down the inheritance tree
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public)
                {
                    memberInfo += " - " + member.Name;

                    // TODO: add support for all known types, visitor pattern?
                    var typedMember = member as IPropertySymbol;
                    var memberType = typedMember.Type;
                    switch (memberType.SpecialType)
                    {
                        case SpecialType.System_String:
                            memberInfo += " (string)";
                            break;
                        case SpecialType.System_DateTime:
                            memberInfo += " (DateTime)";
                            break;
                        case SpecialType.System_Int32:
                            memberInfo += " (int)";
                            break;
                        case SpecialType.System_Int64:
                            memberInfo += " (long)";
                            break;

                        // TODO: below doesn't seem to work, try if the type implements System.Collections.IEnumerable to emit a collection (after IDictionary<>)?
                        case SpecialType.System_Collections_Generic_ICollection_T:
                        case SpecialType.System_Collections_Generic_IEnumerable_T:
                        case SpecialType.System_Collections_Generic_IEnumerator_T:
                        case SpecialType.System_Collections_Generic_IList_T:
                        case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                        case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                        case SpecialType.System_Collections_IEnumerable:
                        case SpecialType.System_Collections_IEnumerator:
                            memberInfo += " (collection<T>)";
                            break;
                    }

                    memberInfo += Environment.NewLine;

                }
            }

            return memberInfo;
        }

        private async Task<ISymbol> GetSymbolUnderCursorAsync(Microsoft.CodeAnalysis.Document document, SemanticModel semanticModel, int position)
        {
            Workspace workspace = document.Project.Solution.Workspace;
            var cancellationToken = new CancellationToken();
            ISymbol selectedSymbol = await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace, cancellationToken);
            return selectedSymbol;
        }

        private void ShowMessageBox(string message)
        {
            string title = "Serialize This";

            VsShellUtilities.ShowMessageBox(
                            this.ServiceProvider,
                            message,
                            title,
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}