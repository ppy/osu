using eden.Game.GamePieces;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.UI;
using Symcol.Rulesets.Core;

namespace osu.Game.Rulesets.Vitaru
{
    public class VitaruInputManager : SymcolInputManager<VitaruAction>
    {
        private Bindable<bool> debugUI = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.DebugOverlay);
        private Bindable<bool> comboFire = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.ComboFire);
        private Bindable<GraphicsPresets> graphics = VitaruSettings.VitaruConfigManager.GetBindable<GraphicsPresets>(VitaruSetting.GraphicsPresets);

        protected override bool VectorVideo => VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.VectorVideos);

        public static Box Shade;

        public VitaruInputManager(RulesetInfo ruleset, int variant) : base(ruleset, variant, SimultaneousBindingMode.Unique)
        {
            if (graphics.Value == GraphicsPresets.Standard)
                Add(Shade = new Box { RelativeSizeAxes = Framework.Graphics.Axes.Both, Alpha = 0, Colour = OpenTK.Graphics.Color4.Orange});
            if (debugUI)
                Add(new DebugValueUI { Anchor = Framework.Graphics.Anchor.CentreLeft, Origin = Framework.Graphics.Anchor.CentreLeft, Position = new OpenTK.Vector2(10, 0)});
            if (comboFire)
                Add(new ComboFire());
        }
    }

    public enum VitaruAction
    {
        None = -1,

        //Movement
        Left = 0,
        Right,
        Up,
        Down,

        //Self-explaitory
        Shoot,
        Spell,

        //Slows the player + reveals hitbox
        Slow,
        Fast,

        //Sakuya
        Increase,
        Decrease,

        //Kokoro
        RightShoot,
        LeftShoot,

        //Nue
        Spell2,
        Spell3,
        Spell4
    }
}
