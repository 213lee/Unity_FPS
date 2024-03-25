using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCollider : MonoBehaviour
{
    public bool IsGround { get; private set; } = true;

    public event System.Action OnLandEvent = null;

    public void Initialize(Player player)
    {
        OnLandEvent += player.Land;
    }
    private void OnTriggerEnter(Collider other)
    {
        OnLandEvent?.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        IsGround = true;
    }

    private void OnTriggerExit(Collider other)
    {
        IsGround = false;
    }
}
