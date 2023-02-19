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
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Misc.PluginResolvers;
using osu.Game.Screens.LLin.Plugins.Config;
using osu.Game.Screens.LLin.Plugins.Internal.DummyAudio;
using osu.Game.Screens.LLin.Plugins.Internal.DummyBase;
using osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar;
using osu.Game.Screens.LLin.Plugins.Internal.LuaSupport;
using osu.Game.Screens.LLin.Plugins.Types;
using osu.Game.Screens.LLin.Plugins.Types.SettingsItems;

namespace osu.Game.Screens.LLin.Plugins
{
    public partial class LLinPluginManager : CompositeDrawable
    {
        #region 插件管理

        private readonly BindableList<LLinPlugin> avaliablePlugins = new BindableList<LLinPlugin>();
        private readonly BindableList<LLinPlugin> activePlugins = new BindableList<LLinPlugin>();
        private readonly List<LLinPluginProvider> providers = new List<LLinPluginProvider>();
        private readonly List<string> blockedProviders = new List<string>();

        private readonly LLinPluginResolver resolver;

        private string blockedPluginFilePath => storage.GetFullPath("custom/blocked_plugins.json");

        #endregion

        #region 插件配置

        private readonly ConcurrentDictionary<Type, IPluginConfigManager> configManagers = new ConcurrentDictionary<Type, IPluginConfigManager>();
        private readonly ConcurrentDictionary<Type, SettingsEntry[]> entryMap = new ConcurrentDictionary<Type, SettingsEntry[]>();

        #endregion

        #region 依赖

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private IDBusManagerContainer<IMDBusObject>? dBusManagerContainer { get; set; }

        public readonly IProvideAudioControlPlugin DefaultAudioController = new OsuMusicControllerWrapper();

        public readonly TypeWrapper DefaultFunctionBarType = new TypeWrapper
        {
            Type = typeof(FunctionBar),
            Name = "默认底栏"
        };

        public readonly TypeWrapper DefaultAudioControllerType = new TypeWrapper
        {
            Type = typeof(OsuMusicControllerWrapper),
            Name = "osu!"
        };

        #endregion

        #region 内部方法/参数

        internal Action<LLinPlugin>? OnPluginAdd;
        internal Action<LLinPlugin>? OnPluginUnLoad;

        internal static int LatestPluginVersion => 10;

        internal SettingsEntry[]? GetSettingsFor(LLinPlugin pl)
        {
            if (!entryMap.ContainsKey(pl.GetType()))
                Logger.Log($"entryMap中没有和{pl}有关的数据。");

            return entryMap.ContainsKey(pl.GetType()) ? entryMap[pl.GetType()] : null;
        }

        internal List<TypeWrapper> GetAllFunctionBarProviders() => resolver.GetAllFunctionBarProviders();

        internal List<TypeWrapper> GetAllAudioControlPlugin() => resolver.GetAllAudioControlPlugin();

        internal Type? GetAudioControlTypeByPath([NotNull] string path) => resolver.GetAudioControlPluginByPath(path);
        internal Type? GetFunctionBarProviderTypeByPath([NotNull] string path) => resolver.GetFunctionBarProviderByPath(path);

        internal IProvideAudioControlPlugin? GetAudioControlByPath([NotNull] string path)
            => (IProvideAudioControlPlugin?)avaliablePlugins.FirstOrDefault(pl => pl is IProvideAudioControlPlugin && resolver.ToPath(pl) == path);

        internal IFunctionBarProvider? GetFunctionBarProviderByPath([NotNull] string path)
            => (IFunctionBarProvider?)avaliablePlugins.FirstOrDefault(pl => pl is IFunctionBarProvider && resolver.ToPath(pl) == path);

        private bool platformSupportsDBus => RuntimeInfo.OS == RuntimeInfo.Platform.Linux;

        internal bool AddPlugin(LLinPlugin? pl)
        {
            if (pl == null || avaliablePlugins.Contains(pl)) return false;

            if (pl.Version < MinimumPluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为旧版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);
            else if (pl.Version > PluginVersion)
                Logger.Log($"插件 \"{pl.Name}\" 是为更高版本的mf-osu打造的, 继续使用可能会导致意外情况的发生!", LoggingTarget.Runtime, LogLevel.Important);

            avaliablePlugins.Add(pl);
            OnPluginAdd?.Invoke(pl);

            pl.PluginManager = this;
            return true;
        }

        internal bool UnLoadPlugin(LLinPlugin? pl, bool blockFromFutureLoad = false)
        {
            if (pl == null || !avaliablePlugins.Contains(pl)) return false;

            var provider = providers.Find(p => p.CreatePlugin.GetType() == pl.GetType());

            activePlugins.Remove(pl);
            avaliablePlugins.Remove(pl);

            if (provider != null)
                providers.Remove(provider);

            try
            {
                if (pl is IFunctionBarProvider functionBarProvider)
                    resolver.RemoveFunctionBarProvider(functionBarProvider);

                if (pl is IProvideAudioControlPlugin provideAudioControlPlugin)
                    resolver.RemoveAudioControlProvider(provideAudioControlPlugin);

                pl.UnLoad();
                OnPluginUnLoad?.Invoke(pl);

                var providerAssembly = provider?.GetType().Assembly;
                var gameAssembly = GetType().Assembly;

                if (providerAssembly != null && providerAssembly != gameAssembly && blockFromFutureLoad)
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
                    container.Remove(pl, true);

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

        internal void ExpireOldPlugins()
        {
            foreach (var pl in avaliablePlugins)
            {
                activePlugins.Remove(pl);
                pl.Expire();
            }

            avaliablePlugins.Clear();
        }

        #endregion

        #region API相关

        public int PluginVersion => LatestPluginVersion;
        public int MinimumPluginVersion => 9;

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

        #endregion

        internal PluginStore? PluginStore;

        public LLinPluginManager()
        {
            resolver = new LLinPluginResolver(this);

            InternalChild = (OsuMusicControllerWrapper)DefaultAudioController;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase gameBase, MConfigManager config)
        {
            try
            {
                using (var writer = new StreamReader(File.OpenRead(blockedPluginFilePath)))
                {
                    var obj = JsonConvert.DeserializeObject<List<string>>(writer.ReadToEnd());

                    if (obj != null)
                        blockedProviders.AddRange(obj);
                }
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                    Logger.Error(e, "读取黑名单插件列表时出现了问题");
            }

            try
            {
                PluginStore = new PluginStore(storage, gameBase);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"未能初始化插件存储, 本次启动将不会加载任何外部插件！({e.Message})");
                PluginStore = null;
            }

            DummyBasePluginProvider dbpp;
            DummyAudioPluginProvider dapp;
            LuaPluginProvider luapp;
            providers.AddRange(new LLinPluginProvider[]
            {
                dbpp = new DummyBasePluginProvider(config, this),
                dapp = new DummyAudioPluginProvider(config, this),
                luapp = new LuaPluginProvider()
            });

            AddPlugin(dbpp.CreatePlugin);
            AddPlugin(dapp.CreatePlugin);
            AddPlugin(luapp.CreatePlugin);

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

            foreach (var pl in this.GetAllPlugins(false))
            {
#pragma warning disable CS0618
                var oldEntries = pl.GetSettingEntries();
#pragma warning restore CS0618

                if (oldEntries != null)
                    entryMap[pl.GetType()] = oldEntries;
                else
                    entryMap[pl.GetType()] = pl.GetSettingEntries(GetConfigManager(pl));
            }
        }

        public string ToPath([NotNull] object target) => resolver.ToPath(target);
    }
}
