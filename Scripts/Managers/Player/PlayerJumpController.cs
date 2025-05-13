using UnityEngine;
using Unity.Netcode;

public class PlayerJumpController : NetworkBehaviour
{
    private GameObject player;
    private Animator playerAnimator;
    private PlayerAnimation playerAnimation;

    private void Awake()
    {
        // Find the Player object and get its Animator and PlayerAnimation components
        player = GameObject.Find("Player");

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerAnimation = player.GetComponent<PlayerAnimation>();
        }
        else
        {
            Debug.LogError("Player not found in the scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Reset jump count and animation when touching the ground
        if (collision.CompareTag("Ground") && player != null)
        {
            playerAnimation?.ResetJumpCount();
            playerAnimator?.SetBool("jump", false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Set jump animation when leaving the ground
        if (collision.CompareTag("Ground") && playerAnimator != null)
        {
            playerAnimator.SetBool("jump", true);
        }
    }
}
