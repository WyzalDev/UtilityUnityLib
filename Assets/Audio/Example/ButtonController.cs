using DG.Tweening;
using UnityEngine;
using WyzalUtilities.Audio;

public class ButtonController : MonoBehaviour
{
    private static FadeSettings exampleFade = new FadeSettings()
    {
        durationIn = 1f,
        durationOut = 1f,
        easeIn = Ease.InCubic,
        easeOut = Ease.OutCubic
    };
    public static void PlayMusicOneWithFade()
    {
        AudioContext.PlayGlobalMusic("croco", exampleFade);
    }
    
    public static void PlayMusicTwoWithFade()
    {
        AudioContext.PlayGlobalMusic("french", exampleFade, "NewLocation");
    }
    
    public static void PlaySfxOne()
    {
        AudioContext.PlayGlobalSfx("gay");
    }
    
    public static void PlaySfxTwo()
    {
        AudioContext.PlayGlobalSfx("quack", "NewLocation");
    }
    
    public static void PlayMusicOneWithoutFade()
    {
        AudioContext.PlayGlobalMusic("croco");
    }
    
    public static void PlayMusicTwoWithoutFade()
    {
        AudioContext.PlayGlobalMusic("french", null, "NewLocation");
    }
}
