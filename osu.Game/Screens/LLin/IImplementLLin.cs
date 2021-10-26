using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Screens.LLin.Misc;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin
{
    /// <summary>
    /// LLin播放器通用接口
    /// </summary>
    /// <remarks>实现与插件相关的接口时最好验证传入的插件是否合法（如：插件是否在MvisPluginmanager.GetAllPlugins(false)给出的列表中）</remarks>
    public interface IImplementLLin
    {
        #region 音频、音频控制

        /// <summary>
        /// 当前播放的曲目
        /// </summary>
        public DrawableTrack CurrentTrack { get; }

        /// <summary>
        /// 切换暂停时调用。<br/><br/>
        /// 传递: 当前音乐是否暂停<br/>
        /// true: 暂停<br/>
        /// false: 播放<br/>
        /// </summary>
        public Action<bool> OnTrackRunningToggle { get; set; }

        /// <summary>
        /// 播放器屏幕退出时调用
        /// </summary>
        public Action Exiting { get; set; }

        /// <summary>
        /// 播放器屏幕进入后台时调用
        /// </summary>
        public Action Suspending { get; set; }

        /// <summary>
        /// 播放器屏幕进入前台时调用
        /// </summary>
        public Action Resuming { get; set; }

        /// <summary>
        /// 进入空闲状态(长时间没有输入)时调用
        /// </summary>
        public Action OnIdle { get; set; }

        /// <summary>
        /// 从空闲状态退出时调用
        /// </summary>
        public Action OnActive { get; set; }

        /// <summary>
        /// 拖动歌曲进度条时调用<br/><br/>
        /// 传递: 拖动的目标时间
        /// </summary>
        public Action<double> OnSeek { get; set; }

        /// <summary>
        /// 调整歌曲进度
        /// </summary>
        /// <param name="position">目标进度（毫秒）</param>
        public void SeekTo(double position);

        #endregion

        #region 其他API

        /// <summary>
        /// 订阅谱面变更事件
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <param name="sender">订阅方</param>
        /// <param name="runOnce">是否立即执行一次</param>
        public void OnBeatmapChanged(Action<WorkingBeatmap> action, object sender, bool runOnce = false);

        /// <summary>
        /// 注册插件KeyBind
        /// </summary>
        /// <param name="plugin">发起插件</param>
        /// <param name="keybind">目标KeyBind</param>
        public void RegisterPluginKeybind(LLinPlugin plugin, PluginKeybind keybind);

        /// <summary>
        /// 撤销一个插件的KeyBind绑定
        /// </summary>
        /// <param name="plugin">发起插件</param>
        /// <param name="keybind">目标KeyBind, 如果是null则撤销该插件的所有按键绑定</param>
        public void UnRegisterPluginKeybind(LLinPlugin plugin, PluginKeybind keybind = null);

        /// <summary>
        /// 获取当前播放器信息
        /// </summary>
        /// <returns>播放器信息</returns>
        public PlayerInfo GetInfo();

        #endregion

        #region Proxy功能实现

        /// <summary>
        /// 添加一个Proxy
        /// </summary>
        /// <param name="d">要添加的Drawable</param>
        public void AddProxy(Drawable d);

        /// <summary>
        /// 移除一个Proxy
        /// </summary>
        /// <param name="d">要移除的Drawable</param>
        /// <returns>
        /// true: 移除成功<br/>
        /// false: 移除出现异常</returns>
        public bool RemoveProxy(Drawable d);

        #endregion

        /// <summary>
        /// 播放器是否处于界面隐藏状态
        /// </summary>
        public bool InterfacesHidden { get; }

        #region 音频插件控制

        /// <summary>
        /// 请求接手音频控制
        /// </summary>
        /// <param name="pacp">请求的插件</param>
        /// <param name="message">要显示的消息</param>
        /// <param name="onDeny">请求拒绝时的动作</param>
        /// <param name="onAllow">请求接受时的动作</param>
        public void RequestAudioControl(IProvideAudioControlPlugin pacp, LocalisableString message, Action onDeny, Action onAllow);

        /// <summary>
        /// 释放音频控制插件
        /// </summary>
        /// <param name="pacp">要释放的插件</param>
        public void ReleaseAudioControlFrom(IProvideAudioControlPlugin pacp);

        #endregion

        #region 插件加载标记

        /// <summary>
        /// 标记一个<see cref="LLinPlugin"/>的状态为加载中
        /// </summary>
        /// <param name="pl">目标插件</param>
        /// <returns>
        /// true: 成功标记<br/>
        /// false: 该插件已经标记为加载中了或插件不在MvisPluginManager给出的列表中</returns>
        public bool MarkAsLoading(LLinPlugin pl);

        /// <summary>
        /// 取消一个<see cref="LLinPlugin"/>的加载标记
        /// </summary>
        /// <param name="pl">目标插件</param>
        /// <returns>
        /// true: 成功取消<br/>
        /// false: 该插件没有加载标记或插件不在MvisPluginManager给出的列表中</returns>
        public bool UnmarkFromLoading(LLinPlugin pl);

        #endregion

        /// <summary>
        /// 底栏高度，可用于避免插件内容与当前底栏插件冲突
        /// </summary>
        public float BottomBarHeight { get; }

        /// <summary>
        /// 当前谱面
        /// </summary>
        public Bindable<WorkingBeatmap> Beatmap { get; }

        #region 播放器背景控制

        /// <summary>
        /// 请求背景黑屏
        /// </summary>
        /// <param name="sender">请求发起方</param>
        /// <returns>请求是否被接受</returns>
        public bool RequestBlackBackground(LLinPlugin sender);

        /// <summary>
        /// 请求去除背景黑屏
        /// </summary>
        /// <param name="sender">请求发起方</param>
        /// <returns>请求是否被接受</returns>
        public bool RequestNonBlackBackground(LLinPlugin sender);

        /// <summary>
        /// 请求去除背景动画
        /// </summary>
        /// <param name="sender">请求发起方</param>
        /// <returns>请求是否被接受</returns>
        public bool RequestCleanBackground(LLinPlugin sender);

        /// <summary>
        /// 请求恢复背景动画
        /// </summary>
        /// <param name="sender">请求发起方</param>
        /// <returns>请求是否被接受</returns>
        public bool RequestNonCleanBackground(LLinPlugin sender);

        #endregion
    }
}
