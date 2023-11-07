using System;
using System.Collections.Generic;
using System.Linq;
using TrickModule.Core;
using UnityEngine;
using UnityEngine.Events;

namespace TrickModule.Game
{
    public class UIManager : MonoSingleton<UIManager>
    {
        [field: SerializeField] public MenuDebugMode MenuDebug { get; } = MenuDebugMode.EditorOnly;

        [SerializeField, Header("Menu")] private List<MenuLoadStrategy> menuLoadStrategies = new List<MenuLoadStrategy>()
        {
            new MenuLoadStrategy()
            {
                MenuLoadMode = MenuLoadMode.List,
            }
        };

        private readonly Dictionary<string, UIMenu> _instantiatedMenus = new();
        private readonly List<UIMenu> _allMenus = new();

        public UnityEvent<UIMenu> PreMenuShowEvent { get; } = new UnityEvent<UIMenu>();
        public UnityEvent<UIMenu> PreMenuHideEvent { get; } = new UnityEvent<UIMenu>();
        public UnityEvent<UIMenu> PostMenuShowEvent { get; } = new UnityEvent<UIMenu>();
        public UnityEvent<UIMenu> PostMenuHideEvent { get; } = new UnityEvent<UIMenu>();

        protected override void Initialize()
        {
            base.Initialize();

            LoadMenus();
        }

        private void LoadMenus()
        {
            // Depending on the load strategy, load the menus
            foreach (MenuLoadStrategy strategy in menuLoadStrategies)
            {
                var menus = new List<UIMenu>();
                switch (strategy.MenuLoadMode)
                {
                    case MenuLoadMode.List:
                        foreach (UIMenu menu in strategy.MenuList) menus.Add(menu);
                        break;
                    case MenuLoadMode.Resources:
                        foreach (string menuPath in strategy.MenuResourcesPathList) menus.AddRange(Resources.LoadAll<UIMenu>(menuPath).Where(menu => menu != null));
                        break;
                    case MenuLoadMode.Addressable:
                        // TODO: Implement addressable loading
                        break;
                    case MenuLoadMode.Manual:
                        // Do nothing
                        break;
                }

                if (strategy.InstantiateOnLoad)
                {
                    foreach (UIMenu menu in menus)
                    {
                        InternalInstantiateMenu(menu);
                    }
                }

                _allMenus.AddRange(menus);
            
                // Ensure that all menus are unique by type
                var duplicateMenus = _allMenus.GroupBy(menu => menu.GetType().Name).Where(group => group.Count() > 1).Select(group => group.Skip(1)).SelectMany(enumerable => enumerable.ToList()).ToList();
                if (duplicateMenus.Count > 0)
                {
                    Logger.Logger.Game.LogError($"[UIManager-LoadMenus] Duplicate menus found: {string.Join(", ", duplicateMenus)}");
                
                    // Remove the duplicates, but keep the first one
                    foreach (var duplicateMenu in duplicateMenus)
                    {
                        _allMenus.Remove(duplicateMenu);
                    
                        Logger.Logger.Game.LogError($"[UIManager-LoadMenus] Removed duplicate menu: {duplicateMenu}");
                    }
                }
            }
        }

        public void LoadMenuManually(UIMenu menu, bool instantiate = true)
        {
            if (menu == null)
            {
                Logger.Logger.Game.LogError("[UIManager-LoadMenuManually] Menu is null!");
                return;
            }

            if (_instantiatedMenus.ContainsKey(menu.GetType().Name))
            {
                Logger.Logger.Game.LogError($"[UIManager-LoadMenuManually] Menu of type {menu.GetType().Name} already exists!");
                return;
            }

            _instantiatedMenus.Add(menu.GetType().Name, instantiate ? Instantiate(menu) : menu);
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void ApplicationQuit()
        {
            base.ApplicationQuit();
        }

        public void TryShow<T>() where T : UIMenu
        {
            var menu = GetMenuByType(typeof(T));
            if (menu != null)
            {
                menu.TryShowMenu();
            }
            else
            {
                Logger.Logger.Game.LogError($"[UIManager-TryShow] Menu of type {typeof(T)} not found!");            
            }
        }

        public void TryHide<T>() where T : UIMenu
        {
            var menu = GetMenuByType(typeof(T));
            if (menu != null)
            {
                menu.TryHideMenu();
            }
            else
            {
                Logger.Logger.Game.LogError($"[UIManager-TryHide] Menu of type {typeof(T)} not found!");            
            }
        }

        public void ShowOnly<T>() where T : UIMenu
        {
            var menu = GetMenuByType(typeof(T));
            if (menu == null) return;
            foreach (var instantiated in _instantiatedMenus)
            {
                if (instantiated.Value == menu)
                    instantiated.Value.TryShowMenu();
                else
                    instantiated.Value.TryHideMenu();
            }
        }

        public UIMenu GetMenuByName(string menuName)
        {
            var type = Type.GetType(menuName, false);
            return type == null ? null : GetMenuByType(type);
        }

        /// <summary>
        /// Gets a menu by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public UIMenu GetMenuByType(Type type)
        {
            if (type == null) return null;
            if (_instantiatedMenus.TryGetValue(type.Name, out var instantiatedMenu)) return instantiatedMenu;
            var menuToInstantiate = _allMenus.Find(menu => menu != null && menu.GetType() == type);
            if (menuToInstantiate == null)
            {
                Debug.LogError($"The menu you are trying to instantiate is null (name={type.Name})");
                return null;
            }

            return InternalInstantiateMenu(menuToInstantiate);
        }

        private UIMenu InternalInstantiateMenu(UIMenu menu)
        {
            var newMenu = Instantiate(menu, transform);
            if (newMenu != null)
            {
                newMenu.InternalSetManager(this);

                _instantiatedMenus[newMenu.GetType().Name] = newMenu;
            
                newMenu.gameObject.SetActive(false);
                newMenu.InternalInit();
                newMenu.InternalStart();
            }

            return newMenu;
        }
    }
}