using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]

public class FightingController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float PlayerSpeed = 1.0f;
    public float PlayerRotation = 10.0f;

    [Header("Sensor mapping")]
    public float accelSensitivity = 1.5f;
    public float gyroSensitivity = 60f;
    public float smoothing = 5f;
    public float deadzone = 0.02f;

    [Header("Input Actions (optional)")]
    public InputActionReference accelAction;
    public InputActionReference gyroAction;
    Vector3 smoothedAccel = Vector3.zero;
    Vector3 smoothedGyro = Vector3.zero;
    float gyroPitch = 0f;

    [Header("Gyro Tilt Settings")]
    public float gyroTiltMoveSpeed = 2.0f;
    public float gyroTiltThreshold = 5f;
    public float gyroTiltClamp = 60f;

    private Animator animator;
    private CharacterController characterController;

    [Header("Fight Settings")]
    public float attackDelay = 0.5f;
    public float dodgeDist = 5f;
    public float AttackRadius = 2.2f;
    public int hitDamage = 5;
    public string[] FightAnimations = { "Attack1Animation", "Attack2Animation", "Attack3Animation", "Attack4Animation" };
    public Transform[] Opponents;
    private float TimeOfLastAttack;

    [Header("Health Settings")]
    public int maxHP = 100;
    public int currHP;
    public HealthBarBehavior healthBarBehavior;

    [Header("Sounds & Effects Settings")]
    public ParticleSystem HitEffect1;
    public ParticleSystem HitEffect2;
    public ParticleSystem HitEffect3;
    public ParticleSystem HitEffect4;

    [Header("Input System Settings")]
    public InputActionReference moveAction;
    public InputActionReference attack1Action;
    public InputActionReference attack2Action;
    public InputActionReference attack3Action;
    public InputActionReference attack4Action;
    public InputActionReference dodgeAction;

    [Header("ScriptableObject Data")]
    public SoundEffectsData soundEffectsData;

    [Header("Controller Feedback")]
    public float hitVibrationLow = 0.5f;
    public float hitVibrationHigh = 0.5f;
    public float hitVibrationDuration = 0.3f;

    private void Awake()
    {
        currHP = maxHP;
        if (healthBarBehavior != null)
            healthBarBehavior.OnStartHealth(currHP);

        animator = GetComponent<Animator>();
        if (animator != null) animator.applyRootMotion = false;
        characterController = GetComponent<CharacterController>();

        // Set DualSense lightbar color to green on game start
        TrySetControllerLightColor(Color.green);
    }

    private void OnEnable()
    {
        // Link input actions to callbacks
        if (attack1Action?.action != null) attack1Action.action.performed += OnAttack1Performed;
        if (attack2Action?.action != null) attack2Action.action.performed += OnAttack2Performed;
        if (attack3Action?.action != null) attack3Action.action.performed += OnAttack3Performed;
        if (attack4Action?.action != null) attack4Action.action.performed += OnAttack4Performed;
        if (dodgeAction?.action != null) dodgeAction.action.performed += OnDodgePerformed;
        if (moveAction?.action != null && !moveAction.action.enabled) moveAction.action.Enable();
    }

    private void OnDisable()
    {
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


    void PlayerMovement()
    {
        // Read movement from the new Input System
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
        GyroscopeBehavior();

    }
    
    void GyroscopeBehavior()
    {
        Vector3 rawAccel = Vector3.zero;
        Vector3 rawGyro = Vector3.zero;

        // Use InputActionReferences if assigned in inspector
        if (accelAction != null && accelAction.action != null && accelAction.action.enabled)
        {
            rawAccel = accelAction.action.ReadValue<Vector3>();
        }

        if (gyroAction != null && gyroAction.action != null && gyroAction.action.enabled)
        {
            rawGyro = gyroAction.action.ReadValue<Vector3>();
        }

        // Othwerwise, try reading from device controls directly
        if (rawAccel == Vector3.zero || rawGyro == Vector3.zero)
        {
            var device = FindDualSenseOrGamepad();

            if (device != null)
            {
                foreach (var control in device.allControls)
                {
                    var n = (control.name ?? string.Empty).ToLower();

                    if (rawAccel == Vector3.zero && (n.Contains("accel") || n.Contains("acceler") || n.Contains("acceleration")))
                    {
                        if (control is Vector3Control v3)
                            rawAccel = v3.ReadValue();
                    }

                    if (rawGyro == Vector3.zero && (n.Contains("gyro") || n.Contains("gyroscope") || n.Contains("rotation") || n.Contains("angular")))
                    {
                        if (control is Vector3Control v3)
                            rawGyro = v3.ReadValue();
                    }
                }
            }
        }
        if (rawAccel == Vector3.zero)
        {
            var acc = InputSystem.GetDevice<UnityEngine.InputSystem.Accelerometer>();
            if (acc != null)
            {
                if (acc.TryGetChildControl<Vector3Control>("acceleration") is Vector3Control v)
                    rawAccel = v.ReadValue();
            }
        }

        if (rawGyro == Vector3.zero)
        {
            var gyr = InputSystem.GetDevice<UnityEngine.InputSystem.Gyroscope>();
            if (gyr != null)
            {
                if (gyr.TryGetChildControl<Vector3Control>("angularVelocity") is Vector3Control v)
                    rawGyro = v.ReadValue();
            }
        }

        //Apply a simple exponential smoothing
        float lerpT = 1 - Mathf.Exp(-smoothing * Time.deltaTime);
        smoothedAccel = Vector3.Lerp(smoothedAccel, rawAccel, lerpT);
        smoothedGyro = Vector3.Lerp(smoothedGyro, rawGyro, lerpT);

        //Deadzone to avoid tiny drift
        if (smoothedAccel.magnitude < deadzone) smoothedAccel = Vector3.zero;
        if (smoothedGyro.magnitude < deadzone) smoothedGyro = Vector3.zero;

        //Move based on sensors
        Vector3 accelMove = new Vector3(smoothedAccel.x, 0f, smoothedAccel.y) * accelSensitivity;
        transform.Translate(accelMove * Time.deltaTime, Space.World);

        Vector3 rot = smoothedGyro * gyroSensitivity;
        gyroPitch += smoothedGyro.x * gyroSensitivity * Time.deltaTime;
        gyroPitch = Mathf.Clamp(gyroPitch, -gyroTiltClamp, gyroTiltClamp);

        //If controller is tilted intentionally (beyond threshold) we want tilt to move the player
        if (Mathf.Abs(gyroPitch) < gyroTiltThreshold)
        {
            transform.Rotate(rot * Time.deltaTime, Space.Self);
        }

        //If tilt exceeds threshold, convert tilt to movement (down -> forward, up -> up)
        if (Mathf.Abs(gyroPitch) >= gyroTiltThreshold)
        {
            float norm = Mathf.Clamp(gyroPitch / gyroTiltClamp, -1f, 1f);

            if (norm > 0f)
            {
                //Tilting down -> move ahead
                Vector3 forwardMove = transform.forward * (norm * gyroTiltMoveSpeed) * Time.deltaTime;
                //Use CharacterController for movement so collisions are respected
                if (characterController != null)
                    characterController.Move(forwardMove);
                else
                    transform.Translate(forwardMove, Space.World);
                animator.SetBool("Walking", true);
            }
            else if (norm < 0f)
            {
                //Tilting up -> move up
                Vector3 upMove = Vector3.up * (-norm * gyroTiltMoveSpeed) * Time.deltaTime;
                if (characterController != null)
                    characterController.Move(upMove);
                else
                    transform.Translate(upMove, Space.World);
            }
        }
    }

    //Try to find a DualSense (PS5) device. Fall back to any connected Gamepad.
    InputDevice FindDualSenseOrGamepad()
    {
        foreach (var d in InputSystem.devices)
        {
            var prod = (d.description.product ?? string.Empty).ToLower();
            var name = (d.name ?? string.Empty).ToLower();

            if (prod.Contains("dualsense") || name.Contains("dualsense") || prod.Contains("HID") && prod.Contains("sense"))
                return d;

            if (prod.Contains("playstation") || prod.Contains("sony") || name.Contains("playstation") || name.Contains("sony"))
                return d;
        }

        return Gamepad.current;
    }

    private void FrwdDodgeBehavior()
    {   
        //Legacy fallback (keyboard) for dodge
        if (Input.GetKeyDown(KeyCode.G))
        {
            animator.Play("DodgeFrontAnimation");
            Vector3 dodgeDir = transform.forward * dodgeDist;
            characterController.Move(dodgeDir);
        }
    }

    private void AttackAnims()
    {
        //Legacy fallback (keyboard) for attacks
        if (Input.GetKeyDown(KeyCode.Alpha1)) FightBehavior(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) FightBehavior(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) FightBehavior(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) FightBehavior(3);
    }


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
        //Start short vibration to signal hit
        SetControllerVibration(hitVibrationLow, hitVibrationHigh);

        yield return new WaitForSeconds(hitVibrationDuration);

        //Play random hit sound from assigned Sound Effects
        if (soundEffectsData?.hitSounds != null && soundEffectsData.hitSounds.Length > 0)
        {
            var clips = soundEffectsData.hitSounds;
            var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        currHP -= hit;
        if (healthBarBehavior != null) healthBarBehavior.SetHealth(currHP);

        //Update lightbar color based on health
        if (currHP <= 40)
        {
            TrySetControllerLightColor(Color.red);
        }

        if (currHP <= 0) PlayerDeathBehavior();

        animator.Play("HitDamageAnimation");

        //Stop vibration after processing hit
        StopControllerVibration();
    }

    private void PlayerDeathBehavior() => Debug.Log("Player Died!!!");

    public void AttackEffect1() { if (HitEffect1 != null) HitEffect1.Play(); }
    public void AttackEffect2() { if (HitEffect2 != null) HitEffect2.Play(); }
    public void AttackEffect3() { if (HitEffect3 != null) HitEffect3.Play(); }
    public void AttackEffect4() { if (HitEffect4 != null) HitEffect4.Play(); }

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

    public void PerformAttackByIndex(int index)
    {
        FightBehavior(index);
    }

    public void PlayAnimationByName(string animationName)
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (!string.IsNullOrEmpty(animationName)) animator.Play(animationName);
    }

    public void TriggerAttackEffect(int effectIndex)
    {
        switch (effectIndex)
        {
            case 1: AttackEffect1(); break;
            case 2: AttackEffect2(); break;
            case 3: AttackEffect3(); break;
            case 4: AttackEffect4(); break;
            default: break;
        }
    }

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
