using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObstacleCollider : MonoBehaviour
{
    public bool wallTrigger { get; private set; } = false;

    private void OnTriggerStay(Collider other)
    {
        wallTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        wallTrigger = false;
    }
}
