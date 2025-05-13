using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    void Awake()
    {
        // Only one instance of the player.
        DontDestroyOnLoad(gameObject);
    }
}
