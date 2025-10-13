using System;
using UnityEngine;

public class FightingController : MonoBehaviour
{
    [Header ("Movement Settings")]
    public float PlayerSpeed = 1.0f;
    public float PlayerRotation = 10.0f;
    private Animator animator;
    private CharacterController characterController;

    [Header("Fight Settings")]
    public float attackDelay = 0.5f;
    public float dodgeDist = 5f;
    public int hitDamage = 5;
    public string[] FightAnimations = { "Attack1Animation", "Attack2Animation", "Attack3Animation", "Attack4Animation" };
    private float TimeOfLastAttack;

    [Header("Sounds & Effects Settings")]

    public ParticleSystem HitEffect1;
    public ParticleSystem HitEffect2;
    public ParticleSystem HitEffect3;
    public ParticleSystem HitEffect4;


    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }


    void PlayerMovement()
    {
        //These 2 variables retrieves the axis input from the user
        float Input_Horizontal = Input.GetAxis("Horizontal");
        float Input_Vertical = Input.GetAxis("Vertical");

        Vector3 Movement = new Vector3(-Input_Horizontal, 0f, Input_Vertical);

        if (Movement != Vector3.zero)
        {
            //Smoothly allows player to rotate facing the direction of the opponent
            Quaternion Rotation = Quaternion.LookRotation(Movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, Rotation, PlayerRotation * Time.deltaTime);

            //If player is moving forward
            if (Input_Horizontal > 0)
            {
                animator.SetBool("Walking", true);
            }

            //If player is moving backward
            else if (Input_Horizontal < 0)
            {
                animator.SetBool("Walking", true);
            }

            else if (Input_Vertical != 0)
            {
                animator.SetBool("Walking", true);
            }
        }

        else
        {
            animator.SetBool("Walking", false);
        }

        characterController.Move(Movement * PlayerSpeed * Time.deltaTime);

    }

    void FightBehavior(int AttackIndx)
    {
        //Checking if the cool down is done or not
        if (Time.time - TimeOfLastAttack > attackDelay)
        {
            animator.Play(FightAnimations[AttackIndx]);
            int Damage = hitDamage;

            Debug.Log("Performed an attack " + (AttackIndx + 1) + " dealing " + Damage + " damage");
            TimeOfLastAttack = Time.time;
        }

        else
        {
            Debug.Log("Cool down, cannot perform any attack");
        }
    }

    void AttackAnims()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FightBehavior(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FightBehavior(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FightBehavior(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            FightBehavior(3);
        }
    }

    void FrwdDodgeBehavior()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            animator.Play("DodgeFrontAnimation");
            Vector3 DodgeDirection = transform.forward * dodgeDist; //Calculation of the direction
            characterController.Move(DodgeDirection); //Application of that calculation
        }
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

    void Update()
    {
        PlayerMovement();
        FrwdDodgeBehavior();
        AttackAnims();
        
    }


}
