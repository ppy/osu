using System;
using System.Collections.Generic;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    public interface IFunctionBarProvider : IDrawable
    {
        /// <summary>
        /// 用于确定附加层(Overlay)安全区的底部Padding
        /// </summary>
        /// <returns>底部Padding</returns>
        public float GetSafeAreaPadding();

        /// <summary>
        /// 是否允许播放器隐藏界面
        /// </summary>
        /// <returns>
        /// true: 可以隐藏
        /// false: 不能隐藏
        /// </returns>
        public bool OkForHide();

        /// <summary>
        /// 设置功能控制
        /// </summary>
        /// <param name="provider"></param>
        /// <returns>
        /// true: 没有问题
        /// false: 出现问题或按钮重复
        /// </returns>
        public bool AddFunctionControl(IFunctionProvider provider);

        /// <summary>
        /// 添加一些控制按钮
        /// </summary>
        /// <param name="providers"></param>
        /// <returns>
        /// true: 没有问题
        /// false: 出现问题或按钮重复
        /// </returns>
        public bool AddFunctionControls(List<IFunctionProvider> providers);

        /// <summary>
        /// 设置一些控制按钮
        /// </summary>
        /// <param name="providers"></param>
        /// <returns>
        /// true: 没有问题
        /// false: 出现问题或按钮重复
        /// </returns>
        public bool SetFunctionControls(List<IFunctionProvider> providers);

        public void Remove(IFunctionProvider provider);

        /// <summary>
        /// 临时显示功能控制按钮
        /// </summary>
        public void ShowFunctionControlTemporary();

        /// <summary>
        /// 获取所有插件按钮
        /// </summary>
        /// <returns>插件按钮列表</returns>
        public List<IPluginFunctionProvider> GetAllPluginFunctionButton();

        public Action OnDisable { get; set; }
    }
}
