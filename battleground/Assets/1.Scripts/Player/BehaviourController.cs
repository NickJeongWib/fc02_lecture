using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ����, �⺻ ����, �������̵� ����, ��� ����, ���콺 �̵���,
/// ���� ���ִ���, GenericBehaviour�� ��ӹ��� ���۵��� ������Ʈ �����ش�.
/// </summary>
public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours; // ���۵�
    private List<GenericBehaviour> overrideBehaviours; // �켱 �� �Ǵ� ����
    private int currentBehaviour; // ���� ���� �ؽ��ڵ�
    private int defaultBehaviour; // �⺻ ���� �ؽ��ڵ�
    private int behaviourLocked; // ��� ���� �ؽ��ڵ�

    // ĳ��
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidBody;
    private ThirdPersonOrbitCamera camScript;
    private Transform myTransform;

    //
    private float h; // horizontal axis
    private float v; // vertical axis
    public float turnSmoothing = 0.06f; // ī�޶� ���ϵ��� ������ �� ȸ�� ����
    private bool changeFOV; // �޸��� ���� ī�޶� �þ߰��� ����Ǿ��� �� ����Ǿ���? 
    private float sprintFOV = 100; // �޸��� �þ߰�
    private Vector3 lastDirection; // ������ ���ߴ� ����
    private bool isSprint; // �޸��� ������?
    private int hFloat; // �ִϸ����� ���� ������ ��
    private int vFloat; // �ִϸ����� ���� ������ ��
    private int groundedBool; // �ִϸ����Ͱ� ���� �ִ°�?
    private Vector3 colExtents; // ������ �浹üũ�� ���� �浹ü ����

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

        // ���� Ÿ���� �ؽ��ڵ�� ������ �ִٰ� ���Ŀ� ���������� ����Ѵ�.
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
