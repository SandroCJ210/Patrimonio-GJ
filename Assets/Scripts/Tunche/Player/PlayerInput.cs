using UnityEngine.InputSystem;

public static class PlayerInput
{
    public static bool DuckPressed
    {
        get
        {
            var kb = Keyboard.current;

            return kb != null &&
                   (kb.sKey.wasPressedThisFrame ||
                    kb.downArrowKey.wasPressedThisFrame);
        }
    }

    public static bool CrossPressed
    {
        get
        {
            var kb = Keyboard.current;

            return kb != null &&
                   (kb.spaceKey.wasPressedThisFrame ||
                    kb.eKey.wasPressedThisFrame);
        }
    }
}