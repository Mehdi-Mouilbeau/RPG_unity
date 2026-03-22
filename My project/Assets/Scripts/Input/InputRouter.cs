using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRouter : MonoBehaviour
{
    public static InputRouter Instance { get; private set; }

    private readonly Dictionary<int, InputDevice> _devices = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        AssignDefaults();
    }

    private void AssignDefaults()
    {
        // P1 = keyboard (always present)
        if (Keyboard.current != null)
            _devices[0] = Keyboard.current;

        // P2 = first connected gamepad; fallback to keyboard for hot-seat
        if (Gamepad.all.Count > 0)
            _devices[1] = Gamepad.all[0];
        else if (Keyboard.current != null)
            _devices[1] = Keyboard.current;
    }

    /// <summary>Returns the input device assigned to the given player (null if not found).</summary>
    public InputDevice GetDevice(int playerIndex) =>
        _devices.TryGetValue(playerIndex, out var d) ? d : null;

    /// <summary>True if both players share the same device (pure hot-seat keyboard).</summary>
    public bool IsSharedKeyboard =>
        _devices.TryGetValue(0, out var d0) &&
        _devices.TryGetValue(1, out var d1) &&
        d0 == d1;
}
