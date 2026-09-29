using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public ObjectsPooler objectsPooler;
    private bool isSelected = false;
    private GameObject selectedObject;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hoverEnterColor;
    [SerializeField] private Color hoverExitColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        spriteRenderer.color = hoverEnterColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.color = hoverExitColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelected) return;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSelected) return;
        isSelected = true;

        selectedObject = objectsPooler.GetObjectFromPool();
        if (selectedObject == null) return;

        ExecuteEvents.Execute<IBeginDragHandler>(selectedObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;
        ExecuteEvents.Execute<IEndDragHandler>(selectedObject, eventData, ExecuteEvents.endDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (selectedObject == null) return;

        ExecuteEvents.Execute<IDragHandler>(selectedObject, eventData, ExecuteEvents.dragHandler);
    }
}
