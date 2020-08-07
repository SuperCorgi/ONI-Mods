using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSerialization;
using UnityEngine;

namespace PipedRustDeoxidizer
{
    public class BuildingNotificationButton : KMonoBehaviour, ISaveLoadable
    {
        private static readonly EventSystem.IntraObjectHandler<BuildingNotificationButton> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<BuildingNotificationButton>(Handler);

        protected override void OnPrefabInit()
        {
            Subscribe(493375141, OnRefreshUserMenuDelegate);
        }

        private static void Handler(BuildingNotificationButton component, object data)
        {
            component.OnRefreshUserMenu(data);
        }

        private void OnMenuToggle()
        {
            Debug.Log($"[MyButton] The button was toggled");
        }

        private void OnRefreshUserMenu(object data)
        {
            KIconButtonMenu.ButtonInfo info = null;
            string iconName = "action_building_disabled";
            string text = "Example";
            System.Action onClick = OnMenuToggle;
            Action shortcutKey = Action.ToggleEnabled;
            string tooltip = "This is an example button";
            info = new KIconButtonMenu.ButtonInfo(iconName, text, onClick, shortcutKey, null, null, null, tooltip);
            Game.Instance.userMenu.AddButton(base.gameObject, info);
        }



        
    }
}
