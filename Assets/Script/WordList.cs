using UnityEngine;

namespace Script
{
    [CreateAssetMenu(menuName = "Nollopa/WordList", fileName = "New WordList")]
    public class WordList : ScriptableObject
    {
        public string[] shortWords;
        public string[] mediumWords;
        public string[] longWords;
    }
}
