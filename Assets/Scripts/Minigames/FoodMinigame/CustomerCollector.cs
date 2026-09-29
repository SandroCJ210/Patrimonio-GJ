using UnityEngine;

public class CustomerCollector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Customer"))
        {
            ObjectsPooler.ReturnObjectToPool(other.gameObject);
        }
    }
}
