using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    public Level2DialogueManager level2Manager;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            level2Manager.TriggerChildScene();
        }
    }
}