## CsharpScript.Running
[English](README.md) | [简体中文](README.zh.md) 
其他相关仓库：[CsharpScript.Console C#运行时脚本控制台](https://github.com/xukonxe/CSharpScript.Console/tree/master)

该仓库包含一个基于 C#Scripting 库的轻量级 C# 脚本执行引擎，支持开发者动态执行 C# 脚本，无需重新编译整个程序。具有以下功能：

- **全局变量**：轻松自定义脚本的传入变量。
- **动态导入**：在运行时添加命名空间，如 `System.Linq` 等。
- **程序集引用**：在脚本执行时动态引用程序集。
- **脚本求值**：将 C# 脚本作为函数运行，并支持 `void` 和返回类型。
- **日志支持**：通过扩展方法轻松输出日志、警告和错误信息。

### 主要特性
- **设置全局变量**：传递一个自定义对象，作为脚本变量区域。
- **命名空间导入**：可动态添加常用的命名空间，如 `System.Linq`。
- **程序集引用**：在运行时引用程序集以使用现成的其他c#库。
- **类型化脚本执行**：执行脚本并返回特定类型的结果。
- **日志功能**：内置日志方法，支持输出日志、警告和错误到控制台，方便调试。

### 示例用法

```csharp
//Globals为自定义类，系统自动将其中字段和值解析为脚本的公共变量。
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

此示例展示了如何设置全局变量、导入命名空间、添加程序集引用，并执行带有返回类型的脚本。

欢迎探索该仓库并参与贡献！
