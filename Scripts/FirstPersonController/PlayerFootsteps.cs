using System.Collections;
using System.Collections.Generic;
using EventObjects;
using UnityEngine;

namespace DREditor.FPC
{

    public class PlayerFootsteps : MonoBehaviour
    {
        public AudioSource AudioSource;
        public AudioClip[] StepSounds;

        public BoolWithEvent Running;

        public float Volume;

        private bool cd;


        public void Step()
        {
            if (cd) return;

            var sound = StepSounds[Random.Range(0, StepSounds.Length - 1)];
            var pitch = Random.Range(0.9f, 1.1f);

            AudioSource.pitch = pitch;
            AudioSource.clip = sound;
            AudioSource.volume = Running.Value ? Volume : Volume / 3f;
            AudioSource.Play();

            cd = true;
            StartCoroutine(Cooldown());

        }

        IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(0.1f);
            cd = false;
        }

    }
}