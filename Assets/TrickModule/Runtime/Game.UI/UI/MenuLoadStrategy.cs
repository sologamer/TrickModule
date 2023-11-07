using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrickModule.Game
{
    [Serializable]
    public class MenuLoadStrategy
    {
        [SerializeField] private MenuLoadMode menuLoadMode;
        [SerializeField] private bool instantiateOnLoad = false;
        [SerializeField] private List<UIMenu> menuList = new List<UIMenu>();
        [SerializeField] private List<string> menuResourcesPathList = new List<string>();
        [SerializeField] private List<string> menuAddressableLabelList = new List<string>();

        public MenuLoadMode MenuLoadMode
        {
            get => menuLoadMode;
            set => menuLoadMode = value;
        }

        public bool InstantiateOnLoad => instantiateOnLoad;
        public List<UIMenu> MenuList => menuList;
        public List<string> MenuResourcesPathList => menuResourcesPathList;

        public List<string> MenuAddressableLabelList => menuAddressableLabelList;
    }
}