using System.Collections.Generic;
using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> ingredientSprites = new List<SpriteRenderer>();
    public Dictionary<string, SpriteRenderer> ingredientElements = new Dictionary<string, SpriteRenderer>();

    void Awake()
    {
        ingredientElements.Add("Cancha", ingredientSprites[0]);
        ingredientElements.Add("Camote", ingredientSprites[1]);
        ingredientElements.Add("Choclo", ingredientSprites[2]);
        ingredientElements.Add("Ceviche", ingredientSprites[3]);
        ingredientElements.Add("CevichePota", ingredientSprites[4]);
        ingredientElements.Add("Aji", ingredientSprites[5]);
        ingredientElements.Add("Alga", ingredientSprites[6]);
        ingredientElements.Add("Lechuga", ingredientSprites[7]);
    }

    public void ResetRecipe()
    {
        foreach (var ingredient in ingredientElements)
        {
            ingredient.Value.enabled = false;
        }
    }
}
