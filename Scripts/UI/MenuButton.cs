using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image image;

    void Start()
    {
        image.enabled = false;
        if(gameObject.TryGetComponent<Button>(out Button btn))
        {
            btn.onClick.AddListener(GameMgr.Instance.ButtonClick);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.enabled = false;
    }

    public void OnDisable()
    {
        image.enabled = false;
    }

}
