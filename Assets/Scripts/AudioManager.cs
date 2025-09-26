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
            musicSource.mute = false;
        }
        if (sfxSource != null)
        {
            sfxSource.spatialBlend = 0f;
            sfxSource.mute = false;
            sfxSource.loop = false;
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
    
    public void StartMoveLoop(bool eating)
    {
        var src = sfxSource != null ? sfxSource : musicSource;
        if (src == null) return;
        var clip = sfxMoveNotEating;
        if (clip == null) return;
        if (src.clip != clip) src.clip = clip;
        src.loop = true;
        if (!src.isPlaying) src.Play();
    }

    public void StopMoveLoop()
    {
        var src = sfxSource != null ? sfxSource : musicSource;
        if (src == null) return;
        if (src.isPlaying && src.clip == sfxMoveNotEating) src.Stop();
        src.loop = false;
        src.clip = null;
    }

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
