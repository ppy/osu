// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class DrawableOsuEditorRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuEditorPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private partial class OsuEditorPlayfield : OsuPlayfield
        {
            [Resolved]
            private EditorBeatmap editorBeatmap { get; set; } = null!;

            protected override GameplayCursorContainer? CreateCursor() => null;

            public OsuEditorPlayfield()
            {
                HitPolicy = new AnyOrderHitPolicy();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                editorBeatmap.BeatmapReprocessed += onBeatmapReprocessed;
            }

            private void onBeatmapReprocessed() => ApplyCircleSizeToPlayfieldBorder(editorBeatmap);

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (editorBeatmap.IsNotNull())
                    editorBeatmap.BeatmapReprocessed -= onBeatmapReprocessed;
            }
        }
    }
}
