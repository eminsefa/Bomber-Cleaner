using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameButton : MonoBehaviour,IPointerClickHandler
{
    public event Action<ButtonTypeEnum> ButtonClicked;

    public enum ButtonTypeEnum
    {
        MenuButton,ReplayButton
    }

    [SerializeField] private ButtonTypeEnum buttonType;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        GetComponent<BoxCollider2D>().enabled = false;
        ButtonClicked?.Invoke(buttonType);
    }
    
}
