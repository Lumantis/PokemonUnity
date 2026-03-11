using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PokemonUnity.Interface;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// On-screen virtual gamepad for mobile/touchscreen.
    /// Shows automatically on mobile platforms (configurable).
    /// Provides a static state dictionary read by <see cref="NewInputController"/>.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VirtualGamepad : MonoBehaviour
    {
        // ── Static virtual state (read by NewInputController) ─────────────────
        private static readonly Dictionary<InputKeys, bool> _state =
            new Dictionary<InputKeys, bool>();

        /// <summary>Read the current virtual button state (called from NewInputController).</summary>
        public static bool GetVirtualKey(InputKeys key)
            => _state.TryGetValue(key, out bool v) && v;

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Visibility")]
        [SerializeField] private bool showOnMobile  = true;
        [SerializeField] private bool showOnDesktop = false;

        [Header("D-Pad")]
        [SerializeField] private Button btnUp;
        [SerializeField] private Button btnDown;
        [SerializeField] private Button btnLeft;
        [SerializeField] private Button btnRight;

        [Header("Action Buttons")]
        [SerializeField] private Button btnA;
        [SerializeField] private Button btnB;
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnSelect;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            bool isMobile = Application.isMobilePlatform;
            bool shouldShow = (isMobile && showOnMobile) || (!isMobile && showOnDesktop);

            var cg = GetComponent<CanvasGroup>();
            cg.alpha          = shouldShow ? 1f : 0f;
            cg.blocksRaycasts = shouldShow;
            cg.interactable   = shouldShow;
        }

        private void Start()
        {
            WireButton(btnUp,     InputKeys.Up);
            WireButton(btnDown,   InputKeys.Down);
            WireButton(btnLeft,   InputKeys.Left);
            WireButton(btnRight,  InputKeys.Right);
            WireButton(btnA,      InputKeys.Action);
            WireButton(btnB,      InputKeys.Back);
            WireButton(btnStart,  InputKeys.Start);
            WireButton(btnSelect, InputKeys.Select);
        }

        private void OnDisable()
        {
            // Release all virtual keys when the gamepad is hidden
            foreach (var key in _state.Keys)
                _state[key] = false;
        }

        // ── Wiring ────────────────────────────────────────────────────────────
        /// <summary>
        /// Wires pointer-down and pointer-up events to set the virtual key state.
        /// Uses <see cref="EventTrigger"/> for reliable multi-touch support.
        /// </summary>
        private void WireButton(Button btn, InputKeys key)
        {
            if (btn == null) return;

            _state[key] = false;

            var trigger = btn.gameObject.GetComponent<EventTrigger>()
                       ?? btn.gameObject.AddComponent<EventTrigger>();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => _state[key] = true);
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => _state[key] = false);
            trigger.triggers.Add(up);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => _state[key] = false);
            trigger.triggers.Add(exit);
        }
    }
}
