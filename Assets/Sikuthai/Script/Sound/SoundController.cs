using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;
    private void Update()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        GetMusicAndSFXButton();
    }


    //public Slider _musicSlider, _sfxSlider;
    // public float volume;
    public void ToggleMusic()
    {
        AudioController.Instance.ToggleMusic();
    }
    public void PlayMusic()
    {
        AudioController.Instance.playMusic();
    }
    public void ToggleSFX()
    {
        AudioController.Instance.ToggleSFX();
    }
    public void PlaySFX()
    {
        AudioController.Instance.playSFX();
    }
    public void ShakeSound()
    {
        AudioController.Instance.PlaySFX("shake");
    }
    public void ShowLogo0Sound()
    {
        AudioController.Instance.PlaySFX("showLogo");
    }
    public void CoinSound()
    {
        AudioController.Instance.PlaySFX("coin");
    }
    public void ShowLogoResult()
    {
        AudioController.Instance.PlaySFX("showResult");
    }
    public void ChangeBetSound()
    {
        AudioController.Instance.PlaySFX("changeBet");
    }
    public void Tap()
    {
        AudioController.Instance.PlaySFX("tap");
    }
    /*   public void MusicVolume()
       {
           AudioManager.Instance.MusicVolume(_musicSlider.value);
       }
       public void SFXVolume()
       {
           AudioManager.Instance.SFXVolume(_sfxSlider.value);
       }*/
    #region Sound button
    public GameObject music, sfx, muteMusic, mutesfx;
    private string musicStr, sfxStr, muteMusicStr, mutesfxStr;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    public void GetMusicAndSFXButton()
    {
        musicStr = PlayerPrefs.GetString("Music");
        if (musicStr == "clicked")
        {
            music.SetActive(false);
            muteMusic.SetActive(true);
            ToggleMusic();
        }
        muteMusicStr = PlayerPrefs.GetString("MuteMusic");
        if (muteMusicStr == "clicked")
        {
            music.SetActive(true);
            muteMusic.SetActive(false);
        }
        sfxStr = PlayerPrefs.GetString("SFX");
        if (sfxStr == "clicked")
        {
            sfx.SetActive(false);
            mutesfx.SetActive(true);
            ToggleSFX();
        }
        mutesfxStr = PlayerPrefs.GetString("MuteSFX");
        if (mutesfxStr == "clicked")
        {
            sfx.SetActive(true);
            mutesfx.SetActive(false);
        }
        //if (PlayerPrefs.HasKey("musicVolume"))
        //{
        //    musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        //}
        //if (PlayerPrefs.HasKey("sfxVolume"))
        //{
        //    sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        //}

    }

    //public Slider musicSlider;
    //public Slider sfxSlider;
    //public void VolumeMusic()
    //{

    //    PlayerPrefs.SetFloat("musicVolume", musicVolume);
    //    AudioContoller.Instance.MusicVolume(musicSlider.value);
    //    musicVolume = musicSlider.value;

    //}
    //public void VolumeSFX()
    //{
    //    PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
    //    AudioContoller.Instance.SFXVolume(sfxSlider.value);
    //    sfxVolume = sfxSlider.value;
    //}
    public void MusicButton()
    {
        PlayerPrefs.SetString("Music", "clicked");
        PlayerPrefs.SetString("MuteMusic", "null");
    }
    public void MuteMusicButton()
    {
        PlayerPrefs.SetString("Music", "null");
        PlayerPrefs.SetString("MuteMusic", "clicked");


    }

    public void SFXButton()
    {
        PlayerPrefs.SetString("SFX", "clicked");
        PlayerPrefs.SetString("MuteSFX", "null");

    }
    public void MuteSFXButton()
    {
        PlayerPrefs.SetString("SFX", "null");
        PlayerPrefs.SetString("MuteSFX", "clicked");

    }
    #endregion end sound button
}

