using UnityEngine;

namespace Managers
{
    public class PostTipData : ScriptableObject
    {

        [TextArea(4, 8)] public string[] InterviewTips;
    }
}