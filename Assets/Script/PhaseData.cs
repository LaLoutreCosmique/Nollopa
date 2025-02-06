using UnityEngine;

namespace Script
{
    [CreateAssetMenu(menuName = "Nollopa/Phase/Phase data", fileName = "New Phase")]
    public class PhaseData : ScriptableObject
    {
        public bool freezeTimer;
        public Enemy enemy;
        public int greenNerf;
        public int redNerf;
        public bool orangeNerf;
    }
}