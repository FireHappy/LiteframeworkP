#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;
using UnityEditor;
using UnityEngine;

namespace LiteFramework.EditorTools
{
    public static class UIRouterGeneratorEditor
    {
        // 方法1: 通过程序集名称过滤（最常用）
        public static Assembly[] GetMainAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return assemblies.Where(assembly =>
                assembly.GetName().Name == "Assembly-CSharp" ||           // 主程序集
                assembly.GetName().Name == "Assembly-CSharp-firstpass"   // 预编译程序集
            ).ToArray();
        }
        public static void GenerateRouterFiles(string outputFolder, string nameSpace)
        {
            Debug.Log("⚙️ 正在生成 UIRouter 注册和委托表...");
            var assemblies = GetMainAssemblies();

            var viewTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IView).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                .ToList();

            var sb = new StringBuilder();

            // 文件头
            sb.AppendLine("// 自动生成，请勿手动修改");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using LiteFramework.Module.UI;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{");

            // 单个静态类 UIRouterRegister
            sb.AppendLine("    public static class UIRouterRegister");
            sb.AppendLine("    {");

            // 注册方法，带 RuntimeInitializeOnLoadMethod
            sb.AppendLine("        [RuntimeInitializeOnLoadMethod]");
            sb.AppendLine("        static void Register()");
            sb.AppendLine("        {");

            // 初始化字典
            foreach (var view in viewTypes)
            {
                var presenter = GetPresenterFromView(view);
                if (presenter == null)
                {
                    Debug.LogWarning($"跳过 View：{view.FullName}，未能解析 Presenter。");
                    continue;
                }

                var presenterFull = presenter.FullName;
                var viewFull = view.FullName;

                // 注册代码
                sb.AppendLine($"            UIRouter.Register<{presenterFull}, {viewFull}>();");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }"); // 类结束
            sb.AppendLine("}"); // 命名空间结束

            // 写入文件
            Directory.CreateDirectory(outputFolder);
            File.WriteAllText(Path.Combine(outputFolder, "UIRouterRegister.cs"), sb.ToString());
            AssetDatabase.Refresh();

            Debug.Log($"✅ UIRouter 文件生成完毕，共生成 {viewTypes.Count} 条映射。");
        }

        private static Type GetPresenterFromView(Type viewType)
        {
            var attr = viewType.GetCustomAttribute<BindPresenterAttribute>();
            if (attr != null)
                return attr.PresenterType;

            var baseType = viewType.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(BaseUIView<>))
                    return baseType.GetGenericArguments()[0];

                baseType = baseType.BaseType;
            }

            return null;
        }
    }
}
#endif
