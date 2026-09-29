using System.Collections.Generic;
using UnityEngine;

public class ObjectsPooler : MonoBehaviour
{
    [SerializeField] private GameObject objectToPool;
    [SerializeField] private int poolSize;
    private List<GameObject> pooledObjects =  new List<GameObject>();
    
    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectToPool);
            pooledObjects.Add(obj);
            obj.SetActive(false);
        }
    }

    public GameObject GetObjectFromPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!pooledObjects[i].activeSelf)
            {
                pooledObjects[i].SetActive(true);
                return pooledObjects[i];
            }
        }
        return null;
    }

    static public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
