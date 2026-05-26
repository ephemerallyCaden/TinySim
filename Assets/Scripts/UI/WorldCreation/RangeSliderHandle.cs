using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RangeSliderHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RangeSlider rangeSlider;
    public bool isMinHandle = true;

    private ScrollRect parentScrollRect;

    private void Start()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMinHandle)
            rangeSlider.OnMinHandleDown();
        else
            rangeSlider.OnMaxHandleDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rangeSlider.OnHandleUp();
    }

    // Intercept drag events so the ScrollRect doesn't scroll while dragging a handle
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Block scroll rect from receiving this drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Consumed — RangeSlider.Update handles movement via Input.mousePosition
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        rangeSlider.OnHandleUp();
    }
}
