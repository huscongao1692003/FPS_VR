using UnityEngine;

public class VRJump : MonoBehaviour
{
    public float jumpForce = 5.0f; // Adjust this value for desired jump height
    public OVRInput.Button jumpButton = OVRInput.Button.Two; // "B" button on Oculus Touch

    private Rigidbody rb;
    private bool isGrounded = true; // Track if the player is on the ground

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (OVRInput.GetDown(jumpButton) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Detect when the player lands back on the ground
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Assuming you have tagged your ground objects
        {
            isGrounded = true;
        }
    }
}
