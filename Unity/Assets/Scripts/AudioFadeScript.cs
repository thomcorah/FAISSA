using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioFadeScript
{
    public static IEnumerator FadeOut(FMOD.Studio.EventInstance instance, float FadeTime)
    {
        float volume = 1.0f;
        while (volume > 0)
        {
            volume -= Time.deltaTime / FadeTime;

            yield return null;
        }

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        float startVolume = 0.2f;

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = 1f;
    }
}
