using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip normalMusic;

    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.spatialBlend = 0f;
    }

    private IEnumerator Start()
    {
        if (normalMusic == null)
        {
            Debug.LogWarning("Assign the normal music clip.", this);
            yield break;
        }

        if (introMusic != null)
        {
            musicSource.clip = introMusic;
            musicSource.Play();

            float elapsed = 0f;

            while (musicSource.isPlaying && elapsed < 3f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = normalMusic;
        musicSource.loop = true;
        musicSource.Play();
    }
}