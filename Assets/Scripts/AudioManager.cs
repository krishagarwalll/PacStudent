using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum Mode { StartScene, LevelScene }
    public Mode sceneMode = Mode.LevelScene;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip bgmStartScene;
    public AudioClip bgmIntro;
    public AudioClip bgmNormal;
    public AudioClip bgmScared;
    public AudioClip bgmDeadGhost;

    [Header("SFX")]
    public AudioClip sfxMoveNotEating;
    public AudioClip sfxEatPellet;
    public AudioClip sfxWallHit;
    public AudioClip sfxDeath;

    float introStartTime;
    bool introSwitched;

    AudioSource oneShot;

    void Start()
    {
        if (musicSource)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = false;
            musicSource.spatialBlend = 0f;
            musicSource.mute = false;
        }
        if (sfxSource)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.mute = false;
        }

        if (sceneMode == Mode.StartScene) PlayStartSceneLoop();
        else PlayIntroThenNormal();
    }

    void Update()
    {
        if (sceneMode != Mode.LevelScene || !musicSource || introSwitched) return;
        bool introFinished = !musicSource.isPlaying;
        bool threeSecondsElapsed = (Time.time - introStartTime) >= 3f;
        if (introFinished || threeSecondsElapsed)
        {
            introSwitched = true;
            PlayLoop(bgmNormal);
        }
    }
    
    public void StartMoveLoop(bool aboutToEat)
    {
        var src = sfxSource ? sfxSource : musicSource;
        if (!src) return;

        var clip = aboutToEat && sfxEatPellet ? sfxEatPellet : sfxMoveNotEating;
        if (!clip) return;

        if (src.clip != clip) { src.clip = clip; src.time = 0f; }
        src.loop = true;
        if (!src.isPlaying) src.Play();
    }

    public void StopMoveLoop()
    {
        var src = sfxSource ? sfxSource : musicSource;
        if (!src) return;

        if (src.isPlaying && (src.clip == sfxMoveNotEating || src.clip == sfxEatPellet))
            src.Stop();
        src.loop = false;
        src.clip = null;
    }

    public void PlayWallHit()       { PlaySfx(sfxWallHit); }
    public void PlayDeath()         { PlaySfx(sfxDeath); }
    public void PlayNormalLoop()    { PlayLoop(bgmNormal); }
    public void PlayScaredLoop()    { PlayLoop(bgmScared); }
    public void PlayDeadGhostLoop() { PlayLoop(bgmDeadGhost); }
    
    AudioSource EnsureOneShot()
    {
        if (oneShot) return oneShot;
        var go = new GameObject("OneShotSource");
        go.transform.SetParent(transform, false);
        oneShot = go.AddComponent<AudioSource>();
        oneShot.playOnAwake = false;
        oneShot.loop = false;
        oneShot.spatialBlend = 0f;
        oneShot.volume = 1f;
        return oneShot;
    }

    void PlayStartSceneLoop()
    {
        if (!musicSource || !bgmStartScene) return;
        musicSource.clip = bgmStartScene;
        musicSource.loop = true;
        musicSource.Play();
    }

    void PlayIntroThenNormal()
    {
        introSwitched = false;
        introStartTime = Time.time;
        if (!musicSource) return;

        if (bgmIntro)
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
        if (!musicSource || !clip) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void PlaySfx(AudioClip clip)
    {
        if (!clip) return;

        var src = EnsureOneShot();
        if (src) { src.PlayOneShot(clip); return; }

        var alt = sfxSource ? sfxSource : musicSource;
        if (alt) { alt.PlayOneShot(clip); return; }

        var pos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, pos, 1f);
    }
}