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
    [SerializeField] DialogueSequence initialSequence;
    int index = -1;

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
        CoreManager.I.DialogScreen.SetActive(true);
        PlayDialog(initialSequence);
    }

    void PlayDialog(DialogueSequence seq)
    {
        index = 0;
        CoreManager.I.Lines.Play(seq.lines[index], NextDialog);
    }

    void NextDialog()
    {
        index++;
        if (index >= initialSequence.lines.Length)
        {
            FinishDialog();
            return;
        }
        CoreManager.I.Lines.Play(initialSequence.lines[index], NextDialog);
    }

    void FinishDialog()
    {
        CoreManager.I.Lines.Stop();
        CoreManager.I.DialogScreen.SetActive(false);
        npcCustomerManager.StartSpawning();
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
