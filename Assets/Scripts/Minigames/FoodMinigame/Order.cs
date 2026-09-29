using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Order", menuName = "Scriptable Objects/Order")]
public class Order : ScriptableObject
{
    public List<string> requiredIngredients = new List<string>();
    
}
