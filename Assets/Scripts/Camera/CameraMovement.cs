using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float smoothness = 5f;
    [SerializeField] private SpriteRenderer background; // arrastra aquí tu imagen de fondo

    private Camera cam;
    private float currentVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Movement();
        ClampToBackground();
    }

        void Movement()
    {
        float input = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input += 1f;

        float targetVelocity = input * speed;
        currentVelocity = Mathf.Lerp(currentVelocity, targetVelocity, smoothness * Time.deltaTime);

        transform.position += Vector3.right * currentVelocity * Time.deltaTime;
    }

    void ClampToBackground()
    {
        if (background == null) return;

        float halfWidth = cam.orthographicSize * cam.aspect;

        float minX = background.bounds.min.x + halfWidth;
        float maxX = background.bounds.max.x - halfWidth;

        // Se centra la imagen
        if (minX > maxX)
        {
            minX = maxX = background.bounds.center.x;
        }

        Vector3 pos = transform.position;
        float clampedX = Mathf.Clamp(pos.x, minX, maxX);

        // Si llega al limite se frena 
        if (!Mathf.Approximately(clampedX, pos.x))
        {
            currentVelocity = 0f;
        }

        transform.position = new Vector3(clampedX, pos.y, pos.z);
    }
}