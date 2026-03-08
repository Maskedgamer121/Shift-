using Cavrnus.SpatialConnector.PlayerControllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Cavrnus.SpatialConnector.UI
{
	[RequireComponent(typeof(TMP_InputField))]
    public class CavrnusInputFieldHelper : MonoBehaviour
    {
        public UnityEvent<string> OnEndEdit;
        
        private TMP_InputField inputField;
        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            inputField.onSubmit.AddListener(Submit);
        }

        private void Update()
        {
            if (inputField.isFocused)
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				if (SimpleCavrnusInput.IsKeyDown(Key.Enter))
#elif ENABLE_LEGACY_INPUT_MANAGER
	            if (SimpleCavrnusInput.IsKeyDown(KeyCode.Return))
#else
				if (false)
#endif
                {
					Submit(inputField.text);
                }
            }
        }

        private void Submit(string val)
        {
            OnEndEdit?.Invoke(val);
        }

        private void OnDestroy()
        {
            inputField.onEndEdit.RemoveListener(Submit);
        }
    }
}