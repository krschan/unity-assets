using UnityEngine;
using Unity.Netcode;

public class PlayerAttackManager : NetworkBehaviour
{
    [SerializeField] private GameObject attackPrefab;

    private void Update()
    {
        // Trigger attack when the "L" key is pressed
        if (Input.GetKeyDown(KeyCode.L))
        {
            Instantiate(attackPrefab, transform.position, Quaternion.identity); // Spawn attack prefab
        }
    }
}
