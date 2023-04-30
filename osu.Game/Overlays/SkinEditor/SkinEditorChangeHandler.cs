// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinEditorChangeHandler : EditorChangeHandler
    {
        private readonly ISerialisableDrawableContainer? firstTarget;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindableList<ISerialisableDrawable>? components;

        public SkinEditorChangeHandler(Drawable targetScreen)
        {
            // To keep things simple, we are currently only handling the current target screen for undo / redo.
            // In the future we'll want this to cover all changes, even to skin's `InstantiationInfo`.
            // We'll also need to consider cases where multiple targets are on screen at the same time.

            firstTarget = targetScreen.ChildrenOfType<ISerialisableDrawableContainer>().FirstOrDefault();

            if (firstTarget == null)
                return;

            components = new BindableList<ISerialisableDrawable> { BindTarget = firstTarget.Components };
            components.BindCollectionChanged((_, _) => SaveState(), true);
        }

        protected override void WriteCurrentStateToStream(MemoryStream stream)
        {
            if (firstTarget == null)
                return;

            var skinnableInfos = firstTarget.CreateSerialisedInfo().ToArray();
            string json = JsonConvert.SerializeObject(skinnableInfos, new JsonSerializerSettings { Formatting = Formatting.Indented });
            stream.Write(Encoding.UTF8.GetBytes(json));
        }

        protected override void ApplyStateChange(byte[] previousState, byte[] newState)
        {
            if (firstTarget == null)
                return;

            var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SerialisedDrawableInfo>>(Encoding.UTF8.GetString(newState));

            if (deserializedContent == null)
                return;

            SerialisedDrawableInfo[] skinnableInfos = deserializedContent.ToArray();
            ISerialisableDrawable[] targetComponents = firstTarget.Components.ToArray();

            // Store components based on type for later lookup
            var typedComponents = new Dictionary<Type, Stack<Drawable>>();

            for (int i = targetComponents.Length - 1; i >= 0; i--)
            {
                Drawable component = (Drawable)targetComponents[i];
                Type lookup = component.GetType();

                if (!typedComponents.TryGetValue(lookup, out Stack<Drawable>? typeComponents))
                    typedComponents.Add(lookup, typeComponents = new Stack<Drawable>());

                typeComponents.Push(component);
            }

            // Remove all components
            for (int i = targetComponents.Length - 1; i >= 0; i--)
                firstTarget.Remove(targetComponents[i], false);

            foreach (var skinnableInfo in skinnableInfos)
            {
                Type lookup = skinnableInfo.Type;

                if (!typedComponents.TryGetValue(lookup, out Stack<Drawable>? typeComponents))
                {
                    firstTarget.Add((ISerialisableDrawable)skinnableInfo.CreateInstance());
                    continue;
                }

                if (typeComponents.TryPop(out Drawable? component))
                {
                    // Re-use unused component
                    component.ApplySerialisedInfo(skinnableInfo);
                }
                else
                {
                    // Create new one
                    component = skinnableInfo.CreateInstance();
                }

                firstTarget.Add((ISerialisableDrawable)component);
            }

            foreach ((Type _, Stack<Drawable> typeComponents) in typedComponents)
            {
                // Dispose extra components
                foreach (var component in typeComponents)
                    component.Dispose();
            }
        }
    }
}
