﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using OpenTK.Input;

namespace osu.Framework.Input.Handlers.Joystick
{
    public class OpenTKJoystickHandler : InputHandler
    {
        private ScheduledDelegate scheduled;

        private int mostSeenDevices;
        private List<JoystickDevice> devices = new List<JoystickDevice>();

        public override bool Initialize(GameHost host)
        {
            Enabled.ValueChanged += enabled =>
            {
                if (enabled)
                {
                    host.InputThread.Scheduler.Add(scheduled = new ScheduledDelegate(delegate
                    {
                        refreshDevices();

                        foreach (var device in devices)
                        {
                            if (device.State.Equals(device.LastState))
                                continue;

                            if (device.LastState != null)
                            {
                                PendingStates.Enqueue(new InputState { Joystick = new OpenTKJoystickState(device) });
                                FrameStatistics.Increment(StatisticsCounterType.JoystickEvents);
                            }
                        }
                    }, 0, 0));
                }
                else
                {
                    scheduled?.Cancel();
                    devices.Clear();
                }
            };

            Enabled.TriggerChange();

            return true;
        }

        private void refreshDevices()
        {
            var newDevices = new List<JoystickDevice>();

            // Update devices and add them to newDevices if still connected
            foreach (var dev in devices)
            {
                dev.Refresh();

                if (dev.State.IsConnected)
                    newDevices.Add(dev);
            }

            // Find any newly-connected devices
            while (true)
            {
                var newDevice = new JoystickDevice(mostSeenDevices);
                if (!newDevice.Capabilities.IsConnected)
                    break;

                Logger.Log($"Connected joystick device: {newDevice.Guid}", level: LogLevel.Important);

                newDevices.Add(newDevice);
                mostSeenDevices++;
            }

            devices = newDevices;
        }

        public override bool IsActive => true;
        public override int Priority => 0;

        private class OpenTKJoystickState : JoystickState
        {
            private const float dead_zone = 0.1f;

            public OpenTKJoystickState(JoystickDevice device)
            {
                Axes = Enumerable.Range(0, device.Capabilities.AxisCount).Select(i => device.State.GetAxis(i)).ToList();
                Buttons = Enumerable.Range(0, device.Capabilities.ButtonCount).Where(i => device.State.GetButton(i) == ButtonState.Pressed).Select(i => (JoystickButton)i)
                                    .Concat(Axes.Where(a => !Precision.AlmostEquals(a, 0, dead_zone)).Select((axisValue, index) =>
                                    {
                                        if (axisValue < 0)
                                            return JoystickButton.FirstAxisNegativeButton + index;
                                        return JoystickButton.FirstAxisPositiveButton + index;
                                    })).ToList();

            }
        }

        private class JoystickDevice
        {
            /// <summary>
            /// The last state of this <see cref="JoystickDevice"/>.
            /// This is updated with ever invocation of <see cref="Refresh"/>.
            /// </summary>
            public OpenTK.Input.JoystickState? LastState { get; private set; }

            /// <summary>
            /// The current state of this <see cref="JoystickDevice"/>.
            /// Use <see cref="Refresh"/> to update the state.
            /// </summary>
            public OpenTK.Input.JoystickState State { get; private set; }

            /// <summary>
            /// The capabilities for this joystick device.
            /// This is only queried once when <see cref="JoystickDevice"/> is constructed.
            /// </summary>
            public readonly JoystickCapabilities Capabilities;

            /// <summary>
            /// The <see cref="Guid"/> for this <see cref="JoystickDevice"/>.
            /// </summary>
            public readonly Guid Guid;

            private readonly int deviceIndex;

            public JoystickDevice(int deviceIndex)
            {
                this.deviceIndex = deviceIndex;

                Capabilities = OpenTK.Input.Joystick.GetCapabilities(deviceIndex);
                Guid = OpenTK.Input.Joystick.GetGuid(deviceIndex);

                Refresh();
            }

            /// <summary>
            /// Refreshes the state of this <see cref="JoystickDevice"/>.
            /// </summary>
            public void Refresh()
            {
                LastState = State;
                State = OpenTK.Input.Joystick.GetState(deviceIndex);
            }
        }
    }
}
