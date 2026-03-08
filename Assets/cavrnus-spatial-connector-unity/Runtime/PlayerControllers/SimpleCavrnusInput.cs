using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Cavrnus.SpatialConnector.PlayerControllers
{
    public static class SimpleCavrnusInput
    {
        public enum MouseButton
        {
            Left = 0,
            Right = 1
        }

        public static bool IsMouseDown(MouseButton button)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Mouse.current == null) return false;
        return button switch
        {
            MouseButton.Left => Mouse.current.leftButton.wasPressedThisFrame,
            MouseButton.Right => Mouse.current.rightButton.wasPressedThisFrame,
            _ => false
        };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown((int)button);
#else
        return false;
#endif
        }

        public static bool IsMouseUp(MouseButton button)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Mouse.current == null) return false;
        return button switch
        {
            MouseButton.Left => Mouse.current.leftButton.wasReleasedThisFrame,
            MouseButton.Right => Mouse.current.rightButton.wasReleasedThisFrame,
            _ => false
        };
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonUp((int)button);
#else
        return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        public static bool IsKeyDown(Key key)
        {
			if (Keyboard.current == null) 
		        return false;
			return Keyboard.current[key].isPressed;
		}
#elif ENABLE_LEGACY_INPUT_MANAGER
	    public static bool IsKeyDown(KeyCode key)
	    {
		    return Input.GetKeyDown(key);
	    }
#else
 	    public static bool IsKeyDown(KeyCode key)
	    {
	        return false;
		}
#endif
	}
}