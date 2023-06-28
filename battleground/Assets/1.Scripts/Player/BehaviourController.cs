using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 동작, 기본 동작, 오버라이딩 동작, 잠긴 동작, 마우스 이동값,
/// 땅에 서있는지, GenericBehaviour를 상속받은 동작들을 업데이트 시켜준다.
/// </summary>
public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours; // 동작들
    private List<GenericBehaviour> overrideBehaviours; // 우선 시 되는 동작
    private int currentBehaviour; // 현재 동작 해시코드
    private int defaultBehaviour; // 기본 동작 해시코드
    private int behaviourLocked; // 잠긴 동작 해시코드

    // 캐싱
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidBody;
    private ThirdPersonOrbitCamera camScript;
    private Transform myTransform;

    //
    private float h; // horizontal axis
    private float v; // vertical axis
    public float turnSmoothing = 0.06f; // 카메라를 향하도록 움직일 때 회전 감도
    private bool changeFOV; // 달리기 동작 카메라 시야각이 변경되었을 때 저장되었나? 
    private float sprintFOV = 100; // 달리기 시야각
    private Vector3 lastDirection; // 마지막 향했던 방향
    private bool isSprint; // 달리는 중인지?
    private int hFloat; // 애니메이터 관련 가로축 값
    private int vFloat; // 애니메이터 관련 세로축 값
    private int groundedBool; // 애니메이터가 지상에 있는가?
    private Vector3 colExtents; // 땅과의 충돌체크를 위한 충돌체 영역

    public float GetH { get => h; }
    public float GetV { get => v; }
    public ThirdPersonOrbitCamera GetCamScript { get => camScript; }
    public Rigidbody GetRigidBody { get => myRigidBody; }
    public Animator GetAnimator { get => myAnimator; }
    public int GetDefaultBehaviour { get => defaultBehaviour; }

    void Awake()
    {
        behaviours = new List<GenericBehaviour>();
        overrideBehaviours = new List<GenericBehaviour>();
        myAnimator = GetComponent<Animator>();
        hFloat = Animator.StringToHash(FC.AnimatorKey.Horizontal);
        vFloat = Animator.StringToHash(FC.AnimatorKey.Vertical);
        camScript = playerCamera.GetComponent<ThirdPersonOrbitCamera>();
        myRigidBody = GetComponent<Rigidbody>();
        myTransform = transform;

        // ground?
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    public bool isMoving()
    {
        // return (h != 0) || (v != 0); x
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    public bool isHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    public bool canSprint()
    {
        foreach (GenericBehaviour behaviour in behaviours)
        {
            if (!behaviour.AllowSprint)
            {
                return false;
            }
        }

        foreach(GenericBehaviour genericBehaviour in overrideBehaviours)
        {
            if (!genericBehaviour.AllowSprint)
            {
                return false;
            }
        }

        return true;
    }

    public bool isSprinting()
    {
        return isSprint && isMoving();
    }

    public bool isGrounded()
    {
        Ray ray = new Ray(myTransform.position + Vector3.up * 2 * colExtents.x, Vector3.down);

        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        myAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        myAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

        isSprint = Input.GetButton(ButtonName.Sprint);

        if (isSprinting())
        {
            changeFOV = true;
            camScript.SetFOV(sprintFOV);
        }
        else if (changeFOV)
        {
            camScript.ResetFOV();
            changeFOV = false;
        }

        myAnimator.SetBool(groundedBool, isGrounded());
    }

    public void Repositioning()
    {
        if (lastDirection != Vector3.zero)
        {
            lastDirection.y = 0.0f;
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            Quaternion newRotation = Quaternion.Slerp(myRigidBody.rotation, targetRotation, turnSmoothing);

            myRigidBody.MoveRotation(newRotation);
        }
    }

    void FixedUpdate()
    {
        bool isAnyBehaviourActive = false;
        if (behaviourLocked > 0 || overrideBehaviours.Count > 0)
        {
            foreach (GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    isAnyBehaviourActive = true;
                    behaviour.LocalFixedUpdate();
                }
            }
        }
        else
        {
            foreach (GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalFixedUpdate();
            }
        }

        if (!isAnyBehaviourActive && overrideBehaviours.Count == 0)
        {
            myRigidBody.useGravity = true;
            Repositioning();
        }
    }

    void LateUpdate()
    {
        if (behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach(GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    behaviour.LocalLateUpdate();
                }
            }
        }
        else
        {
            foreach (GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalLateUpdate();
            }
        }
    }

    public void SubScribeBehaviour(GenericBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void RegisterDefaultBehaviour(int behaviourCode)
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }

    public void RegisterBehaviour(int behaviourCode)
    {
        if (currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }

    public void UnRegisterBehaviour(int BehaviourCode)
    {
        if(currentBehaviour == BehaviourCode)
        {
            currentBehaviour = defaultBehaviour;
        }
    }

    public bool OverrideWithBehaviour(GenericBehaviour behaviour)
    {
        if (!overrideBehaviours.Contains(behaviour))
        {
            if (overrideBehaviours.Count == 0)
            {
                foreach(GenericBehaviour behaviour1 in behaviours)
                {
                    if (behaviour1.isActiveAndEnabled && currentBehaviour == behaviour1.GetBehaviourCode)
                    {
                        behaviour1.OnOverride();
                        break;
                    }
                }
            }
            overrideBehaviours.Add(behaviour);
            return true;
        }
        return false;
    }

    public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
    {
        if (overrideBehaviours.Contains(behaviour))
        {
            overrideBehaviours.Remove(behaviour);
            return true;
        }
        return false;
    }

    public bool isOverriding(GenericBehaviour behaviour = null)
    {
        if (behaviour == null)
        {
            return overrideBehaviours.Count > 0;
        }
        return overrideBehaviours.Contains(behaviour);
    }

    public bool isCurrentBehaviour(int behaviourCode)
    {
        return currentBehaviour == behaviourCode;
    }

    public bool GetTempLockStatus(int behaviourCode = 0)
    {
        return (behaviourLocked != 0 && behaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode)
    {
        if(behaviourLocked == 0)
        {
            behaviourLocked = behaviourCode;
        }
    }

    public void UnLockTempBehaviour(int behaviourCode)
    {
        if(behaviourLocked == behaviourCode)
        {
            behaviourLocked = 0;
        }
    }

    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }

    public void SetLastDirection(Vector3 direction)
    {
        lastDirection = direction;
    }
}

public abstract class GenericBehaviour : MonoBehaviour
{
    protected int speedFloat;
    protected BehaviourController behaviourController;
    protected int behaviourCode;
    protected bool canSprint;

    void Awake()
    {
        behaviourController = GetComponent<BehaviourController>();
        speedFloat = Animator.StringToHash(FC.AnimatorKey.Speed);

        // 동작 타입을 해시코드로 가지고 있다가 추후에 구별용으로 사용한다.
        behaviourCode = GetType().GetHashCode();
    }

    public int GetBehaviourCode
    {
        get => behaviourCode;
    }

    public bool AllowSprint
    {
        get => canSprint;
    }

    public virtual void LocalLateUpdate()
    {

    }

    public virtual void LocalFixedUpdate()
    {

    }

    public virtual void OnOverride()
    {

    }
}
