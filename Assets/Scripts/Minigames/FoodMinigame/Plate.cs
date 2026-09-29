using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System;

public class Plate : MonoBehaviour, IDraggable
{
    public bool isOverCustomer = false;
    public bool isOverPlatePositioner = false;
    private NPCCustomer customerReference;
    public Transform platePositionerTransform;
    private readonly List<Transform> platePositionersInContact = new List<Transform>();
    private Vector3 initialPosition;
    public Dictionary<string, bool> ingredientsOnPlateDict = new Dictionary<string, bool>()
    {
        { "Cancha", false },
        { "Camote", false },
        { "Choclo", false },
        { "Ceviche", false },
        { "CevichePota", false },
        { "Aji", false },
        { "Alga", false },
        { "Lechuga", false }
    };
    [SerializeField] private List<SpriteRenderer> ingredientSprites = new List<SpriteRenderer>();
    private Dictionary<string, SpriteRenderer> ingredientElements = new Dictionary<string, SpriteRenderer>();

    void Start()
    {
        initialPosition = transform.position;
        ingredientElements.Add("Cancha", ingredientSprites[0]);
        ingredientElements.Add("Camote", ingredientSprites[1]);
        ingredientElements.Add("Choclo", ingredientSprites[2]);
        ingredientElements.Add("Ceviche", ingredientSprites[3]);
        ingredientElements.Add("CevichePota", ingredientSprites[4]);
        ingredientElements.Add("Aji", ingredientSprites[5]);
        ingredientElements.Add("Alga", ingredientSprites[6]);
        ingredientElements.Add("Lechuga", ingredientSprites[7]);
    }

    void OnEnable()
    {
        platePositionersInContact.Clear();
        platePositionerTransform = null;
        isOverPlatePositioner = false;
    }

    private Transform CurrentPlatePositioner
    {
        get
        {
            Transform closest = null;
            float minDist = float.MaxValue;

            foreach (Transform positioner in platePositionersInContact)
            {
                float dist = ((Vector2)positioner.position - (Vector2)transform.position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = positioner;
                }
            }

            return closest;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("Begin Drag on Plate");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.WorldToScreenPoint(transform.position).z));
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(isOverCustomer && customerReference != null)
        {
            customerReference.ReceiveOrder(this);
            ResetPlate();
        }

        if (isOverPlatePositioner)
        {
            Transform currentPositioner = CurrentPlatePositioner;
            if (currentPositioner != null)
            {
                platePositionerTransform = currentPositioner;
                transform.position = currentPositioner.position;
                initialPosition = currentPositioner.position;
            }
            return;
        }

        transform.position = initialPosition;
        //ResetPlate();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ingredient"))
        {
            string ingredientName = other.GetComponent<Ingredient>().ingredientName;
            //Debug.Log($"{ingredientName} entered the plate");
        }

        if(other.CompareTag("Customer"))
        {
            customerReference = other.GetComponent<NPCCustomer>();
            isOverCustomer = true;
        }

        if (other.CompareTag("PlatePositioner"))
        {
            if (!platePositionersInContact.Contains(other.transform))
            {
                platePositionersInContact.Add(other.transform);
            }

            platePositionerTransform = CurrentPlatePositioner;
            isOverPlatePositioner = platePositionerTransform != null;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Customer"))
        {
            customerReference = null;
            isOverCustomer = false;
        }

        if (other.CompareTag("PlatePositioner"))
        {
            platePositionersInContact.Remove(other.transform);
            platePositionerTransform = CurrentPlatePositioner;
            isOverPlatePositioner = platePositionerTransform != null;
        }
    }

    public void AddIngredient(string ingredientName)
    {
        if (ingredientsOnPlateDict.ContainsKey(ingredientName))
        {
            ingredientsOnPlateDict[ingredientName] = true;
            //Debug.Log($"{ingredientName} added to the plate");
            ingredientElements[ingredientName].enabled = true;
            //PrintIngredientsOnPlate();
        }
    }

    public void PrintIngredientsOnPlate()
    {
        string ingredients = $"Ingredients on plate [{gameObject.name}]: ";
        foreach (var kvp in ingredientsOnPlateDict)
        {
            if (kvp.Value)
            {
                ingredients += kvp.Key + ", ";
            }
        }
        Debug.Log(ingredients);
    }

    public void ResetIngredientSprites()
    {
        foreach (var kvp in ingredientElements)
        {
            kvp.Value.enabled = false;
        }
    }

    public void ResetPlate()
    {
        foreach (var key in new List<string>(ingredientsOnPlateDict.Keys))
        {
            ingredientsOnPlateDict[key] = false;
        }
        ResetIngredientSprites();
        initialPosition = new Vector2(-40,-40);
        ObjectsPooler.ReturnObjectToPool(gameObject);
    }


}
