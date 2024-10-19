using System;
using System.Dynamic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using TGZG.CSharpScript.Running;

namespace TestConsole {
	public static class ExtensionMethods {
		public static void Log(this object message) {
			Console.WriteLine(message);
		}
	}
	public class Program {
		//宿主字段
		public static class 你好 {
			public static double 数据 = 100000;
		}
		public class Globals {
			public double 俯仰;
			public double 偏航;
			public double 滚转;
		}
		public static void Main(string[] args) {
			//File Running Example
			var 脚本路径 = Path.Combine(Environment.CurrentDirectory, "ExampleFile.cssc");
			while (true) {
				//CSharpScriptRunning.RunFileAsAction(脚本路径, true);
				CSharpScriptRunning.RunFileAsFunc<double>(脚本路径, true);
				你好.数据.Log();//宿主操作示例
				Console.ReadKey();
			}
			//获取当前程序集
			//三个脚本方式：
			//1.脚本操作宿主
			//CSharpScriptRunning.RunFileAsAction(脚本路径, true);
			//CSharpScriptRunning.RunFileAsFunc<T>(脚本路径, true);
			//2.宿主的外接模块
			//CSharpScriptRunning.RunFileAsAction(脚本路径, false);
			//CSharpScriptRunning.RunFileAsFunc<T>(脚本路径, false);
			//3.完整.cs代码文件自动解析为脚本
			//TODO：待写
		}
	}
}
