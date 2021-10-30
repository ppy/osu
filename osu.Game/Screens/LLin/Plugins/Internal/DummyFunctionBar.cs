using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Screens.LLin.Plugins.Types;

namespace osu.Game.Screens.LLin.Plugins.Internal
{
    public class DummyFunctionBar : LLinPlugin, IFunctionBarProvider
    {
        public DummyFunctionBar()
        {
            Name = "无";
        }

        protected override Drawable CreateContent()
        {
            throw new NotImplementedException();
        }

        protected override bool OnContentLoaded(Drawable content)
        {
            throw new NotImplementedException();
        }

        protected override bool PostInit()
        {
            throw new NotImplementedException();
        }

        public override int Version => 6;

        public float GetSafeAreaPadding()
        {
            throw new NotImplementedException();
        }

        public bool OkForHide()
        {
            throw new NotImplementedException();
        }

        public bool AddFunctionControl(IFunctionProvider provider)
        {
            throw new NotImplementedException();
        }

        public bool AddFunctionControls(List<IFunctionProvider> providers)
        {
            throw new NotImplementedException();
        }

        public bool SetFunctionControls(List<IFunctionProvider> providers)
        {
            throw new NotImplementedException();
        }

        public void Remove(IFunctionProvider provider)
        {
            throw new NotImplementedException();
        }

        public void ShowFunctionControlTemporary()
        {
            throw new NotImplementedException();
        }

        public List<IPluginFunctionProvider> GetAllPluginFunctionButton()
        {
            throw new NotImplementedException();
        }

        public Action OnDisable { get; set; }
    }
}
