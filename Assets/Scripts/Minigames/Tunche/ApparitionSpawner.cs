using System.Collections;
using UnityEngine;

public class ApparitionSpawner : MonoBehaviour
{
    [SerializeField] private SpriteRenderer apparition; 
    [SerializeField] private AudioClip sound;           // opcional (a espera de equipo de sonido)
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform[] points;        // puntos donde puede aparecer

    [Header("Tiempos")]
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 10f;
    [SerializeField] private float visibleTime = 1.5f;

    private Transform lastPoint;

    void Start()
    {
        apparition.gameObject.SetActive(false);
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // espera con el ser oculto
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (points.Length == 0) continue;

            // mover al nuevo punto y mostrarlo
            apparition.transform.position = PickPoint().position;
            apparition.gameObject.SetActive(true);

            if (sound != null && audioSource != null)
                audioSource.PlayOneShot(sound);

            //evita la aparicion de otro 
            yield return new WaitForSeconds(visibleTime);

            apparition.gameObject.SetActive(false);
        }
    }

    Transform PickPoint()
    {
        Transform point = points[Random.Range(0, points.Length)];

        // Evita repetir el mismo punto dos veces seguidas
        if (points.Length > 1)
        {
            int tries = 0;
            while (point == lastPoint && tries < 10)
            {
                point = points[Random.Range(0, points.Length)];
                tries++;
            }
        }

        lastPoint = point;
        return point;
    }

}