﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Platform;

namespace osu.Framework.Graphics.Containers
{
    /// <summary>
    /// A <see cref="SnapTargetContainer"/> that is automatically cached and provides a bindable <see cref="SafeAreaPadding"/> representing
    /// the desired safe area margins. Should be used in conjunction with child <see cref="SafeAreaContainer"/>s.
    /// The root of the scenegraph contains an instance of this container, with <see cref="SafeAreaPadding"/> automatically bound
    /// to the host <see cref="GameWindow"/>'s <see cref="GameWindow.SafeAreaPadding"/>.
    /// </summary>
    [Cached(typeof(SafeAreaTargetContainer))]
    public class SafeAreaTargetContainer : SnapTargetContainer
    {
        private readonly bool usesCustomBinding;

        internal BindableSafeArea SafeAreaPadding { get; } = new BindableSafeArea();

        /// <summary>
        /// Initialises a <see cref="SafeAreaTargetContainer"/> by optionally providing a custom <see cref="BindableSafeArea"/>.
        /// If no such binding is provided, the container will default to <see cref="GameWindow.SafeAreaPadding"/>.
        /// </summary>
        /// <param name="bindableSafeArea">The custom <see cref="BindableSafeArea"/> to bind to, if required.</param>
        public SafeAreaTargetContainer(BindableSafeArea bindableSafeArea = null)
        {
            usesCustomBinding = bindableSafeArea != null;

            if (bindableSafeArea != null)
                SafeAreaPadding.BindTo(bindableSafeArea);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            if (!usesCustomBinding && host.Window != null)
                SafeAreaPadding.BindTo(host.Window.SafeAreaPadding);
        }
    }
}
