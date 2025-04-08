using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimation : MonoBehaviour
{
    public Transform[] patrolPoints; // Points the ghost moves between
    public float speed = 2.0f;       // Movement speed
    public float reachDistance = 0.5f; // Distance threshold to switch target

    private int currentPointIndex = 0; // Current target point index

    void Start()
    {
        // Exit if no patrol points are set
        if (patrolPoints.Length == 0)
        {
            return;
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;

        // Move toward the current patrol point
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);

        // Check if the ghost has reached the point
        if (Vector2.Distance(transform.position, targetPoint.position) < reachDistance)
        {
            // Switch to the next patrol point
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

            // Flip the sprite depending on direction
            if (currentPointIndex == 0)
            {
                transform.localScale = new Vector3(6, 6, 1);
            }
            else
            {
                transform.localScale = new Vector3(-6, 6, 1);
            }
        }
    }
}
