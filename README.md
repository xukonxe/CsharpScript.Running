# CsharpScript.Running
- [English](README.md) | [简体中文](README.zh.md) 
- Related repositorie：[CsharpScript.Console C#Script Runtime Console](https://github.com/xukonxe/CSharpScript.Console/tree/master)
This repository contains a lightweight C# script execution engine built on top of the Microsoft.CodeAnalysis.CSharp.Scripting library. It allows developers to dynamically execute C# scripts with support for:

- **Global Variables**: Set global context for scripts with user-defined objects.
- **Dynamic Imports**: Add namespaces such as `System.Linq` and others at runtime.
- **Assembly References**: Dynamically reference assemblies during script execution.
- **Script Evaluation**: Run C# scripts with both `void` and return types.
- **Logging Support**: Easily log messages, warnings, and errors through extension methods.

### Key Features
- **Set Global Variables**: Pass an object with fields that can be accessed and modified within the script.
- **Namespace Imports**: Add commonly used namespaces dynamically, such as `System.Linq`.
- **Assembly References**: Reference assemblies at runtime to unlock additional functionality.
- **Typed Script Execution**: Execute scripts and return results with a specific type.
- **Logging**: Built-in logging methods to output logs, warnings, and errors to the console for easier debugging.

### Example Usage

```csharp
public class Globals {
    public string name = "undefined";
    public int age = 0;
}

new CSharpScriptRunning()
    .SetGlobals(new Globals())
    .Import("System.Linq")
    .Reference(typeof(Enumerable).Assembly)
    .Run<int>("age = 10; return age;")
    .Log();
```

This example demonstrates setting a global variable, importing namespaces, adding assembly references, and running a script with a return type.

Feel free to explore the repository and contribute!
