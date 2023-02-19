using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;

namespace osu.Game.Screens.LLin.Plugins.Internal.LuaSupport
{
    public class DefaultImportGenerator
    {
        private static string? generatedImport;

        public static string Generate(string[] namespaces, bool forceRegen = false)
        {
            string content = "";

            if (generatedImport != null && !forceRegen)
                return generatedImport;

            var frameworkTypes = Assembly.GetAssembly(typeof(SpriteText))?.GetTypes().ToList();

            foreach (string ns in namespaces)
            {
                var validNameSpaces = new List<string>();

                try
                {
                    var types = Assembly.GetExecutingAssembly().GetTypes()
                                        .Where(type => type.Namespace != null && type.Namespace.Contains(ns) && type.IsClass && type.IsPublic)
                                        .ToList();

                    var frameworkAsmTypes = frameworkTypes?.Where(type => type.Namespace != null && type.Namespace.Contains(ns) && type.IsClass && type.IsPublic)
                                                          .ToList();

                    if (frameworkAsmTypes != null)
                    {
                        frameworkTypes!.RemoveAll(t => frameworkAsmTypes.Contains(t));
                        types.AddRange(frameworkAsmTypes);
                    }

                    foreach (var type in types)
                    {
                        var nameSpace = type.Namespace!;

                        if (!validNameSpaces.Contains(nameSpace))
                        {
                            validNameSpaces.Add(nameSpace);
                            content += $"\nimport(\"{type.Namespace}\")";
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "?");
                }
            }

            generatedImport = content;

            return content;
        }
    }
}
