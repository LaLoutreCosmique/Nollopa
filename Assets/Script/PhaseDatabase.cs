using UnityEngine;

namespace Script
{
    [CreateAssetMenu(menuName = "Nollopa/Phase/Phase database", fileName = "New Phase Database")]
    public class PhaseDatabase : ScriptableObject
    {
        public PhaseData[] data;
    }
}