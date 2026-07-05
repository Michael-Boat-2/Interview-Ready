using UnityEngine;
    
namespace Managers
{
    
    [CreateAssetMenu(fileName = "PlayerProfileData", menuName = "Game/Player Profile Data")]
    public class PlayerProfileData : ScriptableObject
    {
        public string playerName = "";
        public Sprite playerAvatar;
    }
    
}