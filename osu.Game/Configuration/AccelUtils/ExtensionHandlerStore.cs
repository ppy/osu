// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Configuration.AccelUtils
{
    internal class ExtensionHandlerStore : NamespacedResourceStore<byte[]>
    {
        private readonly Storage customStorage;
        private readonly Dictionary<Assembly, Type> loadedAssemblies = new Dictionary<Assembly, Type>();

        public ExtensionHandlerStore(Storage storage)
            : base(new StorageBackedResourceStore(storage), "custom")
        {
            customStorage = storage.GetStorageForDirectory("custom");

            prepareLoad();
        }

        public bool Contains(string path)
        {
            try
            {
                customStorage.Exists(path);
                return true;
            }
            catch (Exception e)
            {
                if (!(e is ArgumentException))
                    Logger.Error(e, "获取文件路径时发生了错误");

                return false;
            }
        }

        private void prepareLoad()
        {
            //获取custom下面所有以Font.dll、.Mvis.dll结尾的文件
            var files = customStorage.GetFiles(".", "M.AccelPlugin.*.dll");

            try
            {
                //加载自带的Mvis插件
                //From RulesetStore
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name = assembly.GetName().Name ?? "";

                    if (!name.StartsWith("M.AccelPlugin", StringComparison.Ordinal))
                        continue;

                    loadAssembly(assembly);
                }

                //从程序根目录加载
                if (RuntimeInfo.IsDesktop)
                {
                    foreach (string file in Directory.GetFiles(RuntimeInfo.StartupDirectory, "M.AccelPlugin.*.dll"))
                        loadAssembly(Assembly.LoadFrom(file));
                }

                foreach (string file in files)
                {
                    //获取完整路径
                    string fullPath = customStorage.GetFullPath(file);

                    //Logger.Log($"加载 {fullPath}");
                    loadAssembly(Assembly.LoadFrom(fullPath));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"载入文件时出现问题: {e.Message}");
            }
        }

        /// <summary>
        /// 加载一个Assembly
        /// </summary>
        /// <param name="assembly">要加载的Assembly</param>
        private void loadAssembly(Assembly assembly)
        {
            if (loadedAssemblies.Any(a => a.Key.FullName == assembly.FullName))
                return;

            string name = Path.GetFileNameWithoutExtension(assembly.Location);
            if (loadedAssemblies.Values.Any(t => Path.GetFileNameWithoutExtension(t.Assembly.Location) == name))
                return;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    //Logger.Log($"case: 尝试加载 {type} ... {ok}");

                    if (type.GetInterfaces().Contains(typeof(IExtensionHandler)))
                    {
                        loadedAssemblies[assembly] = type;
                        //Logger.Log($"{type}是插件Provider");

                        if (assembly.FullName != null)
                            addMvisPlugin(type, assembly.FullName);
                        else
                            Logger.Log($"{assembly}没有FullName，将不会加载");
                    }

                    //Logger.Log($"{type}不是任何一个SubClass");
                }

                //添加store
                //gameBase.Resources.AddStore(new DllResourceStore(assembly));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"载入 {assembly.FullName} 时出现了问题, 请联系你的插件提供方。");
            }
        }

        /// <summary>
        /// 向CustomStore添加一个插件
        /// </summary>
        /// <param name="type">要添加的插件</param>
        /// <param name="fullName">与pluginType对应的Assembly的fullName</param>
        private void addMvisPlugin(Type type, string fullName)
        {
            //Logger.Log($"载入 {fullName}");

            try
            {
                var handlerInstance = (IExtensionHandler)Activator.CreateInstance(type)!;
                AccelExtensionsUtil.AddHandler(handlerInstance);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"尝试添加插件{fullName}时出现了问题");
            }
        }
    }
}
