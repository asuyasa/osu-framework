// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Framework.Platform.Sdl;
using osuTK;

namespace osu.Framework.Platform
{
    public class DesktopWindow : Window
    {
        private readonly BindableSize sizeFullscreen = new BindableSize();
        private readonly BindableSize sizeWindowed = new BindableSize();
        private readonly BindableDouble windowPositionX = new BindableDouble();
        private readonly BindableDouble windowPositionY = new BindableDouble();
        private readonly Bindable<DisplayIndex> windowDisplayIndex = new Bindable<DisplayIndex>();

        public readonly Bindable<ConfineMouseMode> ConfineMouseMode = new Bindable<ConfineMouseMode>();

        protected Vector2 RelativePosition
        {
            get
            {
                var displayBounds = CurrentDisplay.Value.Bounds;
                var windowX = Position.Value.X - displayBounds.X;
                var windowY = Position.Value.Y - displayBounds.Y;
                var windowSize = sizeWindowed.Value;

                return new Vector2(
                    displayBounds.Width > windowSize.Width ? (float)windowX / (displayBounds.Width - windowSize.Width) : 0,
                    displayBounds.Height > windowSize.Height ? (float)windowY / (displayBounds.Height - windowSize.Height) : 0);
            }
            set
            {
                if (WindowMode.Value != Configuration.WindowMode.Windowed)
                    return;

                var displayBounds = CurrentDisplay.Value.Bounds;
                var windowSize = sizeWindowed.Value;
                var windowX = (int)Math.Round((displayBounds.Width - windowSize.Width) * value.X);
                var windowY = (int)Math.Round((displayBounds.Height - windowSize.Height) * value.Y);

                Position.Value = new Point(windowX + displayBounds.X, windowY + displayBounds.Y);
            }
        }

        /// <summary>
        /// Initialises a window for desktop platforms.
        /// Uses <see cref="Sdl2WindowBackend"/> and <see cref="PassthroughGraphicsBackend"/>.
        /// </summary>
        public DesktopWindow()
            : base(new Sdl2WindowBackend(), new PassthroughGraphicsBackend())
        {
        }

        public override void SetupWindow(FrameworkConfigManager config)
        {
            base.SetupWindow(config);

            CurrentDisplay.ValueChanged += evt =>
            {
                windowDisplayIndex.Value = (DisplayIndex)evt.NewValue.Index;
                windowPositionX.Value = 0.5;
                windowPositionY.Value = 0.5;
            };

            config.BindWith(FrameworkSetting.LastDisplayDevice, windowDisplayIndex);
            windowDisplayIndex.BindValueChanged(evt => CurrentDisplay.Value = Displays.ElementAtOrDefault((int)evt.NewValue) ?? PrimaryDisplay, true);

            sizeFullscreen.ValueChanged += evt =>
            {
                if (evt.NewValue.IsEmpty)
                    return;

                WindowBackend.FullscreenSize = evt.NewValue;
            };

            sizeWindowed.ValueChanged += evt =>
            {
                if (evt.NewValue.IsEmpty)
                    return;

                WindowBackend.WindowedSize = evt.NewValue;
                Size.Value = evt.NewValue;

                Console.WriteLine($"sizeWindowed.ValueChanged: Size = {Size.Value}");
            };

            config.BindWith(FrameworkSetting.SizeFullscreen, sizeFullscreen);
            config.BindWith(FrameworkSetting.WindowedSize, sizeWindowed);

            config.BindWith(FrameworkSetting.WindowedPositionX, windowPositionX);
            config.BindWith(FrameworkSetting.WindowedPositionY, windowPositionY);

            RelativePosition = new Vector2((float)windowPositionX.Value, (float)windowPositionY.Value);

            config.BindWith(FrameworkSetting.WindowMode, WindowMode);
            WindowMode.BindValueChanged(evt => UpdateWindowMode(evt.NewValue), true);

            // config.BindWith(FrameworkSetting.ConfineMouseMode, ConfineMouseMode);
            // ConfineMouseMode.BindValueChanged(confineMouseModeChanged, true);

            Resized += onResized;
            Moved += onMoved;
        }

        private void onResized()
        {
            if (WindowState.Value == Platform.WindowState.Normal)
            {
                sizeWindowed.Value = WindowBackend.WindowedSize;
                Size.Value = sizeWindowed.Value;
                updateWindowPositionConfig();
            }
        }

        private void onMoved(Point point)
        {
            if (WindowState.Value == Platform.WindowState.Normal)
                updateWindowPositionConfig();
        }

        private void updateWindowPositionConfig()
        {
            if (WindowState.Value == Platform.WindowState.Normal)
            {
                var relativePosition = RelativePosition;
                windowPositionX.Value = relativePosition.X;
                windowPositionY.Value = relativePosition.Y;
            }
        }

        private void confineMouseModeChanged(ValueChangedEvent<ConfineMouseMode> args)
        {
            bool confine = false;

            switch (args.NewValue)
            {
                case Input.ConfineMouseMode.Fullscreen:
                    confine = WindowMode.Value != Configuration.WindowMode.Windowed;
                    break;

                case Input.ConfineMouseMode.Always:
                    confine = true;
                    break;
            }

            if (confine)
                CursorState.Value |= Platform.CursorState.Confined;
            else
                CursorState.Value &= ~Platform.CursorState.Confined;
        }
    }
}
