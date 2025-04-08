using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour {

    int jumpCount = 0; // Number of jumps performed
    float movement = 0; // Current horizontal input
    [SerializeField] float jumpSpeed = 5.0f; // Force applied when jumping
    [SerializeField] float movementSpeed = 0.1f; // Speed of horizontal movement

    void Update() {
        // Get horizontal input
        movement = Input.GetAxis("Horizontal");

        // Handle jump input, allow up to 2 jumps
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2) {
            jumpCount++;
            Jump();
        }
    }

    void FixedUpdate() {
        // Update running animation based on movement
        GetComponent<Animator>().SetBool("run", movement != 0);

        // Move right
        if (movement > 0) {
            Vector3 newPosition = transform.position;
            newPosition.x += movementSpeed;
            transform.position = newPosition;
            movement = 0;

            // Face right
            Vector3 spin = transform.localScale;
            spin.x = Mathf.Abs(spin.x);
            transform.localScale = spin;
        }

        // Move left
        if (movement < 0) {
            transform.Translate(Vector3.left * movementSpeed);
            movement = 0;

            // Face left
            Vector3 spin = transform.localScale;
            spin.x = -Mathf.Abs(spin.x);
            transform.localScale = spin;
        }
    }

    void Jump() {
        // Apply vertical force and trigger jump animation
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
        GetComponent<Animator>().SetBool("jump", true);
    }

    public void ResetJumpCount() {
        // Reset jump counter (called when landing)
        jumpCount = 0;
    }
}
