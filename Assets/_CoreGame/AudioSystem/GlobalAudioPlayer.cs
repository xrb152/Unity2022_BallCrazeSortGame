using UnityEngine;

public static class GlobalAudioPlayer
{

    public static AudioPlayer audioPlayer;
    public static void ChangeVolumeMusic(float value)
    {
        if (audioPlayer != null)
            AudioPlayer.MusicSource.volume = AudioPlayer.MusicSource.volume == 0f ? 0f : value;
    }
    public static void PlaySFX(string sfxName)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playSFX(sfxName);
    }
    public static void PlaySFXWithAudioItem(AudioItem item)
    {
        if (audioPlayer != null && item != null) audioPlayer.playSFXWithAudioItem(item);
    }
    public static void PlayLoopSFX(string sfxName, float time, Transform parent = null)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playLoopSFX(sfxName, time, parent);
    }
    public static void PlayMultipleSFX(string sfxName)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playMultipleSFX(sfxName);
    }
    public static void PlaySFXAtPosition(string sfxName, Vector3 position)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playSFXAtPosition(sfxName, position);
    }
    public static void PlayGrowUpSFX(string sfxName, int id)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playGrowUpSFX(sfxName, id);
    }
    public static void PlaySFXAtPosition(string sfxName, Vector3 position, Transform parent)
    {
        if (audioPlayer != null && sfxName != "") audioPlayer.playSFXAtPosition(sfxName, position, parent);
    }
    public static void PlayeMusicFromIntro(string name)
    {
        if (audioPlayer != null) audioPlayer.PlayMusicFromIntro(name);
    }
    public static void PlayMusic(string musicName)
    {
        if (audioPlayer != null) audioPlayer.playMusic(musicName);
    }
}