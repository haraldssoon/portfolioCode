using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;
using UnityEngine.AI;
using System;
using Events;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private State[] stateList;
    [SerializeField] [Range(-1f, 1f)] protected float fieldOfView;    //calculated through dot-product (1 being 100% infront.) 0.8f is usally good.
    [SerializeField] [Range(1f, 20f)] protected float aggroRadius;
    [SerializeField] [Range(1f, 20f)] protected float attackRange;
    [SerializeField] [Range(1f, 20f)] protected float visionRange;
    [SerializeField] [Range(1f, 10f)] protected float walkPointRange;
    [SerializeField] private float standardDistanceToPlayer;
    [SerializeField] protected float walkingSpeed;
    [SerializeField] protected float runningSpeed;

    [SerializeField] protected float timeBetweenAttacks;

    [SerializeField] private int attackDamage;

    [SerializeField] protected LayerMask sightMask;
    [SerializeField] private Animator anim;

    [SerializeField] private float alertRange = 7;

    Health enemyHealth;

    private Vector3 startingPosition;
    private Vector3 currentGoalPosition;
    private Vector3 spawnPosition;
    private Transform player;
    private float gameTimer;
    private float attackTimeCounter;
    private float standinStillTimer;
    private bool isIdling;
    private bool alreadyIdled;
    private bool isDead;

    //------------------------------------------------------
    // ----------- GETTERS AND SETTERS ---------------------
    //------------------------------------------------------

    public Transform Player => player;
    public NavMeshAgent NavMeshAgent { get; private set; }
    public StateMachine stateMachine { get; set; }
    public Animator Anim => anim;

    public Vector3 DirectionToTarget => (player.position - transform.position).normalized;
    public Vector3 StartingPosition => startingPosition;
    public Vector3 SpawnPosition => spawnPosition;
    public Vector3 CurrentGoalPosition => currentGoalPosition;
    public Vector3 StandardDistance => Player.transform.position - (DirectionToTarget * standardDistanceToPlayer);

    public float AggroRadius => aggroRadius;
    public float AttackRange => attackRange;

    public float TimeBetweenAttacks => timeBetweenAttacks;
    public float FieldOfView => fieldOfView;
    public float GameTimer => gameTimer;
    public float VisionRange => visionRange;
    public float WalkingSpeed => walkingSpeed;
    public float RunningSpeed => runningSpeed;
    public float DistanceToPlayer => Vector3.Distance(transform.position, player.position);
    public float AttackTimeCounter { get => attackTimeCounter; set => attackTimeCounter = value; }
    public float StandingStillTimer { get => standinStillTimer; set => standinStillTimer = value; }
    public bool IsIdling { get => isIdling; set => isIdling = value; }
    public bool AlreadyIdled { get => alreadyIdled; set => alreadyIdled = value; }
    public bool IsDead { get => isDead; set => isDead = value; }
    public LayerMask SightMask => sightMask;

    //public int AttackDamage => attackDamage;
    public int EnemyHealth { get { return enemyHealth.GetHealth; } }
    public int MaxHealth => enemyHealth.GetMaxHealth;

    public float AlertRange { get => alertRange; }
    public int AttackDamage { get => attackDamage; set => attackDamage = value; }

    [Header("Sound")]
    FMOD.Studio.EventInstance idleSoundInstance;
    FMOD.Studio.EventInstance walkingSoundInstance;
    [FMODUnity.EventRef]
    [SerializeField] private string idleSound;
    [FMODUnity.EventRef]
    [SerializeField] public string walkSound;
    [FMODUnity.EventRef]
    [SerializeField] public string alertSound;
    [FMODUnity.EventRef]
    [SerializeField] private string biteSound;

    protected void PlayBiteSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(biteSound, transform.position);
    }

    public void StartWalkSound()
    {
        walkingSoundInstance = FMODUnity.RuntimeManager.CreateInstance(walkSound);
        walkingSoundInstance.start();
    }

    public void StopWalkSound()
    {
        walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        walkingSoundInstance.release();
    }

    public void StopIdleSound()
    {
        idleSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        idleSoundInstance.release();
    }
    private void OnDisable()
    {
        idleSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        idleSoundInstance.release();

        walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        walkingSoundInstance.release();

        EnemyDestroyedEvent e = new EnemyDestroyedEvent(GetComponent<GuidComponent>());
        e.FireEvent();
    }

    private void OnDestroy()
    {
        idleSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        idleSoundInstance.release();

        walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        walkingSoundInstance.release();

        EnemyDestroyedEvent e = new EnemyDestroyedEvent(GetComponent<GuidComponent>());
        e.FireEvent();
        //DamageIndicatorEvent.UnRegisterListener(ShowMyPosition);
    }

    private void Start()
    {
        player = GameObject.Find("Player").transform;
        NavMeshAgent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<Health>();
        stateMachine = new StateMachine(this, stateList);
        spawnPosition = transform.position;

        idleSoundInstance = FMODUnity.RuntimeManager.CreateInstance(idleSound);
        idleSoundInstance.start();

        walkingSoundInstance = FMODUnity.RuntimeManager.CreateInstance(walkSound);
    }

    private void Update()
    {
        stateMachine.Run();

        gameTimer += Time.deltaTime;

        idleSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        walkingSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
    }

    public void StopMoving()
    {
        ChangeTargetDestination(transform.position);
        MoveToPosition();
    }
    public void ChangeTargetDestination(Vector3 newDestination)
    {
        startingPosition = transform.position;
        currentGoalPosition = newDestination;
    }

    public virtual void MoveToPosition()
    {
        if (NavMeshAgent == null)
        {
            Debug.Log("The navmesh agent component is not attached to " + gameObject.name);
        }
        else
        {
            NavMesh.SamplePosition(currentGoalPosition, out NavMeshHit hit, 7f, NavMesh.AllAreas);
            NavMeshAgent.SetDestination(hit.position);

        }

        //Debug.DrawRay(transform.position, transform.forward * 50f, Color.green);
        //Debug.DrawRay(transform.position, currentGoalPosition - this.transform.position, Color.red, 0f);
        //Debug.DrawLine(this.gameObject.transform.position, currentGoalPosition, Color.cyan);
    }

    public virtual void FindNewRoamingPosition()
    {
        Vector3 randomPos = RandomizeNewRoamingPosition();
        ChangeTargetDestination(randomPos);
    }

    public bool InLineOfSight()
    {
        Physics.Linecast(transform.position, player.position, out RaycastHit hit, sightMask);
        bool AISeePlayer = (hit.collider != null && hit.transform.gameObject.layer == player.gameObject.layer);

        if (aggroRadius > DistanceToPlayer && AISeePlayer)
            return true;

        if (DistanceToPlayer > visionRange) 
            return false; 
        
        if (fieldOfView > Vector3.Dot(transform.forward, DirectionToTarget))
            return false;

           return AISeePlayer;
    }

    public bool ReachedPosition()
    {
        if (NavMeshAgent.pathPending)
        {
            if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
            {
                if (!NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 RandomizeNewRoamingPosition()
    {
        //return transform.position + GetRandomDir() * Random.Range(10f, 200f);
        return spawnPosition + GetRandomDir() * UnityEngine.Random.Range(-walkPointRange, walkPointRange);
    }


    public Vector3 GetRandomDir()
    {
        float x = UnityEngine.Random.Range(-1f, 1f);
        float y = 0f;
        float z = UnityEngine.Random.Range(-1f, 1f);

        return new Vector3(x, y, z).normalized;
    }

    public void ChangeMovementSpeed(float newMoveSpeed)
    {
        NavMeshAgent.speed = newMoveSpeed;
    }

    public void LookAtPlayer()
    {
        var lookPos = DirectionToTarget;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10);
    }

    public bool BoolRandomizer()
    {
        int x = UnityEngine.Random.Range(0, 3);
        if (x == 0)
        {
            return true;
        }
        return false;
    }


    public IEnumerator PlayIdleAnim()
    {
        //borde vi ladda in viktiga/specifika anims vid spawn för att ha lätt tillgång till dem senare? för t.ex. timers?
        isIdling = true;
        alreadyIdled = true;
        anim.SetBool("IsIdling", true);
        //anim.SetBool("IsBiting", true);

        float animLength = 0;
        AnimationClip[] animClips = anim.runtimeAnimatorController.animationClips;
        foreach(AnimationClip a in animClips)
        {
            if (a.name.Contains("Idle"))
            {
                animLength = a.length;
            }
        }

        yield return new WaitForSeconds(animLength);
        isIdling = false;
        anim.SetBool("IsIdling", false);
        //anim.SetBool("IsBiting", false);
    }

    public void ChasePlayer()
    {
        ChangeTargetDestination(StandardDistance);
        MoveToPosition();
    }

    public IEnumerator PlayAnim(string animationName, int extraTime)
    {
        anim.SetBool(animationName, true);
        float animLength = 0;
        AnimationClip[] animClips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip a in animClips)
        {
            if (a.name.Contains(animationName))
            {
                animLength = a.length;
            }
        }

        //2 är temporär för att den inte ska försvinna direkt.
        yield return new WaitForSeconds(animLength+extraTime);
        anim.SetBool(animationName, false);
        Destroy(gameObject);
    }
}
