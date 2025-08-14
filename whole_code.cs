/*
THIS IS THE WHOLE CODE AS I REMMEBER. 
Again I have not worked on this in about 2 years so I honestly could not recreate or fix any of this without having to give it a really
really good look. 
*/

using System.Collections;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public bool CanMove { get; private set; } = true; // This is to get that the player is in control of the player
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool IsSliding => Input.GetKeyDown(slideKey) && IsSprinting;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool canSlide = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.C;
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
  
    [Header("Movment Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80.0f; // this is how many degrees so that we stop looking straight up
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80.0f; // this is for lowerLimit

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Sliding Parameters")]
    [SerializeField] private float lengthOfSlide;
    private bool sliding;
    private float slide_time;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;// Crouch height
    [SerializeField] private float standingHeight = 2f;// Stand height
    [SerializeField] private float timeToCrouch = 0.25f; // is crouching
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0); // Standing center point
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0.5f, 0); // Croucnhing center point
    private bool isCrouching;// Is in crouch animation
    private bool duringCrouchAnimation;// Time to crouch/stand

    // SLIDING PARAMETERS

    private Vector3 hitPointNormal;

    private bool isSliding
    {
        get
        {
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal; // This gets the angle value of the slope were standing at
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            } 
            else 
            {
                return false;
            }
        }
    }

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; // This is the speed thats linked to our characterController script
    private Vector2 currentInput; // This is what we are putting throught the keyboard

    private float rotationX = 0;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if(canJump)
                HandleJump();

            if(canCrouch)
                HandleCrouch();

            ApplyFinalMovement();
        }
    }
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y); // this means that were gonna add together everything with the current input with the direction
        moveDirection.y = moveDirectionY;
    } 
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }
    private void HandleJump()
    {
        if(ShouldJump)
            moveDirection.y = jumpForce;
    }
    private void HandleCrouch()
    {
        if(ShouldCrouch) // StartCoreRoutine is core code for crouching and thingy I think Idk 
            StartCoroutine(CrouchStand());
    }
    private void HandleSlide()
    {
        if(IsSliding)
        {
            sliding = true;
        }
    }
    private void ApplyFinalMovement()
    {
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if(willSlideOnSlopes && isSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);
    }
    private IEnumerator CrouchStand() //IEnumberator is so that we can treat these two things as one This whole code is for the change of Y's center
    {

        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
                characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
