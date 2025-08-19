using UnityEngine;
public class FinishVolume2D : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        GameStateManager.I.OnRoundEnd(0); // stub
    }
}
