using System;
using System.Linq;
using UnityEngine.UI;

namespace TrickModule.Game
{
    [Serializable]
    public class MenuAudioService : IMenuService
    {
        public void ExecuteInit(UIMenu menu)
        {
            menu.GetComponentsInChildren<Button>(true).ToList().ForEach(button =>
            {
                if (button != null) button.onClick.AddListener(PlayButtonAudio);
            });
            menu.GetComponentsInChildren<Toggle>(true).ToList().ForEach(toggle =>
            {
                if (toggle != null) toggle.onValueChanged.AddListener(_ => PlayButtonAudio());
            });
        }

        public void ExecuteShow(UIMenu menu)
        {
        
        }

        public void ExecuteHide(UIMenu menu)
        {
        }
    
        private void PlayButtonAudio()
        {
        
        }
    }
}