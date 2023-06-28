using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동과 점프 동작을 담당하는 컴포넌트
/// 충돌처리에 대한 기능도 포함
/// 기본 동작으로써 작동
/// </summary>
public class MoveBehaviour : GenericBehaviour
{
    // 움직임
    public float walkSpeed = 0.15f;
    public float runSpeed = 1.0f;
    public float sprintSpeed = 2.0f;
    public float speedDampTIme = 0.1f;

    // 점프
    public float jumpHeight = 1.5f;
    public float jumpInertialForce = 10f; // 점프관성
    public float speed, speedSeeker;
    private int isJump;
    private int isgrounded;
    private bool isColliding;
    private bool jump;
    private CapsuleCollider capsuleCollider;
    private Transform myTransform;

    void Start()
    {
        myTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        isJump = Animator.StringToHash(FC.AnimatorKey.Jump);
        isgrounded = Animator.StringToHash(FC.AnimatorKey.Grounded);
        behaviourController.GetAnimator.SetBool(isgrounded, true);

        //
        behaviourController.SubScribeBehaviour(this);
        behaviourController.RegisterDefaultBehaviour(this.behaviourCode);
        speedSeeker = runSpeed;
    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);

        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
        Vector3 targetDirection = Vector3.zero;
        targetDirection = forward * vertical + right * horizontal;

        if (behaviourController.isMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Quaternion newRotation = Quaternion.Slerp(behaviourController.GetRigidBody.rotation,
                targetRotation, behaviourController.turnSmoothing);
            behaviourController.GetRigidBody.MoveRotation(newRotation);
            behaviourController.SetLastDirection(targetDirection);
        }

        if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            behaviourController.Repositioning();
        }

        return targetDirection;
    }

    void RemoveVerticalVelocity()
    {
        Vector3 horizontalVelocity = behaviourController.GetRigidBody.velocity;
        horizontalVelocity.y = 0.0f;
        behaviourController.GetRigidBody.velocity = horizontalVelocity;
    }

    void MovementManagement(float horizontal, float vertical)
    {
        if (behaviourController.isGrounded())
        {
            behaviourController.GetRigidBody.useGravity = true;
        }
        else if (!behaviourController.GetAnimator.GetBool(isJump) && behaviourController.GetRigidBody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }

        Rotating(horizontal, vertical);
        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;

        if(behaviourController.isSprinting())
        {
            speed = sprintSpeed;
        }

        behaviourController.GetAnimator.SetFloat(speedFloat, speed, speedDampTIme, Time.deltaTime);
    }

    void OnCollisionStay(Collision collision)
    {
        isColliding = true;

        if (behaviourController.isCurrentBehaviour(GetBehaviourCode) && collision.GetContact(0).normal.y <= 0.1f)
        {
            float vel = behaviourController.GetAnimator.velocity.magnitude;
            Vector3 targetMove = Vector3.ProjectOnPlane(myTransform.forward, collision.GetContact(0).normal).normalized * vel;

            behaviourController.GetRigidBody.AddForce(targetMove, ForceMode.VelocityChange);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    void JumpManagement()
    {
        if (jump && !behaviourController.GetAnimator.GetBool(isJump) && behaviourController.isGrounded())
        {
            behaviourController.LockTempBehaviour(behaviourCode);
            behaviourController.GetAnimator.SetBool(isJump, true);

            if( behaviourController.GetAnimator.GetFloat(speedFloat) > 0.1f)
            {
                capsuleCollider.material.dynamicFriction = 0.0f;
                capsuleCollider.material.staticFriction = 0.0f;
                RemoveVerticalVelocity();

                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourController.GetRigidBody.AddForce(Vector3.up, ForceMode.VelocityChange);
            }
        }
        else if (behaviourController.GetAnimator.GetBool(isJump))
        {
            if (behaviourController.isGrounded() && !isColliding && behaviourController.GetTempLockStatus())
            {
                behaviourController.GetRigidBody.AddForce(myTransform.forward * jumpInertialForce * Physics.gravity.magnitude *
                    sprintSpeed, ForceMode.Acceleration);
            }
            
            if(behaviourController.GetRigidBody.velocity.y < 0.0f && behaviourController.isGrounded())
            {
                behaviourController.GetAnimator.SetBool(isgrounded, true);
                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.6f;
                jump = false;
                behaviourController.GetAnimator.SetBool(isJump, false);
                behaviourController.UnLockTempBehaviour(behaviourCode);
            }
        }
    }

    void Update()
    {
        if (!jump && Input.GetButtonDown(ButtonName.Jump) && behaviourController.isCurrentBehaviour(behaviourCode) &&
            !behaviourController.isOverriding())
        {
            jump = true;
        }
    }

    public override void LocalFixedUpdate()
    {
        MovementManagement(behaviourController.GetH, behaviourController.GetV);
        JumpManagement();
    }
}
