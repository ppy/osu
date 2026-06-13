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
        public override IconUsage? Icon => OsuIcon.ModMovingFast; // TODO: Mod icon
        public override Type[] IncompatibleMods => new[] { typeof(ModCinema), typeof(ModRelax) };

        [SettingSource("Centred background", "Have the background follow.")] // From the perspective of the player w.r.t. catcher. Less ambiguous than e.g. "(un)adjusted"
        public BindableBool CentredBackground { get; } = new BindableBool();

        [SettingSource("Centred storyboard / video", "Have the storyboard / video follow.")] // From the perspective of the player w.r.t. catcher. Less ambiguous than e.g. "(un)adjusted"
        public BindableBool CentredStoryboard { get; } = new BindableBool();

        private readonly Bindable<bool> showStoryboard = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ShowStoryboard, showStoryboard);
        }

        // Input manager variable not required
        // Drawable ruleset variable not required

        private CatchPlayfield playfield = null!;

        // osu!catch is one dimension in note hitting, X, so position adjustment logic is for float X
        private const float playfield_misc_x_scale = 1.6f; // Brute-forced, is magical, todo: may need more intricate calculation
        private float miscX => playfield.X * playfield_misc_x_scale;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            playfield = (CatchPlayfield)drawableRuleset.Playfield;
        }

        private bool isExited(Player player) => !player.IsCurrentScreen(); // Inspired from TestScenePause's confirmExited

        private Drawable? getDrawableStoryboard(Player player) => player.DimmableStoryboard.Children.FirstOrDefault(d => d is DrawableStoryboard);

        public void ApplyToPlayer(Player player)
        {
            // Playfield position adjusting

            playfield.OnUpdate += _ => playfield.MoveToX(CatchPlayfield.CENTER_X - playfield.Catcher.X);

            // Background and storyboard position adjusting

            Storyboard storyboard = player.GameplayState.Storyboard;

            if (!CentredBackground.Value)
            {
                Action<Drawable> backgroundAction = null!;
                backgroundAction = _ => player.ApplyToBackground(bsb =>
                {
                    if (isExited(player)) // Background screen beatmap persists upon exiting the play, so manual event removal and its repositioning to x = 0 is necessary
                    {
                        bsb.OnUpdate -= backgroundAction;
                        bsb.MoveToX(0.0f);

                        return;
                    }

                    bool storyboardReplacesBackground = storyboard.ReplacesBackground && storyboard.HasDrawable; // Based on Player's
                    if (!storyboardReplacesBackground || !showStoryboard.Value)
                        bsb.MoveToX(miscX);
                });

                player.ApplyToBackground(bsb => bsb.OnUpdate += backgroundAction);
            }

            if (storyboard.HasDrawable && !CentredStoryboard.Value)
            {
                Drawable? drawableStoryboard = getDrawableStoryboard(player); // The drawable storyboard may still be loaded even if Show storyboard was just disabled while entering the play

                if (drawableStoryboard.IsNotNull())
                {
                    drawableStoryboard.OnUpdate += _ => drawableStoryboard.MoveToX(miscX);

                    return;
                }

                showStoryboard.BindValueChanged(ss => Task.Run(async () => // Task.Run to not have 'async' lambda with delegate returning 'void'
                {
                    if (!ss.NewValue)
                        return;

                    showStoryboard.UnbindEvents(); // Show storyboard being enabled even briefly during a play means the drawable storyboard will load into memory if the play continues long enough

                    drawableStoryboard = getDrawableStoryboard(player);

                    while (drawableStoryboard.IsNull())
                    {
                        await Task.Delay(200).ConfigureAwait(false);
                        if (isExited(player))
                            return;

                        drawableStoryboard = getDrawableStoryboard(player);
                    }

                    drawableStoryboard.OnUpdate += _ => drawableStoryboard.MoveToX(miscX);
                }), true);
            }
        }
    }
}
