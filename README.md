# SerializeThis

## Introduction
SerializeThis is a [Visual Studio Extension (Marketplace)](https://marketplace.visualstudio.com/items?itemName=CodeCaster.CodeCasterSerializeThis). It lets you generate an example JSON for a class by right-clicking a type, variable or member name. This can be helpful to generate example JSON or C# object initializer code to use in unit tests or through a REST client such as [Insomnia](https://insomnia.rest/).

This is not meant as a replacement for documentation and client generators such as [Swagger](http://swagger.io/).

It currently looks like this:

![SerializeThis Screenshot](./static/images/Serialize_This-Menu.png)

The serialized model looks like this:

![SerializeThis Screenshot](./static/images/Serialize_This-Serialized.png)

The extension creates a temp file, writes the output there, opens the file in Visual Studio and then attempts to delete the file. 

## Solution design

### SerializeThis.Serialization
The Serialization library is a comprehensive bridge between Roslyn, Reflection and the Visual Studio Debugger on the input side, and C# object initializers and JSON output on the other side. Ideally it works all ways, so it can replace existing Json to C# generators.

### Input libraries
* SerializeThis.Serialization.Roslyn: parse the open file and its context in the IDE, design-time.
* SerializeThis.Reflection: runtime experiment, not quite complete. And why would you use it when there's like JsonConvert.SerializeObject(), and otherwise ... this extension? (May be) useful in some unit tests though.

### Output libraries
* SerializeThis.Serialization.CSharp: creates object initializers.
* SerializeThis.Serialization.Json: generates JSON.

### Visual Studio Extension
The startup project, SerializeThis.VS2019. Contains a tad too much logic.

VS2022 will require a new strategy and solution.

### Tests
A couple of test projects, not quite covered.

## Feedback
Its basic functionality has been tested in various scenarios, but if you have a type it can't serialize, feel free to [open an issue](https://github.com/CodeCasterNL/SerializeThis/issues)!

## Building and Running
This output project is a Visual Studio Extension, so you'll need to install the [Visual Studio 2019 SDK](https://docs.microsoft.com/en-us/visualstudio/extensibility/installing-the-visual-studio-sdk?view=vs-201) in order to compile it. The startup project requres Visual Studio 2019 to open and build, but the extension should work in Visual Studio 2015, 2017 and 2019. Versions after 1.2.1 were only tested in VS 2019.

Start debugging by running the `src/SerializeThis.Extension.VS2019` project from its solution, which starts an experimental instance of Visual Studio, where the extension will be loaded. You can then open any C# file, right-click a type name and see the "Serialize As" submenu.

The experimental instance has its own file history, so click "Continue without code" on the startup screen, click `File -> Open -> File...` and select the file you wish to test, for example the file `JsonTestClasses.cs` in the root of this repository. On later runs of the experimental instance, you can then click `File -> Recent Files -> ...\JsonTestClasses.cs` so you won't have to browse to it again.

## Known issues
* Must fix: I broke recursion.
* Nice to have: support runtime/assigned types being different from declared types (`object[] foo = new { "Bar", null, 42, new Foo { Bar = "Baz" } }`).
* Refactor: add interface/struct/... detection, so we won't generate `new ICollection<string>` but emit nothing or a comment. Or find and instantiate known interface types?
* * E.g. `IList<T> = new List<T>, IEnumerable<T> = new Collection<T>, ...`?

