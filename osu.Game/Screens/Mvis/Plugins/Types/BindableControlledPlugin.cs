using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    public abstract class BindableControlledPlugin : MvisPlugin
    {
        [Resolved]
        private MvisPluginManager manager { get; set; }

        protected BindableBool Value = new BindableBool();

        protected override void LoadComplete()
        {
            Value.BindValueChanged(OnValueChanged, true);

            base.LoadComplete();
        }

        protected virtual void OnValueChanged(ValueChangedEvent<bool> v)
        {
            if (Value.Value)
                manager.ActivePlugin(this);
            else
                manager.DisablePlugin(this);
        }

        public override bool Disable()
        {
            Value.Value = false;
            return base.Disable();
        }

        public override bool Enable()
        {
            Value.Value = true;
            return base.Enable();
        }

        public override void UnLoad()
        {
            Value.UnbindAll();
            base.UnLoad();
        }
    }
}
