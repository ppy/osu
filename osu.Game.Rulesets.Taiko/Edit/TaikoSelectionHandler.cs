// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public class TaikoSelectionHandler : SelectionHandler
    {
        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
        {
            if (selection.All(s => s.HitObject is Hit))
            {
                var hits = selection.Select(s => s.HitObject).OfType<Hit>();

                yield return new TernaryStateMenuItem("Rim", action: state =>
                {
                    ChangeHandler.BeginChange();

                    foreach (var h in hits)
                    {
                        switch (state)
                        {
                            case TernaryState.True:
                                h.Type = HitType.Rim;
                                break;

                            case TernaryState.False:
                                h.Type = HitType.Centre;
                                break;
                        }
                    }

                    ChangeHandler.EndChange();
                })
                {
                    State = { Value = getTernaryState(hits, h => h.Type == HitType.Rim) }
                };
            }

            if (selection.All(s => s.HitObject is TaikoHitObject))
            {
                var hits = selection.Select(s => s.HitObject).OfType<TaikoHitObject>();

                yield return new TernaryStateMenuItem("Strong", action: state =>
                {
                    ChangeHandler.BeginChange();

                    foreach (var h in hits)
                    {
                        switch (state)
                        {
                            case TernaryState.True:
                                h.IsStrong = true;
                                break;

                            case TernaryState.False:
                                h.IsStrong = false;
                                break;
                        }

                        EditorBeatmap?.UpdateHitObject(h);
                    }

                    ChangeHandler.EndChange();
                })
                {
                    State = { Value = getTernaryState(hits, h => h.IsStrong) }
                };
            }
        }

        private TernaryState getTernaryState<T>(IEnumerable<T> selection, Func<T, bool> func)
        {
            if (selection.Any(func))
                return selection.All(func) ? TernaryState.True : TernaryState.Indeterminate;

            return TernaryState.False;
        }
    }
}
