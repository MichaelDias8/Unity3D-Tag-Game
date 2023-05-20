using UnityEngine;
using Mirror;
using TMPro;
using static Models;

public class PlayerController : NetworkBehaviour
{
    public static GameManager gameManager;
    Rigidbody characterRigidBody;
    Animator characterAnimator;
    PlayerInputActions playerInputActions;
    //[HideInInspector]
    public Vector2 input_Movement;
    //[HideInInspector]
    public Vector2 input_View;

    Vector3 playerMovement;

    [Header("Settings")]
    public PlayerSettingsModel settings;

    [Header("Camera")]
    public Transform cameraTarget;
    public CameraController cameraController;

    [Header("Movement")]
    public float movementSpeedOffset = 1;
    public float movementSmoothdamp = 0.3f; 
    public bool isCrouching;
    public bool isWalking;
    public bool isSprinting;

    private float verticalSpeed;
    private float targetVerticalSpeed;
    private float verticalSpeedVelocity;

    private float horizontalSpeed;
    private float targetHorizontalSpeed;
    private float horizontalSpeedVelocity;

    private Vector3 relativePlayerVelocity;

    [Header("Inventory")]
    public PlayerInfoModel playerInfo;

    [Header("Gravity")]
    public float gravity = 10.4f;
    public LayerMask groundMask;
    private Vector3 gravityDirection;

    [Header("Jumping / Falling")]
    public float fallingSpeed;
    public float fallingSpeedPeak;
    public float fallingThreshold;
    public float fallingMovementSpeed;
    public float fallingRunningMovementSpeed;
    public float maxFallingMovementSpeed;
    private bool jumpingTriggered;
    private bool fallingTriggered;

    [Header("Landing")]
    private float landingTime;
    private bool landingTriggered;
    public float landingDuration = 0.2f;

    #region - Awake -
   
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        characterRigidBody = GetComponent<Rigidbody>();
        characterAnimator = GetComponent<Animator>();

        playerInputActions = new PlayerInputActions();

        playerInputActions.Movement.Movement.performed += x => input_Movement = x.ReadValue<Vector2>();
        playerInputActions.Movement.View.performed += x => input_View = x.ReadValue<Vector2>();
 
        playerInputActions.Actions.Jump.performed += x => Jump();

        playerInputActions.Actions.Walk.performed += x => ToggleWalking();
        playerInputActions.Actions.Sprint.performed += x => Sprint();

        playerInputActions.Actions.Tag.performed += x => Tag();

        playerInputActions.Actions.Crouch.performed += x => Crouch();

        gravityDirection = Vector3.down;
    }

    #endregion

    #region - Jumping -
  
    private void Jump()
    {
        if(!isOwned) return;
        transform.SetParent(null);
        
        if (jumpingTriggered)
        {
            return;
        }

        jumpingTriggered = true;

        if (IsMoving() && !isWalking)
        {
            characterAnimator.SetTrigger("RunningJump");
        }
        else
        {
            characterAnimator.SetTrigger("Jump");
        }
    }

    public void ApplyJumpForce()
    {
        if (!IsGrounded())
        {
            return;
        }
        characterRigidBody.AddForce(transform.up * settings.JumpingForce, ForceMode.Impulse);
        fallingTriggered = true;
    }

    #endregion

    #region - Sprinting -

    private void Sprint()
    {
        isSprinting = !isSprinting;
    }

    private void CalculateSprint()
    {
        if (isSprinting)
        {
            if (playerInfo.boost >= 1)
            {
                playerInfo.boost -= playerInfo.BoostDrain * Time.deltaTime;
            }
            else
            {  
                isSprinting = false;
            }
        }
        else
        {
            playerInfo.boost += playerInfo.BoostRegen * Time.deltaTime;
            playerInfo.boost = Mathf.Clamp(playerInfo.boost, 0, 100);
        
        }
        //update the boost bar
        gameManager.boostText.text = (playerInfo.boost.ToString("F1")) + "/100 BOOST";
    }

    #endregion

    #region - Gravity -

    private bool IsGrounded()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.2f, groundMask);
        
        int i = 0;
        while (i < hitColliders.Length)
        {
            if ((hitColliders[i].gameObject.GetComponent<BoxController>() != null))
                transform.SetParent(hitColliders[i].gameObject.transform);

            i++;
        }

        if(hitColliders.Length > 0)
        {
            return true;
        }
        return false;
    }

    private bool IsFalling()
    {
        if (fallingSpeed < fallingThreshold)
        {
            return true;
        }

        return false;
    }

    private bool IsLanding()
    {
        if(landingTriggered)
        {
            if (landingTime + landingDuration < Time.time)
            {
                landingTriggered = false;
                return false;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CalculateGravity()
    {
        Physics.gravity = gravityDirection * gravity;
    }

    private void CalculateFalling()
    {
        fallingSpeed = relativePlayerVelocity.y;

        if (fallingSpeed < fallingSpeedPeak && fallingSpeed < -0.1f && (fallingTriggered || jumpingTriggered))
        {
            fallingSpeedPeak = fallingSpeed;
        }

        if ((IsFalling() && !IsGrounded() && !jumpingTriggered && !fallingTriggered) || (jumpingTriggered && !fallingTriggered && !IsGrounded()))
        {
            fallingTriggered = true;
            characterAnimator.SetTrigger("Falling");
        }

        if (fallingTriggered && IsGrounded() && fallingSpeed < -0.1f)
        {
            fallingTriggered = false;
            jumpingTriggered = false;
            landingTime = Time.time;
            landingTriggered = true;

            if (fallingSpeedPeak < -25)
            {
                characterAnimator.SetTrigger("HardLand");
            }
            else
            {
                characterAnimator.SetTrigger("Land");
            }

            fallingSpeedPeak = 0;
        }
    }

    #endregion

    #  region - Movement -

    private void ToggleWalking()
    {
        isWalking = !isWalking;
    }

    public bool IsMoving()
    {
        if (relativePlayerVelocity.x > 0.4f || relativePlayerVelocity.x < -0.4f)
        {
            return true;
        }

        if (relativePlayerVelocity.z > 0.4f || relativePlayerVelocity.z < -0.4f)
        {
            return true;
        }

        return false;
    }

    public bool IsInputMoving()
    {
        if (input_Movement.x > 0.2f || input_Movement.x < -0.2f)
        {
            return true;
        }

        if (input_Movement.y > 0.2f || input_Movement.y < -0.2f)
        {
            return true;
        }

        return false;
    }

    private void Movement()
    {

        relativePlayerVelocity = transform.InverseTransformDirection(characterRigidBody.velocity);

        if (input_Movement.y > 0)
        {
            if(isSprinting)
                targetVerticalSpeed = settings.SprintingSpeed;
            else
                targetVerticalSpeed = ((isWalking || isCrouching) ? settings.WalkingSpeed : settings.RunningSpeed);
            
        }
        else
        {
            targetVerticalSpeed = ((isWalking || isCrouching) ? settings.WalkingBackwardSpeed : settings.RunningBackwardSpeed);

        }

        if (isSprinting)
            targetHorizontalSpeed = settings.SprintingStrafingSpeed;
        else
                targetHorizontalSpeed = ((isWalking || isCrouching) ? settings.WalkingStrafingSpeed : settings.RunningStrafingSpeed);
            


        targetVerticalSpeed = (targetVerticalSpeed * movementSpeedOffset) * input_Movement.y;
        targetHorizontalSpeed = (targetHorizontalSpeed * movementSpeedOffset) * input_Movement.x;

        verticalSpeed = Mathf.SmoothDamp(verticalSpeed, targetVerticalSpeed, ref verticalSpeedVelocity, movementSmoothdamp);
        horizontalSpeed = Mathf.SmoothDamp(horizontalSpeed, targetHorizontalSpeed, ref horizontalSpeedVelocity, movementSmoothdamp);

       
        characterAnimator.SetFloat("Vertical", verticalSpeed);
        characterAnimator.SetFloat("Horizontal", horizontalSpeed);
        
        if (IsInputMoving())
        {
            playerMovement = cameraController.transform.forward * verticalSpeed;
            playerMovement += cameraController.transform.right * horizontalSpeed;
            transform.SetParent(null);
        }
        else
        {
            playerMovement = Vector3.zero;
        }
       
        if (jumpingTriggered || IsFalling() || IsLanding())
        {
            
            characterAnimator.applyRootMotion = false;

            if(Vector3.Dot(characterRigidBody.velocity, playerMovement) < maxFallingMovementSpeed)
            {
                characterRigidBody.AddForce(playerMovement * (isWalking ? fallingMovementSpeed : fallingRunningMovementSpeed));
            }
        }
        else
        {
            characterAnimator.applyRootMotion = true;
        }
    }

    #endregion

    #region Player Tagging
    
    private void Tag()
    {
        if(!isOwned) return;
        gameManager.TryTag(this.gameObject);
        characterAnimator.SetTrigger("Tag");

    }

    #endregion

    private void Crouch()
    {
        if(!isOwned) return;
        isCrouching = !isCrouching;
        characterAnimator.SetBool("Crouching", isCrouching);
    }

    #region - Update -

    private void Update()
    {
        if(!isOwned) return;
        CalculateGravity();
        CalculateFalling();
        Movement();
        CalculateSprint();
    }

    #endregion

    #region - Enable/Disable -

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    #endregion

    #region - Gizmos -

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position, 0.2f);
    }

    #endregion

}
