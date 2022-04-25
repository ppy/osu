using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using M.DBus;
using M.DBus.Services.Notifications;
using M.DBus.Tray;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Misc.PluginResolvers;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Internal;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin.Plugins
{
    public class LLinPluginManager : CompositeDrawable
    {
        private readonly BindableList<LLinPlugin> avaliablePlugins = new BindableList<LLinPlugin>();
        private readonly BindableList<LLinPlugin> activePlugins = new BindableList<LLinPlugin>();
        private readonly List<LLinPluginProvider> providers = new List<LLinPluginProvider>();
        private List<string> blockedProviders;

        private readonly ConcurrentDictionary<Type, IPluginConfigManager> configManagers = new ConcurrentDictionary<Type, IPluginConfigManager>();

        [Resolved]
        private Storage storage { get; set; }

        [Resolved(canBeNull: true)]
        [CanBeNull]
        private IDBusManagerContainer<IMDBusObject> dBusManagerContainer { get; set; }

        internal Action<LLinPlugin> OnPluginAdd;
        internal Action<LLinPlugin> OnPluginUnLoad;

        public int PluginVersion => 9;
        public int MinimumPluginVersion => 8;
        private const bool experimental = false;

        public readonly IProvideAudioControlPlugin DefaultAudioController = new OsuMusicControllerWrapper();
        public readonly IFunctionBarProvider DummyFunctionBar = new DummyFunctionBar();

        private readonly LLinPluginResolver resolver;

        private string blockedPluginFilePath => storage.GetFullPath("custom/blocked_plugins.json");

        public LLinPluginManager()
        {
            resolver = new LLinPluginResolver(this);

            InternalChild = (OsuMusicControllerWrapper)DefaultAudioController;
        }

        [CanBeNull]
        internal PluginStore PluginStore;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase gameBase, Storage storage)
        {
            try
            {
                using (var writer = new StreamReader(File.OpenRead(blockedPluginFilePath)))
                {
                    blockedProviders = JsonConvert.DeserializeObject<List<string>>(writer.ReadToEnd())
                                       ?? new List<string>();
                }
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                    Logger.Error(e, "读取黑名单插件列表时出现了问题");

                blockedProviders = new List<string>();
            }

            try
            {
                PluginStore = new PluginStore(storage, gameBase);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"未能初始化插件存储, 本次启动将不会加载任何插件！({e.Message})");
                PluginStore = null;
            }

            if (PluginStore != null)
            {
                foreach (var provider in PluginStore.LoadedPluginProviders)
                {
                    if (!blockedProviders.Contains(provider.GetType().Assembly.ToString()))
                    {
                        AddPlugin(provider.CreatePlugin);
                        providers.Add(provider);
                    }
                }
            }

            resolver.UpdatePluginDictionary(GetAllPlugins(false));

#pragma warning disable 162
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!DebugUtils.IsDebugBuild && experimental)
            {
                Logger.Log($"看上去该版本 ({PluginVersion}) 尚处于实现性阶段。 "
                           + "请留意该版本的任何功能都可能会随时变动。 ",
                    LoggingTarget.Runtime,
                    LogLevel.Important);
            }
#pragma warning restore 162
        }

        public IPluginConfigManager GetConfigManager(LLinPlugin pl) =>
            configManagers.GetOrAdd(pl.GetType(), _ => pl.CreateConfigManager(storage));

        public void RegisterDBusObject(IMDBusObject target)
        {
            if (platformSupportsDBus)
                dBusManagerContainer?.Add(target);
        }

        public void UnRegisterDBusObject(IMDBusObject target)
        {
            if (platformSupportsDBus)
                dBusManagerContainer?.Remove(target);
        }

        public void AddDBusMenuEntry(SimpleEntry entry)
        {
            if (platformSupportsDBus)
                dBusManagerContainer?.AddTrayEntry(entry);
        }

        public void RemoveDBusMenuEntry(SimpleEntry entry)
        {
            if (platformSupportsDBus)
                dBusManagerContainer?.RemoveTrayEntry(entry);
        }

        public void PostSystemNotification(SystemNotification notification)
        {
            if (platformSupportsDBus)
                dBusManagerContainer?.PostSystemNotification(notification);
        }

        private bool platformSupportsDBus => RuntimeInfo.OS == RuntimeInfo.Platform.Linux;

        internal bool AddPlugin(LLinPlugin pl)
        {
            if (avaliablePlugins.Contains(pl) || pl == null) return false;

            if (pl.Version < MinimumPluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为旧版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);
            else if (pl.Version > PluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为更高版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);

            avaliablePlugins.Add(pl);
            OnPluginAdd?.Invoke(pl);
            return true;
        }

        internal bool UnLoadPlugin(LLinPlugin pl, bool blockFromFutureLoad = false)
        {
            if (!avaliablePlugins.Contains(pl) || pl == null) return false;

            var provider = providers.Find(p => p.CreatePlugin.GetType() == pl.GetType());

            activePlugins.Remove(pl);
            avaliablePlugins.Remove(pl);
            providers.Remove(provider);

            try
            {
                if (pl is IFunctionBarProvider functionBarProvider)
                    resolver.RemoveFunctionBarProvider(functionBarProvider);

                if (pl is IProvideAudioControlPlugin provideAudioControlPlugin)
                    resolver.RemoveAudioControlProvider(provideAudioControlPlugin);

                pl.UnLoad();
                OnPluginUnLoad?.Invoke(pl);

                var providerAssembly = provider.GetType().Assembly;
                var gameAssembly = GetType().Assembly;

                if (providerAssembly != gameAssembly && blockFromFutureLoad)
                {
                    blockedProviders.Add(providerAssembly.ToString());

                    using (var writer = new StreamWriter(File.OpenWrite(blockedPluginFilePath)))
                    {
                        string serializedString = JsonConvert.SerializeObject(blockedProviders);
                        writer.Write(serializedString);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"卸载插件时出现了问题: {e.Message}");

                //直接dispose掉插件
                if (pl.Parent is Container container)
                {
                    container.Remove(pl);
                    pl.Dispose();
                }

                //刷新列表
                resolver.UpdatePluginDictionary(GetAllPlugins(false));
            }

            return true;
        }

        internal bool ActivePlugin(LLinPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || activePlugins.Contains(pl) || pl == null) return false;

            if (!activePlugins.Contains(pl))
                activePlugins.Add(pl);

            bool success = pl.Enable();

            if (!success)
                activePlugins.Remove(pl);

            return success;
        }

        internal bool DisablePlugin(LLinPlugin pl)
        {
            if (!avaliablePlugins.Contains(pl) || !activePlugins.Contains(pl) || pl == null) return false;

            activePlugins.Remove(pl);
            bool success = pl.Disable();

            if (!success)
            {
                activePlugins.Add(pl);
                Logger.Log($"卸载插件\"${pl.Name}\"失败");
            }

            return success;
        }

        public List<LLinPlugin> GetActivePlugins() => activePlugins.ToList();

        /// <summary>
        /// 获取所有插件
        /// </summary>
        /// <param name="newInstance">
        /// 是否处理当前所有插件并创建新插件本体<br/>
        /// </param>
        /// <returns>所有已加载且可用的插件</returns>
        public List<LLinPlugin> GetAllPlugins(bool newInstance)
        {
            if (newInstance)
            {
                //bug: 直接调用Dispose会导致快速进出时抛出Disposed drawabled may never in the scene graph
                ExpireOldPlugins();

                foreach (var p in providers)
                {
                    avaliablePlugins.Add(p.CreatePlugin);
                }

                resolver.UpdatePluginDictionary(avaliablePlugins.ToList());
            }

            return avaliablePlugins.ToList();
        }

        internal void ExpireOldPlugins()
        {
            foreach (var pl in avaliablePlugins)
            {
                activePlugins.Remove(pl);
                pl.Expire();
            }

            avaliablePlugins.Clear();
        }

        internal List<IFunctionBarProvider> GetAllFunctionBarProviders() => resolver.GetAllFunctionBarProviders();

        internal List<IProvideAudioControlPlugin> GetAllAudioControlPlugin() => resolver.GetAllAudioControlPlugin();

        internal IProvideAudioControlPlugin GetAudioControlByPath([NotNull] string path) => resolver.GetAudioControlPluginByPath(path);
        internal IFunctionBarProvider GetFunctionBarProviderByPath([NotNull] string path) => resolver.GetFunctionBarProviderByPath(path);

        public string ToPath([NotNull] object target) => resolver.ToPath(target);
    }
}
