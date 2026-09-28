
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float smoothness = 5f;

    [SerializeField] private SpriteRenderer background;

    [SerializeField] private PlayerStateController player;

    private Camera cam;
    private float currentVelocity;

    public void RecenterToFront()
    {
        currentVelocity = 0f;
        if (background == null) return;

        Vector3 position = transform.position;
        position.x = background.bounds.center.x;
        transform.position = position;
        ClampToBackground();
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
      
        if (player != null &&
            (player.CameraLocked || !player.InputEnabled))
        {
            currentVelocity = 0f;
            ClampToBackground();
            return;
        }

        Movement();
        ClampToBackground();
    }

    void Movement()
    {
        float input = 0f;

       
        var kb = Keyboard.current;

        if (kb == null)
        {
            currentVelocity = 0f;
            return;
        }

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
            input -= 1f;

        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
            input += 1f;

        float targetVelocity = input * speed;

       
        currentVelocity = Mathf.Lerp(
            currentVelocity,
            targetVelocity,
            smoothness * Time.deltaTime
        );

        transform.position +=
            Vector3.right * currentVelocity * Time.deltaTime;
    }

    void ClampToBackground()
    {
        if (background == null)
            return;

        float halfWidth = cam.orthographicSize * cam.aspect;

        float minX = background.bounds.min.x + halfWidth;
        float maxX = background.bounds.max.x - halfWidth;

        if (minX > maxX)
        {
            minX = maxX = background.bounds.center.x;
        }

        Vector3 pos = transform.position;

        float clampedX = Mathf.Clamp(
            pos.x,
            minX,
            maxX
        );

        if (!Mathf.Approximately(clampedX, pos.x))
        {
            currentVelocity = 0f;
        }

        transform.position = new Vector3(
            clampedX,
            pos.y,
            pos.z
        );
    }
}
