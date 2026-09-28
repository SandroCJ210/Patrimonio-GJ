using UnityEngine;

public class ApparitionSpawner : MonoBehaviour
{
    [SerializeField] Transform[] slots;

    public void PlaceRandomly(Transform target)
    {
        if (target == null || slots == null || slots.Length == 0) return;
        Transform slot = slots[Random.Range(0, slots.Length)];
        if (slot != null) target.position = slot.position;
    }
}
