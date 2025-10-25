using UnityEngine;

[CreateAssetMenu(menuName = "SudoFighter/SoundEffects", fileName = "SoundEffectsData")]
public class SoundEffectsData : ScriptableObject
{
    [Tooltip("Audio clips played when player is hit or when attacks land")]
    public AudioClip[] hitSounds;
}
