using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            [TextArea(4, 8)]
            public string text;

            public enum PanelType { None, CV, Events }
            public PanelType panelToActivate;
        }

        public string speakerName = "Career Coach";
        public Sprite characterPortrait;
        public Entry[] entries;
    }
}