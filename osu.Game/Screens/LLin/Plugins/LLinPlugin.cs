using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace osu.Game.Screens.LLin.Plugins
{
    public abstract class LLinPlugin : Container
    {
        /// <summary>
        /// 加载插件要提供的内容
        /// </summary>
        /// <returns>要加载的Drawable</returns>
        protected abstract Drawable CreateContent();

        /// <summary>
        /// 为游戏设置创建设置页面
        /// </summary>
        /// <returns>创建的设置页面</returns>
        public virtual PluginSettingsSubSection CreateSettingsSubSection() => null;

        /// <summary>
        /// 为Mvis侧边栏创建设置页面
        /// </summary>
        /// <returns>创建的设置页面</returns>
        public virtual PluginSidebarSettingsSection CreateSidebarSettingsSection() => null;

        public virtual IPluginConfigManager CreateConfigManager(Storage storage) => null;

        /// <summary>
        /// 内容加载完毕后要执行的步骤
        /// </summary>
        /// <param name="content"></param>
        /// <returns>
        /// true: 没有错误<br/>
        /// false: 出现错误
        /// </returns>
        protected abstract bool OnContentLoaded(Drawable content);

        /// <summary>
        /// 加载内容前要执行的步骤
        /// </summary>
        /// <returns>
        /// true: 没有错误<br/>
        /// false: 出现错误
        /// </returns>
        protected abstract bool PostInit();

        /// <summary>
        /// 插件对应的侧边栏页面(未完全实现)
        /// </summary>
        public virtual PluginSidebarPage CreateSidebarPage() => null;

        /// <summary>
        /// 插件Flags，决定了插件的一系列属性
        /// <seealso cref="PluginFlags"/>
        /// </summary>
        public readonly List<PluginFlags> Flags = new List<PluginFlags>();

        /// <summary>
        /// 目标位置，决定插件要在哪里被添加
        /// <seealso cref="TargetLayer"/>
        /// </summary>
        public virtual TargetLayer Target => TargetLayer.Background;

        public override string ToString() => $"{Author} - {Name} ({Description}) [{Version}]";

        public string Description = "插件描述";
        public string Author = "插件作者";

        public abstract int Version { get; }

        [Resolved(CanBeNull = true)]
        private IImplementLLin llin { get; set; }

        [CanBeNull]
        [Obsolete("Mvis => LLin")]
        protected IImplementLLin Mvis => llin;

        [CanBeNull]
        protected IImplementLLin LLin => llin;

        #region 异步加载任务相关

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region 杂项

        protected bool ContentLoaded;

        public BindableBool Disabled = new BindableBool
        {
            Default = true,
            Value = true
        };

        protected DependencyContainer DependenciesContainer;

        protected LLinPluginManager PluginManager;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            DependenciesContainer = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            var pluginManager = DependenciesContainer.Get<LLinPluginManager>();
            PluginManager = pluginManager;

            var config = pluginManager.GetConfigManager(this);

            if (config != null)
                DependenciesContainer.Cache(config);

            DependenciesContainer.Cache(this);
        }

        #endregion

        private void createLoadTask()
        {
            ContentLoaded = false;

            try
            {
                //加载内容
                LoadComponentAsync(CreateContent(), content =>
                {
                    ContentLoaded = true;

                    //添加内容
                    Add(content);

                    //调用OnContentLoaded进行善后
                    OnContentLoaded(content);

                    llin?.UnmarkFromLoading(this);
                }, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{Name}在加载内容时出现了问题");
            }
        }

        /// <summary>
        /// 取消加载任务
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            llin?.UnmarkFromLoading(this);
        }

        /// <summary>
        /// 加载插件内容
        /// </summary>
        public virtual void Load()
        {
            try
            {
                //向加载列表添加这个plugin
                llin?.MarkAsLoading(this);

                //调用PostInit在加载内容前初始化
                if (!PostInit())
                {
                    llin?.UnmarkFromLoading(this);
                    return;
                }

                createLoadTask();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"{Name}在加载时出现了问题");
                llin?.UnmarkFromLoading(this);
            }
        }

        /// <summary>
        /// 启用插件
        /// </summary>
        /// <returns>
        /// true: 启用过程未发生意外<br/>
        /// false: 启用过程发生意外</returns>
        public virtual bool Enable()
        {
            if (!ContentLoaded)
                Load();

            Disabled.Value = false;

            return true;
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        public virtual void UnLoad()
        {
            Disable();
            Expire();
        }

        /// <summary>
        /// 禁用插件
        /// </summary>
        /// <returns>
        /// true: 禁用过程未发生意外<br/>
        /// false: 禁用过程发生意外</returns>
        public virtual bool Disable()
        {
            if (!ContentLoaded)
                Cancel();

            Disabled.Value = true;
            return true;
        }

        /// <summary>
        /// 插件支持的属性<br/>
        /// CanDisable - 插件支持禁用<br/>
        /// CanUnload - 插件支持卸载<br/>
        /// CanReload - 插件支持重新加载(未实现)
        /// </summary>
        public enum PluginFlags
        {
            CanDisable,
            CanUnload,
            CanReload
        }

        /// <summary>
        /// 目标位置<br/>
        /// Background - 背景<br/>
        /// Foreground - 前景
        /// </summary>
        public enum TargetLayer
        {
            Background,
            Foreground,
            FunctionBar
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
                Cancel();
        }
    }
}
