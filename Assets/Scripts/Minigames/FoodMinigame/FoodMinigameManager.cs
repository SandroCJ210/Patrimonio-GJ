using UnityEngine;
using TMPro;

public class FoodMinigameManager : MonoBehaviour
{
    public static FoodMinigameManager Instance;
    public static int satisfiedCustomers = 0;
    public int necesarySatisfiedCustomers = 6;
    public TextMeshProUGUI satisfiedCustomersText;
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        satisfiedCustomersText.text = $"0 / {necesarySatisfiedCustomers}";
    }

    public void UpdateSatisfiedCustomersText()
    {
        satisfiedCustomersText.text = $"{satisfiedCustomers} / {necesarySatisfiedCustomers}";

        if(satisfiedCustomers >= necesarySatisfiedCustomers)
        {
            audioSource.Stop();
            Debug.Log("YOUUUUUUUUUU WINNNNNNNNNNNNN!");
        }
    }
}
