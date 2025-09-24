using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum Mode { StartScene, LevelScene }
    public Mode sceneMode = Mode.LevelScene;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip bgmStartScene;
    public AudioClip bgmIntro;
    public AudioClip bgmNormal;
    public AudioClip bgmScared;
    public AudioClip bgmDeadGhost;

    public AudioClip sfxMoveNotEating;
    public AudioClip sfxEatPellet;
    public AudioClip sfxWallHit;
    public AudioClip sfxDeath;

    float introStartTime;
    bool introSwitched;

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.spatialBlend = 0f;
            musicSource.loop = false;
        }

        if (sceneMode == Mode.StartScene) PlayStartSceneLoop();
        else PlayIntroThenNormal();
    }

    void Update()
    {
        if (sceneMode != Mode.LevelScene || musicSource == null || introSwitched) return;

        bool introFinished = !musicSource.isPlaying;
        bool threeSecondsElapsed = (Time.time - introStartTime) >= 3f;

        if (introFinished || threeSecondsElapsed)
        {
            introSwitched = true;
            PlayLoop(bgmNormal);
        }
    }

    public void PlayNormalLoop()    => PlayLoop(bgmNormal);
    public void PlayScaredLoop()    => PlayLoop(bgmScared);
    public void PlayDeadGhostLoop() => PlayLoop(bgmDeadGhost);

    public void PlayMove(bool eating)
    {
        var clip = eating ? sfxEatPellet : sfxMoveNotEating;
        PlaySfx(clip);
    }

    public void PlayWallHit() => PlaySfx(sfxWallHit);
    public void PlayDeath()   => PlaySfx(sfxDeath);

    void PlayStartSceneLoop()
    {
        if (musicSource == null || bgmStartScene == null) return;
        musicSource.clip = bgmStartScene;
        musicSource.loop = true;
        musicSource.Play();
    }

    void PlayIntroThenNormal()
    {
        introSwitched = false;
        introStartTime = Time.time;

        if (musicSource == null) return;

        if (bgmIntro != null)
        {
            musicSource.loop = false;
            musicSource.clip = bgmIntro;
            musicSource.Play();
        }
        else
        {
            PlayLoop(bgmNormal);
            introSwitched = true;
        }
    }

    void PlayLoop(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource != null)
        {
            sfxSource.spatialBlend = 0f;
            sfxSource.PlayOneShot(clip);
        }
        else if (musicSource != null)
        {
            musicSource.PlayOneShot(clip);
        }
    }
}
