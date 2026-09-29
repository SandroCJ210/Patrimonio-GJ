using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IBeginDragHandler, IDragHandler, IEndDragHandler
{
}

public class Ingredient : MonoBehaviour, IDraggable
{
    public string ingredientName;
    private Vector3 initialPosition;
    private readonly List<Plate> platesInContact = new List<Plate>();
    public bool isOverPlate => platesInContact.Count > 0;
    private Plate CurrentPlate
    {
        get
        {
            Plate closest = null;
            float minDist = float.MaxValue;

            foreach (Plate plate in platesInContact)
            {
                float dist = ((Vector2)plate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = plate;
                }
            }
            return closest;
        }
    }

    public void OnEnable()
    {
        initialPosition = transform.position;
        platesInContact.Clear();
    }

    public void OnDisable()
    {
        transform.position = initialPosition;
        platesInContact.Clear();
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(
            eventData.position.x,
            eventData.position.y,
            Camera.main.WorldToScreenPoint(transform.position).z));

        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Plate target = CurrentPlate;

        if (target != null)
        {
            //Debug.Log($"{ingredientName} dropped on plate {target.name}");
            target.AddIngredient(ingredientName);
        }

        ResetIngredient();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Plate")) return;

        Plate plate = collision.GetComponent<Plate>();
        if (plate != null && !platesInContact.Contains(plate))
        {
            platesInContact.Add(plate);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Plate")) return;

        Plate plate = collision.GetComponent<Plate>();
        if (plate != null)
        {
            platesInContact.Remove(plate);
        }
    }

    void ResetIngredient()
    {
        platesInContact.Clear();
        ObjectsPooler.ReturnObjectToPool(gameObject);
    }
}