using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCCustomerManager : MonoBehaviour
{
    public static int activeCustomerCount = 0;
    [SerializeField] private List<Order> orders = new List<Order>();
    [SerializeField] private List<ObjectsPooler> objectsPoolers = new List<ObjectsPooler>();
    [SerializeField] private List<Transform> customerPositions = new List<Transform>();
    public List<bool> customerOccupiedPositions = new List<bool>(){false, false, false};
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 10f;
    

    void Start()
    {
        StartCoroutine(SpawnCustomers());
    }

    void Update()
    {
        
    }

    public void ReleaseCustomerPosition(int positionIndex)
    {
        if (positionIndex < 0 || positionIndex >= customerOccupiedPositions.Count)
        {
            return;
        }

        if (!customerOccupiedPositions[positionIndex])
        {
            return;
        }

        customerOccupiedPositions[positionIndex] = false;
        activeCustomerCount = Mathf.Max(0, activeCustomerCount - 1);
    }

    IEnumerator SpawnCustomers()
    {
        while (true)
        {
            Debug.Log("-----------SPAWNING CUSTOMERS----------");
            List<int> availablePositions = new List<int>();
            for (int positionIndex = 0; positionIndex < customerOccupiedPositions.Count; positionIndex++)
            {
                if (!customerOccupiedPositions[positionIndex])
                {
                    availablePositions.Add(positionIndex);
                }
            }

            if (availablePositions.Count == 0)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            int numberOfCustomersToSpawn = Random.Range(1, availablePositions.Count + 1);

            for(int i=0; i < numberOfCustomersToSpawn; i++)
            {
                int objectPoolerIndex = Random.Range(0, objectsPoolers.Count);
                int availablePositionIndex = Random.Range(0, availablePositions.Count);
                int customerPosition = availablePositions[availablePositionIndex];
                int orderIndex = Random.Range(0, orders.Count);
                availablePositions.RemoveAt(availablePositionIndex);

                GameObject customerObj = objectsPoolers[objectPoolerIndex].GetObjectFromPool();
                NPCCustomer npcCustomer = customerObj.GetComponent<NPCCustomer>();

                customerOccupiedPositions[customerPosition] = true;
                customerObj.transform.position = spawnPoint.position;
                npcCustomer.SetOrder(orders[orderIndex]);
                npcCustomer.SetOccupiedPosition(this, customerPosition);
                npcCustomer.GoToOrder((Vector2)customerPositions[customerPosition].position);
                activeCustomerCount++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
        
    }
}
