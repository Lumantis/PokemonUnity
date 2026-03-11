using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PokemonUnity.Interface;
using PokemonEssentials.Interface;
using PokemonEssentials.Interface.EventArg;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Input controller built on Unity Input System 1.11+.
    /// Replaces the legacy <c>UserInputController</c> (which used <c>UnityEngine.Input.GetKey</c>).
    /// Supports keyboard AND gamepad simultaneously.
    /// Attach this as a sibling of <see cref="InputManager"/> and wire it in via
    /// <see cref="InputManager.AddController"/>.
    /// </summary>
    public class NewInputController : MonoBehaviour, IInput
    {
        // ── Keyboard mapping ─────────────────────────────────────────────────
        // Maps each InputKeys value to ONE OR MORE keyboard keys (any pressed = active).
        private static readonly Dictionary<InputKeys, Key[]> KeyboardMap =
            new Dictionary<InputKeys, Key[]>
        {
            { InputKeys.Up,     new[] { Key.UpArrow,    Key.W          } },
            { InputKeys.Down,   new[] { Key.DownArrow,  Key.S          } },
            { InputKeys.Left,   new[] { Key.LeftArrow,  Key.A          } },
            { InputKeys.Right,  new[] { Key.RightArrow, Key.D          } },
            { InputKeys.Action, new[] { Key.Z,          Key.Space      } },
            { InputKeys.Back,   new[] { Key.X,          Key.Backspace  } },
            { InputKeys.Start,  new[] { Key.Enter,      Key.NumpadEnter} },
            { InputKeys.Select, new[] { Key.RightShift, Key.LeftShift  } },
            { InputKeys.F5,     new[] { Key.F5                         } },
            { InputKeys.F6,     new[] { Key.F6                         } },
            { InputKeys.F7,     new[] { Key.F7                         } },
            { InputKeys.F8,     new[] { Key.F8                         } },
            { InputKeys.F9,     new[] { Key.F9                         } },
        };

        // ── Gamepad mapping ──────────────────────────────────────────────────
        private static readonly Dictionary<InputKeys, GamepadButton[]> GamepadMap =
            new Dictionary<InputKeys, GamepadButton[]>
        {
            { InputKeys.Up,     new[] { GamepadButton.DpadUp         } },
            { InputKeys.Down,   new[] { GamepadButton.DpadDown       } },
            { InputKeys.Left,   new[] { GamepadButton.DpadLeft       } },
            { InputKeys.Right,  new[] { GamepadButton.DpadRight      } },
            { InputKeys.Action, new[] { GamepadButton.South          } },
            { InputKeys.Back,   new[] { GamepadButton.East           } },
            { InputKeys.Start,  new[] { GamepadButton.Start          } },
            { InputKeys.Select, new[] { GamepadButton.Select         } },
        };

        // ── State ────────────────────────────────────────────────────────────
        private readonly Dictionary<int, bool>  _current  = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool>  _previous = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _holdTime = new Dictionary<int, float>();

        public int PlayerIndex { get; set; }

        public event Action<object, IButtonEventArgs> OnKeyPress;

        // ── Unity lifecycle ──────────────────────────────────────────────────
        private void Update() => update();

        // ── IInput ───────────────────────────────────────────────────────────
        public void update()
        {
            var keyboard = Keyboard.current;
            var gamepad  = Gamepad.current;

            foreach (InputKeys key in Enum.GetValues(typeof(InputKeys)))
            {
                int num      = (int)key;
                bool pressed = false;

                // Keyboard
                if (keyboard != null && KeyboardMap.TryGetValue(key, out Key[] keys))
                    foreach (Key k in keys)
                        pressed |= keyboard[k].isPressed;

                // Gamepad
                if (!pressed && gamepad != null && GamepadMap.TryGetValue(key, out GamepadButton[] btns))
                    foreach (GamepadButton b in btns)
                        pressed |= gamepad[b].isPressed;

                // Virtual gamepad (mobile overlay)
                pressed |= VirtualGamepad.GetVirtualKey(key);

                _previous[num] = _current.ContainsKey(num) ? _current[num] : false;
                _current[num]  = pressed;

                if (!_holdTime.ContainsKey(num)) _holdTime[num] = 0f;
                _holdTime[num] = pressed ? _holdTime[num] + Time.deltaTime : 0f;
            }
        }

        /// <summary>True while the key is held.</summary>
        public bool press(int num)
            => _current.ContainsKey(num) && _current[num];

        /// <summary>True only on the first frame the key is pressed.</summary>
        public bool trigger(int num)
            => _current.GetValueOrDefault(num) && !_previous.GetValueOrDefault(num);

        /// <summary>
        /// True on first press and again every 0.5 s after holding 1 s
        /// (matches the legacy UserInputController repeat behaviour).
        /// </summary>
        public bool repeat(int num)
        {
            if (!press(num)) return false;
            if (trigger(num)) return true;
            float hold = _holdTime.GetValueOrDefault(num);
            return hold > 1.0f && (hold % 0.5f) < Time.deltaTime;
        }

        public void ClearState()
        {
            _current.Clear();
            _previous.Clear();
            _holdTime.Clear();
        }
    }
}
