// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using M.Resources.Fonts;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Screens
{
    internal class CustomStorage : NamespacedResourceStore<byte[]>
    {
        private readonly OsuGameBase gameBase;
        private readonly Storage customStorage;
        private readonly Dictionary<Assembly, Type> loadedAssemblies = new Dictionary<Assembly, Type>();

        public List<Font> ActiveFonts = new List<Font>();
        public static bool CustomFontLoaded;

        public CustomStorage(Storage storage, OsuGameBase gameBase)
            : base(new StorageBackedResourceStore(storage), "custom")
        {
            this.gameBase = gameBase;

            customStorage = storage.GetStorageForDirectory("custom");

            prepareFontLoad();

            var instances = loadedAssemblies.Values.Select(r => (Font)Activator.CreateInstance(r)).ToList();

            foreach (var f in instances)
            {
                ActiveFonts.Add(f);
            }
        }

        private void prepareFontLoad()
        {
            //获取custom下面所有以Font.dll结尾的文件
            var fonts = customStorage.GetFiles(".", "*.Font.dll");

            foreach (var font in fonts)
            {
                //获取完整路径
                var fullPath = customStorage.GetFullPath(font);

                //Logger.Log($"加载 {fullPath}");
                addFont(Assembly.LoadFrom(fullPath));
            }
        }

        private void addFont(Assembly assembly)
        {
            //Logger.Log($"尝试添加 {assembly}");

            if (loadedAssemblies.Any(a => a.Key.FullName == assembly.FullName))
                return;

            if (!assembly.GetTypes().First().IsSubclassOf(typeof(Font)))
                return;

            try
            {
                //添加assembly
                var fontType = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Font)));
                loadedAssemblies[assembly] = fontType;

                var currentFontInfo = (Font)Activator.CreateInstance(fontType);

                //添加Store
                gameBase.Resources.AddStore(new DllResourceStore(assembly));

                //加载字体
                gameBase.AddFont(gameBase.Resources, "Fonts/Custom");

                if (currentFontInfo.HaveBlack)
                    gameBase.AddFont(gameBase.Resources, "Fonts/Custom-Black");

                if (currentFontInfo.HaveBold)
                    gameBase.AddFont(gameBase.Resources, "Fonts/Custom-Bold");

                //设置CustomFontLoaded
                CustomFontLoaded = true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "尝试添加字体时出现了问题");
            }
        }
    }
}
