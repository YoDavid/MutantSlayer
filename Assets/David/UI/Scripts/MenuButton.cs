using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour,  ISelectHandler,  IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visuals")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;
    [SerializeField] private float fadeDuration = 0.1f;

    [Header("References")]
    public Button button;

    public void OnPointerEnter(PointerEventData eventData) => Select();
    public void OnPointerExit(PointerEventData eventData) => Deselect();

    private bool _isSelected = false;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (targetImage == null) targetImage = GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        targetImage.color = normalColor;
    }

    public void Select(bool fromEventSystem = false)
    {
        if (_isSelected) return;

        _isSelected = true;
        targetImage.color = highlightedColor;

        if (!fromEventSystem)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void Deselect()
    {
        _isSelected = false;
        targetImage.color = normalColor;
    }

    public void SetAlpha(float alpha)
    {
        if (targetImage != null)
        {
            Color c = targetImage.color;
            c.a = alpha;
            targetImage.color = c;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        Select(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Deselect();
    }
}