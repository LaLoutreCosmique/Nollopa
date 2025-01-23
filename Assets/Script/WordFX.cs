using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Script
{
    public class WordFX : MonoBehaviour
    {
        [SerializeField] KeyFX keyPrefab;
        [SerializeField] float padding;

        List<KeyFX> m_Keys = new ();
        int index;
        bool canShake = true;

        public IEnumerator DisplayWord(string word)
        {
            canShake = false;
            for (int i = 0; i < word.Length; i++)
            {
                Vector3 keyPosition = new Vector3((i - (word.Length - 1) / 2.0f) * padding, 3);
                m_Keys.Add(Instantiate(keyPrefab, transform).Init(word.ToCharArray()[i], keyPosition));
                SoundFX.Instance.PlaySound(SoundType.DisplayLetter);
                yield return new WaitForSeconds(0.07f);
            }

            canShake = true;
        }

        public void Press()
        {
            m_Keys[index].Press();
            index++;
            SoundFX.Instance.PlaySound(SoundType.ValidLetter);
        }

        public void Cancel()
        {
            SoundFX.Instance.PlaySound(SoundType.InvalidLetter);
            
            if (canShake)
                transform.DOShakePosition(0.2f, .5f).onComplete = () => canShake = true;
            canShake = false;
            
            foreach (KeyFX key in m_Keys)
                key.Release();

            index = 0;
        }

        public void Finish()
        {
            SoundFX.Instance.PlaySound(SoundType.WordDestroy);
            foreach (KeyFX key in m_Keys)
                Destroy(key.gameObject);

            m_Keys.RemoveRange(0, m_Keys.Count);
            index = 0;
        }
    }
}
