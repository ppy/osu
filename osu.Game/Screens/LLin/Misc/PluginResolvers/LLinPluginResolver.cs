using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin.Misc.PluginResolvers
{
    public class LLinPluginResolver
    {
        private readonly LLinPluginManager pluginManager;

        public LLinPluginResolver(LLinPluginManager pluginManager)
        {
            this.pluginManager = pluginManager;
        }

        public string ToPath(object target)
        {
            Type targetType;

            if (target is Type) targetType = (Type)target;
            else if (target is TypeWrapper) targetType = ((TypeWrapper)target).Type;
            else targetType = target.GetType();

            return targetType.Name + "@" + targetType.Namespace;
        }

        internal bool RemoveFunctionBarProvider(IFunctionBarProvider functionBarProvider)
            => functionBarDictionary.Remove(ToPath(functionBarProvider), out _);

        internal bool RemoveAudioControlProvider(IProvideAudioControlPlugin provideAudioControlPlugin)
            => audioPluginDictionary.Remove(ToPath(provideAudioControlPlugin), out _);

        private readonly ConcurrentDictionary<string, TypeWrapper> audioPluginDictionary = new ConcurrentDictionary<string, TypeWrapper>();
        private readonly ConcurrentDictionary<string, TypeWrapper> functionBarDictionary = new ConcurrentDictionary<string, TypeWrapper>();

        internal void UpdatePluginDictionary(List<LLinPlugin> newPluginList)
        {
            functionBarDictionary.Clear();
            audioPluginDictionary.Clear();

            foreach (var plugin in newPluginList)
            {
                string pluginPath = ToPath(plugin);

                if (plugin is IFunctionBarProvider functionBarProvider)
                {
                    var typeWrapper = new TypeWrapper
                    {
                        Type = functionBarProvider.GetType(),
                        Name = $"{plugin.Name} ({plugin.Author})"
                    };
                    functionBarDictionary[pluginPath] = typeWrapper;
                }

                if (plugin is IProvideAudioControlPlugin audioControlPlugin)
                {
                    var typeWrapper = new TypeWrapper
                    {
                        Type = audioControlPlugin.GetType(),
                        Name = $"{plugin.Name} ({plugin.Author})"
                    };
                    audioPluginDictionary[pluginPath] = typeWrapper;
                }
            }

            var defaultAudio = pluginManager.DefaultAudioControllerType;
            var defaultFunctionbar = pluginManager.DefaultFunctionBarType;

            audioPluginDictionary[ToPath(defaultAudio)] = defaultAudio;
            functionBarDictionary[ToPath(defaultFunctionbar)] = defaultFunctionbar;
        }

        internal Type? GetAudioControlPluginByPath(string path)
        {
            TypeWrapper? result;
            if (audioPluginDictionary.TryGetValue(path, out result))
                return result.Type;

            return null;
        }

        internal Type? GetFunctionBarProviderByPath(string path)
        {
            TypeWrapper? result;
            if (functionBarDictionary.TryGetValue(path, out result))
                return result.Type;

            return null;
        }

        private List<TypeWrapper> cachedAudioControlPluginList;

        internal List<TypeWrapper> GetAllAudioControlPlugin()
        {
            var list = new List<TypeWrapper>();

            foreach (var keyPair in audioPluginDictionary)
            {
                list.Add(keyPair.Value);
            }

            if (cachedAudioControlPluginList == null || cachedAudioControlPluginList != list)
                cachedAudioControlPluginList = list;

            return list;
        }

        private List<TypeWrapper> cachedFunctionBarPluginList;

        internal List<TypeWrapper> GetAllFunctionBarProviders()
        {
            var list = new List<TypeWrapper>();

            foreach (var keyPair in functionBarDictionary)
            {
                list.Add(keyPair.Value);
            }

            if (cachedFunctionBarPluginList == null || cachedFunctionBarPluginList != list)
                cachedFunctionBarPluginList = list;

            return list;
        }
    }
}
