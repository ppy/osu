// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFirstPerson : Mod, IReadFromConfig, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToPlayer
    {
        public override string Name => "First Person";
        public override string Acronym => "FP";
        public override LocalisableString Description => "Catch, from the catcher's perspective!";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => OsuIcon.ModMovingFast;
        public override Type[] IncompatibleMods => new[] { typeof(ModCinema), typeof(ModRelax), typeof(ModNoScope) };

        [SettingSource("Centred background", "Have the background follow.")] // From the perspective of the player w.r.t. catcher. Less ambiguous than e.g. "(un)adjusted"
        public BindableBool CentredBackground { get; } = new BindableBool();

        [SettingSource("Centred storyboard / video", "Have the storyboard / video follow.")] // From the perspective of the player w.r.t. catcher. Less ambiguous than e.g. "(un)adjusted"
        public BindableBool CentredStoryboard { get; } = new BindableBool();

        private readonly Bindable<bool> showStoryboard = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ShowStoryboard, showStoryboard);
        }

        private CatchPlayfield catchPlayfield = null!;

        private float miscellaneousXOffset => shift_x_factor * catchPlayfield.X;
        private const float shift_x_factor = 1.6f; // Brute-forced, is magical, todo: may need more intricate calculation

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            catchPlayfield = (CatchPlayfield)drawableRuleset.Playfield;

            catchPlayfield.OnUpdate += _ => catchPlayfield.MoveToX(CatchPlayfield.CENTER_X - catchPlayfield.Catcher.X);
        }

        public void ApplyToPlayer(Player player)
        {
            Storyboard storyboard = player.GameplayState.Storyboard;

            player.ApplyToBackground(bsb => bsb.OnUpdate += _ =>
            {
                // TODO: Make background return to default X when leaving gameplay

                bool storyboardReplacesBackground = storyboard.ReplacesBackground && storyboard.HasDrawable; // Based on Player's
                if (!storyboardReplacesBackground)
                    updateBackgroundX(bsb);
            });

            if (!storyboard.HasDrawable)
                return;

            Drawable? drawableStoryboard = getDrawableStoryboard(player);

            if (drawableStoryboard.IsNull())
            {
                showStoryboard.BindValueChanged(ss => Task.Run(async () => // Task.Run to not have 'async' lambda with delegate returning 'void'
                {
                    if (!ss.NewValue)
                        return;

                    showStoryboard.UnbindBindings(); // Show storyboard being enabled even briefly during gameplay means the drawable storyboard will load

                    drawableStoryboard = getDrawableStoryboard(player);

                    while (drawableStoryboard.IsNull())
                    {
                        await Task.Delay(200).ConfigureAwait(false);
                        drawableStoryboard = getDrawableStoryboard(player);

                        if (!player.IsAlive || !player.IsCurrentScreen())
                            return;
                    }

                    drawableStoryboard.OnUpdate += _ => updateStoryboardX(drawableStoryboard);
                }), true);

                return;
            }

            drawableStoryboard.OnUpdate += _ => updateStoryboardX(drawableStoryboard);
        }

        private Drawable? getDrawableStoryboard(Player player) => player.DimmableStoryboard.Children.FirstOrDefault(c => c is DrawableStoryboard);

        private void updateBackgroundX(BackgroundScreenBeatmap backgroundScreenBeatmap)
        {
            if (!CentredBackground.Value)
                backgroundScreenBeatmap.MoveToX(miscellaneousXOffset);
        }

        private void updateStoryboardX(Drawable drawableStoryboard)
        {
            if (!CentredStoryboard.Value)
                drawableStoryboard.MoveToX(miscellaneousXOffset);
        }
    }
}
