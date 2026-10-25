using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip normalMusic;
    [SerializeField] private AudioClip scaredMusic;
    [SerializeField] private AudioClip deadMusic;
    [SerializeField] private AudioClip wallSound;
    [SerializeField] private AudioClip bonusSound;
    [SerializeField] private AudioClip enemySound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource effectsSource;
    private AudioSource musicSource;

    public float DeathDuration => deathSound != null ? Mathf.Max(.5f, deathSound.length) : .5f;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
    }
    private void SetMusic(AudioClip clip, bool loop)
    {
        if (musicSource.clip == clip && musicSource.isPlaying && musicSource.loop == loop) return;
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        if (clip != null) musicSource.Play();
    }
    public void PlayIntro() { SetMusic(introMusic, true); }
    public void UpdateMusic(bool anyDead, bool scared) { SetMusic(anyDead ? deadMusic : scared ? scaredMusic : normalMusic, true); }
    public void StopMusic() { musicSource.Stop(); }
    public void PlayWall() { if (wallSound != null) effectsSource.PlayOneShot(wallSound); }
    public void PlayBonus() { if (bonusSound != null) effectsSource.PlayOneShot(bonusSound); }
    public void PlayEnemy() { if (enemySound != null) effectsSource.PlayOneShot(enemySound); }
    public void PlayDeath()
    {
        musicSource.Stop();
        effectsSource.Stop();
        if (deathSound != null) effectsSource.PlayOneShot(deathSound);
    }
}
