using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class SideMenuContainer : MonoBehaviour
    {
        public event Action ManuallyClosed;
        
        [SerializeField] private Transform menuContainer;

        [Space]
        [SerializeField] private Button buttonClose;
        [SerializeField] private TextMeshProUGUI menuTitle;

        private CanvasGroup canvasGroup;
        private List<GameObject> childrenMenus = new List<GameObject>();
        
        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            SetMenuContainerVisibility(false);
            buttonClose.onClick.AddListener(OnButtonCloseClicked);
        }

        public void AddMenuToContainer(CavrnusSideMenuData md)
        {
            childrenMenus.Add(md.Menu);
            md.Menu.transform.SetParent(menuContainer);

            var rt = md.Menu.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
    
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            
            md.Menu.SetActive(false);
        }

        public void SetTargetMenuVisibility(int menuId, CavrnusSideMenuData data, bool vis)
        {
            if (vis) {
                childrenMenus.ForEach(m => m.SetActive(false));
                childrenMenus[menuId].SetActive(true);

                menuTitle.text = data.Title;
            }
            else
                childrenMenus[menuId].SetActive(false);
        }

        public void SetMenuContainerVisibility(bool state)
        {
            if (gameObject.activeSelf == state)
                return;
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(state);

            if (state)
                gameObject.DoFade(new List<CanvasGroup> {canvasGroup}, 0.1f, true);
        }
        
        private void OnButtonCloseClicked()
        {
            SetMenuContainerVisibility(false);
            ManuallyClosed?.Invoke();
        }

        private void OnDestroy()
        {
            buttonClose.onClick.RemoveListener(OnButtonCloseClicked);
        }
    }
}