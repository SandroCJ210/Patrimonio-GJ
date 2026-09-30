using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FoodMinigameManager : MonoBehaviour
{
    public static FoodMinigameManager Instance;
    public static int satisfiedCustomers = 0;
    public int necesarySatisfiedCustomers = 6;
    public int minNumberOfRaids = 7;
    public TextMeshProUGUI satisfiedCustomersText;
    public AudioSource audioSource;
    public NPCCustomerManager npcCustomerManager;
    public GameObject victoryPanel;
    public GameObject failurePanel;

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

    void Update()
    {
        
    }

    public void UpdateSatisfiedCustomersText()
    {
        satisfiedCustomersText.text = $"{satisfiedCustomers} / {necesarySatisfiedCustomers}";

        if(satisfiedCustomers >= necesarySatisfiedCustomers)
        {
            audioSource.Stop();
            victoryPanel.SetActive(true);
            satisfiedCustomers = 0;
            StopCoroutine(npcCustomerManager.spawnCoroutine);
        }
    }

    public void GameOver()
    {        
        audioSource.Stop();
        failurePanel.SetActive(true);
        satisfiedCustomers = 0;
        StopCoroutine(npcCustomerManager.spawnCoroutine);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
