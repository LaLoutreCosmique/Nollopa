using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script
{
    public class SoundFX : MonoBehaviour
    {
        public static SoundFX Instance;

        [SerializeField] AudioSource source;
        [SerializeField] SoundData[] data;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void PlaySound(SoundType type)
        {
            foreach (SoundData sound in data)
            {
                if (sound.type != type) continue;
                CreateSource(sound);
                return;
            }
        }

        void CreateSource(SoundData sound)
        {
            AudioSource newSource = Instantiate(source, Vector3.zero, quaternion.identity);
            newSource.clip = sound.clip;
            newSource.volume = sound.volume;
            newSource.Play();
            Destroy(newSource.gameObject, newSource.clip.length);
        }
        
        [System.Serializable]
        public struct SoundData
        {
            [SerializeField] public SoundType type;
            [FormerlySerializedAs("source")] [SerializeField] public AudioClip clip;
            [SerializeField] public float volume;
        }
    }

    public enum SoundType
    {
        Attack,
        PlayerHurt,
        EnemyHurt,
        Disappear,
        ValidLetter,
        InvalidLetter,
        WordDestroy,
        Step,
        DisplayLetter
    }
}
