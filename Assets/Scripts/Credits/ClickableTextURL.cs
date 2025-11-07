using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableTextURL : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private string urlToOpen = "https://www.google.com"; // Editable desde el inspector

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(urlToOpen);
    }
}
