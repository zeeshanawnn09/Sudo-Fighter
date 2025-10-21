using System.Collections;
using UnityEngine;

public class OpponentAIController : MonoBehaviour
{
    [Header ("Opponent Movement Settings")]
    public float PlayerSpeed = 1.0f;
    public float PlayerRotation = 10.0f;
    public Animator animator;
    public CharacterController characterController;

    [Header("Opponent Fight Settings")]
    public float attackDelay = 0.5f;
    public float dodgeDist = 5f;
    public float HitRadius = 2f;
    public int hitDamage = 5;
    public int AttackCount = 0;
    public int RandNumb;
    public bool isHit;
    public FightingController[] fightingControllers;
    public Transform[] characters;
    public string[] FightAnimations = { "Attack1Animation", "Attack2Animation", "Attack3Animation", "Attack4Animation" };
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
    public AudioClip[] HitSounds;

    void Awake()
    {
        currHP = maxHP;
        healthBarBehavior.OnStartHealth(currHP);
        RandomNumberGenerator();
    }

    void RandomNumberGenerator()
    {
        RandNumb = Random.Range(1, 5);
    }

    void FightBehavior(int AttackIndx)
    {

        animator.Play(FightAnimations[AttackIndx]);
        int Damage = hitDamage;

        Debug.Log("Performed an attack " + (AttackIndx + 1) + " dealing " + Damage + " damage");
        TimeOfLastAttack = Time.time;
        
    }

    void BkwrdDodgeBehavior()
    {
        animator.Play("DodgeBackAnimation");
        Vector3 DodgeDirection = -transform.forward * dodgeDist; //Calculation of the direction
        characterController.SimpleMove(DodgeDirection); //Application of that calculation
    }
    
    public IEnumerator OnHitAnim(int hit)
    {
        yield return new WaitForSeconds(0.3f);

        if (HitSounds != null && HitSounds.Length > 0)
        {
            int RandIndx = UnityEngine.Random.Range(0, HitSounds.Length);
            AudioSource.PlayClipAtPoint(HitSounds[RandIndx], transform.position);
        }

        currHP -= hit;
        healthBarBehavior.SetHealth(currHP);
        
        if (currHP <= 0)
        {
            PlayerDeathBehavior();
        }

        animator.Play("HitDamageAnimation");
    }

    void PlayerDeathBehavior()
    {
        Debug.Log("Opponent Died!!!");
    }
    
    public void AttackEffect1()
    {
        HitEffect1.Play();
    }

    public void AttackEffect2()
    {
        HitEffect2.Play();
    }

    public void AttackEffect3()
    {
        HitEffect3.Play();
    }

    public void AttackEffect4()
    {
        HitEffect4.Play();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < fightingControllers.Length; i++)
        {
            //When the opponent is within attack radius AND player is active
            if (characters[i].gameObject.activeSelf && Vector3.Distance(transform.position, characters[i].position) <= HitRadius)
            {
                animator.SetBool("Walking", false); //Stop walking animation

                //If cool down is done
                if (Time.time - TimeOfLastAttack > attackDelay)
                {
                    int RandAttack = Random.Range(0, FightAnimations.Length);

                    //Check if player is taking no damage
                    if (!isHit)
                    {
                        FightBehavior(RandAttack);
                    }

                    //When the player gets hit, need to implement damage
                    fightingControllers[i].StartCoroutine(fightingControllers[i].OnHitAnim(hitDamage));
                }
            }
            //Move towards the player to get into attacking radius
            else
            {
                //If Player is active or not
                if (characters[i].gameObject.activeSelf)
                {
                    //Movement Function
                    Vector3 direction = (characters[i].position - transform.position).normalized; //Calculates position from the player to the curr. pos of the opponent
                    characterController.Move(direction * PlayerSpeed * Time.deltaTime); // This will move the opponent towards player

                    //Rotation Function
                    Quaternion TargetRot = Quaternion.LookRotation(direction); //Face towards the player
                    transform.rotation = Quaternion.Slerp(transform.rotation, TargetRot, PlayerRotation * Time.deltaTime); //Interpolation

                    animator.SetBool("Walking", true);
                }
            }
        }
    }
}
