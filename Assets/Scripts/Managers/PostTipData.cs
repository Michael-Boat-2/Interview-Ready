using UnityEngine;

namespace Managers
{
    [CreateAssetMenu(fileName = "PostTip Data", menuName = "Game/PostTip Data")]
    public class PostTipData : ScriptableObject
    {

        [TextArea(4, 8)] public string[] InterviewTips;
    }
}