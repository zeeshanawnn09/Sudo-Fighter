using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

// Keyword recognizer and Windows speech APIs are only available on some platforms.
// We wrap the runtime listener in a platform check so the file can compile on other targets.

[Serializable]
public class VoiceCommandEntry
{
    [Tooltip("The keyword to listen for (case-insensitive). Example: 'kick'")]
    public string keyword;

    [Tooltip("Optional reference to a FightingController to call built-in wrappers on.")]
    public FightingController fightingController;

    [Tooltip("If true, call PerformAttackByIndex on the FightingController using AttackIndex.")]
    public bool callAttackByIndex = false;

    [Tooltip("Attack index (zero-based). Use 0 for first attack.")]
    public int attackIndex = 0;

    [Tooltip("If true, call PlayAnimationByName on the FightingController using AnimationName.")]
    public bool playAnimation = false;

    [Tooltip("Animation state name to play on the FightingController's Animator.")]
    public string animationName;

    [Tooltip("An arbitrary UnityEvent that will be invoked when the keyword is recognized. Use this to wire Inspector-based actions.")]
    public UnityEvent onRecognized;
}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR

/// <summary>
/// Listens for simple keyword speech commands using Unity's KeywordRecognizer and invokes
/// configured actions. This is a lightweight, on-device keyword recognizer (works in the Editor and Windows builds).
/// Notes:
/// - This uses UnityEngine.Windows.Speech.KeywordRecognizer. Platform availability depends on Unity target and runtime.
/// - PS5 controller internal microphone is typically not directly exposed to a PC build; see README/docs for limitations.
/// </summary>
public class VoiceCommandListener : MonoBehaviour
{
    [Tooltip("List of voice commands to listen for and their mapped actions.")]
    public VoiceCommandEntry[] commands = new VoiceCommandEntry[0];

    [Tooltip("Confidence level for the recognition. Higher = fewer false positives.")]
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;

    private KeywordRecognizer recognizer;
    private string[] keywords = new string[0];

    private void OnEnable()
    {
        SetupRecognizer();
    }

    private void OnDisable()
    {
        StopRecognizer();
    }

    private void SetupRecognizer()
    {
        if (commands == null || commands.Length == 0)
        {
            Debug.Log("VoiceCommandListener: No commands configured.");
            return;
        }

        // Build keyword list (unique, non-empty)
        keywords = commands
            .Where(c => !string.IsNullOrWhiteSpace(c.keyword))
            .Select(c => c.keyword.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (keywords.Length == 0)
        {
            Debug.Log("VoiceCommandListener: No valid keywords found.");
            return;
        }

        try
        {
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
            Debug.Log($"VoiceCommandListener: Started listening for {keywords.Length} keywords.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"VoiceCommandListener: Failed to start KeywordRecognizer - {ex.Message}");
            recognizer = null;
        }
    }

    private void StopRecognizer()
    {
        if (recognizer != null)
        {
            try
            {
                recognizer.OnPhraseRecognized -= OnPhraseRecognized;
                if (recognizer.IsRunning) recognizer.Stop();
            }
            catch { }
            finally
            {
                recognizer.Dispose();
                recognizer = null;
            }
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        var text = args.text?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(text)) return;

        // Find all matching entries (in case multiple entries use the same keyword)
        var matches = commands.Where(c => string.Equals(c.keyword?.Trim(), text, StringComparison.OrdinalIgnoreCase));
        foreach (var entry in matches)
        {
            try
            {
                if (entry.callAttackByIndex && entry.fightingController != null)
                {
                    entry.fightingController.PerformAttackByIndex(entry.attackIndex);
                }

                if (entry.playAnimation && entry.fightingController != null)
                {
                    entry.fightingController.PlayAnimationByName(entry.animationName);
                }

                // Invoke arbitrary UnityEvent for flexible wiring in the Inspector
                entry.onRecognized?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"VoiceCommandListener: Exception while handling keyword '{entry.keyword}': {ex.Message}");
            }
        }
    }

    // Helper to refresh recognizer if commands changed at runtime
    public void RefreshCommands()
    {
        StopRecognizer();
        SetupRecognizer();
    }
}
#else
// Speech/Keyword recognition not available on this platform; provide a stub so code compiles.
public class VoiceCommandListener : MonoBehaviour { }
#endif
