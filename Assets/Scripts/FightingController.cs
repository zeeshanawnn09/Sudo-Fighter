using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]

public class FightingController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units/sec")]
    public float PlayerSpeed = 1.0f;

    [Tooltip("Rotation smoothing factor (higher = snappier). Uses Slerp with Time.deltaTime.")]
    public float PlayerRotation = 10.0f;

    private Animator animator;
    private CharacterController characterController;

    [Header("Fight Settings")]
    public float attackDelay = 0.5f;
    public float dodgeDist = 5f;
    public float AttackRadius = 2.2f;
    public int hitDamage = 5;

    [Tooltip("Animation state names used for attacks (index matched to attack number)")]
    public string[] FightAnimations = { "Attack1Animation", "Attack2Animation", "Attack3Animation", "Attack4Animation" };

    public Transform[] Opponents;
    private float TimeOfLastAttack;

    [Header("Health Settings")]
    public int maxHP = 100;
    public int currHP;
    public HealthBarBehavior healthBarBehavior;

    [Header("Sounds & Effects Settings")]
    // Particle systems attached to the player that are played for each attack
    public ParticleSystem HitEffect1;
    public ParticleSystem HitEffect2;
    public ParticleSystem HitEffect3;
    public ParticleSystem HitEffect4;

    [Header("Input (new Input System)")]
    [Tooltip("Input Action (Vector2) used for player movement. If not assigned, legacy Input axes 'Horizontal'/'Vertical' will be used.")]
    public InputActionReference moveAction;

    [Tooltip("Assign InputActions from your Input Actions asset. Each attack can be bound to a button.")]
    public InputActionReference attack1Action;
    public InputActionReference attack2Action;
    public InputActionReference attack3Action;
    public InputActionReference attack4Action;

    [Tooltip("Dodge action (button)")]
    public InputActionReference dodgeAction;

    [Header("ScriptableObject Data (optional)")]
    public SoundEffectsData soundEffectsData;

    [Header("Controller Feedback")]
    [Tooltip("Low frequency motor intensity (0-1) when hit")]
    public float hitVibrationLow = 0.5f;
    [Tooltip("High frequency motor intensity (0-1) when hit")]
    public float hitVibrationHigh = 0.5f;
    [Tooltip("Duration of vibration in seconds")]
    public float hitVibrationDuration = 0.3f;

    // --- Unity lifecycle -------------------------------------------------

    private void Awake()
    {
        currHP = maxHP;
        if (healthBarBehavior != null)
            healthBarBehavior.OnStartHealth(currHP);

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        // Set DualSense lightbar (or compatible controller light) to green on game start (best-effort)
        TrySetControllerLightColor(Color.green);
    }

    private void OnEnable()
    {
        // Subscribe to attack/dodge actions (if assigned)
        if (attack1Action?.action != null) attack1Action.action.performed += OnAttack1Performed;
        if (attack2Action?.action != null) attack2Action.action.performed += OnAttack2Performed;
        if (attack3Action?.action != null) attack3Action.action.performed += OnAttack3Performed;
        if (attack4Action?.action != null) attack4Action.action.performed += OnAttack4Performed;
        if (dodgeAction?.action != null) dodgeAction.action.performed += OnDodgePerformed;

        // Ensure the move action is enabled so ReadValue works
        if (moveAction?.action != null && !moveAction.action.enabled) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (attack1Action?.action != null) attack1Action.action.performed -= OnAttack1Performed;
        if (attack2Action?.action != null) attack2Action.action.performed -= OnAttack2Performed;
        if (attack3Action?.action != null) attack3Action.action.performed -= OnAttack3Performed;
        if (attack4Action?.action != null) attack4Action.action.performed -= OnAttack4Performed;
        if (dodgeAction?.action != null) dodgeAction.action.performed -= OnDodgePerformed;

        if (moveAction?.action != null && moveAction.action.enabled) moveAction.action.Disable();
    }

    private void Update()
    {
        PlayerMovement();
        FrwdDodgeBehavior();
        AttackAnims();
    }

    // --- Movement & actions ---------------------------------------------

     void PlayerMovement()
    {
        // Read movement from the new Input System if assigned, otherwise fall back to the old Input
        Vector2 move = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
        {
            move = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            //move.x = Input.GetAxis("Horizontal");
            move.y = Input.GetAxis("Vertical");
        }

        Vector3 Movement = new Vector3(0f, 0f, move.y);

        if (Mathf.Abs(move.y) > 0f)
        {
            // Always facing the movement direction
            Quaternion Rotation = Quaternion.LookRotation(Movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, Rotation, PlayerRotation * Time.deltaTime);
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }

        characterController.Move(Movement * PlayerSpeed * Time.deltaTime);

    }


    private void FrwdDodgeBehavior()
    {
        // Legacy fallback (keyboard) for dodge
        if (Input.GetKeyDown(KeyCode.G))
        {
            animator.Play("DodgeFrontAnimation");
            Vector3 dodgeDir = transform.forward * dodgeDist;
            characterController.Move(dodgeDir);
        }
    }

    private void AttackAnims()
    {
        // Legacy fallback (keyboard) for attacks
        if (Input.GetKeyDown(KeyCode.Alpha1)) FightBehavior(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) FightBehavior(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) FightBehavior(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) FightBehavior(3);
    }

    // --- Combat ---------------------------------------------------------

    private void FightBehavior(int attackIndex)
    {
        if (Time.time - TimeOfLastAttack <= attackDelay)
        {
            Debug.Log("Cool down, cannot perform any attack");
            return;
        }

        string animToPlay = (FightAnimations != null && FightAnimations.Length > attackIndex)
            ? FightAnimations[attackIndex]
            : null;

        if (!string.IsNullOrEmpty(animToPlay)) animator.Play(animToPlay);

        Debug.Log($"Performed an attack {attackIndex + 1} dealing {hitDamage} damage");
        TimeOfLastAttack = Time.time;

        if (Opponents != null)
        {
            foreach (Transform opponent in Opponents)
            {
                if (Vector3.Distance(transform.position, opponent.position) <= AttackRadius)
                {
                    var opp = opponent.GetComponent<OpponentAIController>();
                    if (opp != null) opp.StartCoroutine(opp.OnHitAnim(hitDamage));
                }
            }
        }
    }

    public IEnumerator OnHitAnim(int hit)
    {
        // Start short vibration to signal hit
        SetControllerVibration(hitVibrationLow, hitVibrationHigh);

        yield return new WaitForSeconds(hitVibrationDuration);

        // Play random hit sound from assigned SoundEffectsData (if provided)
        if (soundEffectsData?.hitSounds != null && soundEffectsData.hitSounds.Length > 0)
        {
            var clips = soundEffectsData.hitSounds;
            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        currHP -= hit;
        if (healthBarBehavior != null) healthBarBehavior.SetHealth(currHP);

        // Update lightbar color based on health threshold
        if (currHP <= 40)
        {
            TrySetControllerLightColor(Color.red);
        }

        if (currHP <= 0) PlayerDeathBehavior();

        animator.Play("HitDamageAnimation");

        // Stop vibration after processing hit
        StopControllerVibration();
    }

    private void PlayerDeathBehavior() => Debug.Log("Player Died!!!");

    // --- Attack effects (particle systems) -----------------------------

    public void AttackEffect1() { if (HitEffect1 != null) HitEffect1.Play(); }
    public void AttackEffect2() { if (HitEffect2 != null) HitEffect2.Play(); }
    public void AttackEffect3() { if (HitEffect3 != null) HitEffect3.Play(); }
    public void AttackEffect4() { if (HitEffect4 != null) HitEffect4.Play(); }

    // --- Input callbacks ------------------------------------------------

    private void OnAttack1Performed(InputAction.CallbackContext ctx) => FightBehavior(0);
    private void OnAttack2Performed(InputAction.CallbackContext ctx) => FightBehavior(1);
    private void OnAttack3Performed(InputAction.CallbackContext ctx) => FightBehavior(2);
    private void OnAttack4Performed(InputAction.CallbackContext ctx) => FightBehavior(3);

    private void OnDodgePerformed(InputAction.CallbackContext ctx)
    {
        animator.Play("DodgeFrontAnimation");
        Vector3 dodgeDir = transform.forward * dodgeDist;
        characterController.Move(dodgeDir);
    }

    private void OnDestroy()
    {
        // Ensure vibration is stopped when object is destroyed
        StopControllerVibration();
    }

    // --- Controller feedback helpers ----------------------------------

    private void SetControllerVibration(float low, float high)
    {
        try
        {
            // Set vibration for all connected gamepads (prefer DualSense but fallback to any)
            foreach (var gp in Gamepad.all)
            {
                gp.SetMotorSpeeds(low, high);
            }
        }
        catch (Exception) { /* ignore - runtime may not support motors */ }
    }

    private void StopControllerVibration()
    {
        try
        {
            foreach (var gp in Gamepad.all)
            {
                gp.SetMotorSpeeds(0f, 0f);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Try to set the controller light color (DualSense/DualShock/etc.) using reflection.
    /// This method attempts to find a light-setting method on the device and invoke it safely.
    /// </summary>
    private void TrySetControllerLightColor(Color color)
    {
        try
        {
            var devices = UnityEngine.InputSystem.InputSystem.devices;
            foreach (var dev in devices)
            {
                var prod = dev.description.product?.ToLower() ?? string.Empty;

                // Quick check for PlayStation-like controllers
                if (!prod.Contains("dual") && !prod.Contains("wireless") && !prod.Contains("playstation") && !prod.Contains("ps5"))
                    continue;

                var type = dev.GetType();
                // Find a method that likely sets light color
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo candidate = null;
                foreach (var m in methods)
                {
                    var name = m.Name.ToLower();
                    if (name.Contains("light") || name.Contains("setcolor") || name.Contains("setlight"))
                    {
                        var pars = m.GetParameters();
                        if (pars.Length == 1 && pars[0].ParameterType == typeof(Color)) { candidate = m; break; }
                        if (pars.Length == 3 && pars.All(p => p.ParameterType == typeof(byte))) { candidate = m; break; }
                        if (pars.Length == 1 && pars[0].ParameterType == typeof(uint)) { candidate = m; break; }
                    }
                }

                if (candidate == null) continue;

                var ps = candidate.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(Color))
                {
                    candidate.Invoke(dev, new object[] { color });
                    return;
                }
                else if (ps.Length == 3 && ps.All(p => p.ParameterType == typeof(byte)))
                {
                    byte r = (byte)(color.r * 255);
                    byte g = (byte)(color.g * 255);
                    byte b = (byte)(color.b * 255);
                    candidate.Invoke(dev, new object[] { r, g, b });
                    return;
                }
                else if (ps.Length == 1 && ps[0].ParameterType == typeof(uint))
                {
                    uint packed = ((uint)(color.r * 255) << 16) | ((uint)(color.g * 255) << 8) | ((uint)(color.b * 255));
                    candidate.Invoke(dev, new object[] { packed });
                    return;
                }
            }
        }
        catch (Exception)
        {
            // best-effort; ignore if unsupported
        }
    }
}
