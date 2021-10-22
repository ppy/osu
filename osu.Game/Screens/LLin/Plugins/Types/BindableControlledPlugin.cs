using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Screens.LLin.Plugins.Types
{
    public abstract class BindableControlledPlugin : LLinPlugin
    {
        [Resolved]
        private LLinPluginManager manager { get; set; }

        protected BindableBool Value = new BindableBool();
        private bool playerExiting;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (LLin != null)
                LLin.Exiting += () => playerExiting = true;
        }

        protected override void LoadComplete()
        {
            Value.BindValueChanged(OnValueChanged, true);
            base.LoadComplete();
        }

        protected virtual void OnValueChanged(ValueChangedEvent<bool> v)
        {
            if (Value.Value && !playerExiting)
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
