using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float crouchSpeed = 1f;    // Speed while crouching
    [SerializeField] private float mouseSensitivity = 100f;

    private Rigidbody rb;
    private float currentMovementSpeed;
    private Vector3 moveDirection;

    private bool isCrouching = false;                   // To track if player is crouching

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentMovementSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;  // Make the cursor visible
    }

    private void Update()
    {
        RotatePlayer();
        MovementInput();
        CrouchInput();  // Handle crouching input
    }

    private void RotatePlayer()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * rotation);
    }

    private void MovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDirection != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.LeftShift)) // Run when holding LeftShift
            {
                Run();
            }
            else if (isCrouching) // If crouching, use crouch speed
            {
                CrouchWalk();
            }
            else
            {
                Walk();
            }

            // Move the player
            Vector3 move = transform.TransformDirection(moveDirection) * currentMovementSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    private void Run()
    {
        currentMovementSpeed = runSpeed;
    }

    private void Walk()
    {
        currentMovementSpeed = walkSpeed;
    }

    private void CrouchWalk()
    {
        currentMovementSpeed = crouchSpeed;
    }

    private void CrouchInput()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Toggle crouch when the C key is pressed
        {
            if (isCrouching)
            {
                StandUp();
            }
            else
            {
                Crouch();
            }
        }
    }

    private void Crouch()
    {
        isCrouching = true;
        currentMovementSpeed = crouchSpeed;     // Change speed to crouch speed
    }

    private void StandUp()
    {
        isCrouching = false;
        currentMovementSpeed = walkSpeed;        // Reset speed to walk speed
    }

    // You might want to implement a method to notify the enemy detection system
    public bool IsCrouching()
    {
        return isCrouching;
    }
}
