using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCCustomerState
{
    Entering,
    Ordering,
    WaitingForOrder,
    Leaving,
    Default
}

public class NPCCustomer : MonoBehaviour
{
    public AnimationCurve curve;
    public float speed = 5f;
    public float waitTime = 15f;
    public NPCCustomerState state = NPCCustomerState.Default;
    public bool isOrderCorrect = false;
    public Dictionary<string, bool> orderIngredients = new Dictionary<string, bool>()
    {
        { "Ceviche", false },
        { "Cancha", false },
        { "Camote", false },
        { "Choclo", false },
        { "CevichePota", false },
        { "Aji", false },
        { "Alga", false },
        { "Lechuga", false }
    };
    private Coroutine moveCoroutine, waitForOrderCoroutine;
    private NPCCustomerManager customerManager;
    private int occupiedPositionIndex = -1;
    [SerializeField] private Recipe recipe;

    void Awake()
    {
        recipe = GetComponent<Recipe>();
    }

    void Update()
    {
        if(state == NPCCustomerState.Ordering)
        {
            OrderFood();
            state = NPCCustomerState.WaitingForOrder;
        }
        if(state == NPCCustomerState.WaitingForOrder)
        {
            
        }
        if(state == NPCCustomerState.Leaving)
        {
            
        }
    }

    public void OrderFood()
    {
        /* string order = "Customer Order: ";
        foreach (var ingredient in new List<string>(orderIngredients.Keys))
        {
            if (orderIngredients[ingredient])
            {
                order += $"{ingredient}, ";
            }
        }
        Debug.Log(order); */
    }

    public void GoToOrder(Vector2 position)
    {
        state = NPCCustomerState.Entering;
        moveCoroutine =StartCoroutine(MoveNPC(position, speed));
    }

    public void SetOccupiedPosition(NPCCustomerManager manager, int positionIndex)
    {
        customerManager = manager;
        occupiedPositionIndex = positionIndex;
    }

    public void SetOrder(Order order)
    {
        foreach (var ingredient in new List<string>(orderIngredients.Keys))
        {
            bool isIngredientInOrder = order.requiredIngredients.Contains(ingredient);

            orderIngredients[ingredient] = isIngredientInOrder;
            recipe.ingredientElements[ingredient].enabled = isIngredientInOrder;
        }

        /* string orderString = $"[{gameObject.name}] Order: ";
        foreach (var ingredient in new List<string>(orderIngredients.Keys))
        {
            if (orderIngredients[ingredient])
            {
                orderString += $"{ingredient}, ";
            }
        }
        Debug.Log(orderString); */
    }

    IEnumerator WaitForOrder()
    {
        yield return new WaitForSeconds(waitTime);
        ReleaseOccupiedPosition();
        state = NPCCustomerState.Leaving;
        moveCoroutine = StartCoroutine(MoveNPC(new Vector2(-40f, transform.position.y), speed));
    }

    IEnumerator MoveNPC(Vector2 targetPosition, float speed)
    {
        float timeElapsed = 0f;

        while (timeElapsed < speed)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, curve.Evaluate(timeElapsed * Time.deltaTime / speed));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        if (state == NPCCustomerState.Entering)
        {
            state = NPCCustomerState.Ordering;
            waitForOrderCoroutine = StartCoroutine(WaitForOrder());
        }
    }

    public void ReceiveOrder(Plate plate)
    {
        isOrderCorrect = true;
        foreach (var ingredient in orderIngredients)
        {
            if (ingredient.Value && !plate.ingredientsOnPlateDict[ingredient.Key])
            {
                isOrderCorrect = false;
                //Debug.Log($"{ingredient.Key} is in the order but not on the plate");
                break;
            }
        }

        if (isOrderCorrect)
        {
            FoodMinigameManager.satisfiedCustomers++;
            FoodMinigameManager.Instance.UpdateSatisfiedCustomersText();
            //Debug.Log("ORDER IS CORRECT!");
        }else{
            //Debug.Log("ORDER IS INCORRECT!");
        }

        StopCoroutine(waitForOrderCoroutine);
        StopCoroutine(moveCoroutine);
        ReleaseOccupiedPosition();
        StartCoroutine(MoveNPC(new Vector2(-40f, transform.position.y), speed));
        recipe.ResetRecipe();
    }

    private void ReleaseOccupiedPosition()
    {
        if (customerManager == null || occupiedPositionIndex < 0)
        {
            return;
        }

        customerManager.ReleaseCustomerPosition(occupiedPositionIndex);
        occupiedPositionIndex = -1;
    }

}
