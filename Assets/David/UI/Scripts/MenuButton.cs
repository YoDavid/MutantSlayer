using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visuals")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;
    [SerializeField] private float fadeDuration = 0.1f;

    [Header("Scaling")]
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float normalScale = 0.95f;
    [SerializeField] private float scaleDuration = 0.15f;

    [Header("References")]
    public Button button;

    private bool _isSelected = false;
    private Tweener _scaleTweener;
    private Tweener _colorTweener;
    private bool _mouseIsOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_mouseIsOver)
        {
            _mouseIsOver = true;
            AudioManager.Instance.PlayButtonHover(); 
        }

        Debug.Log("Pointer Entered");

        Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseIsOver = false;
        Deselect();
    }

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (targetImage == null) targetImage = GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        targetImage.color = normalColor;
    }

    private void Start()
    {
        transform.localScale = Vector3.one * normalScale;
    }

    public void Select(bool fromEventSystem = false)
    {
        if (_isSelected) return;

        _isSelected = true;

        _scaleTweener?.Kill();
        _colorTweener?.Kill();

        _colorTweener = targetImage.DOColor(highlightedColor, fadeDuration)
            .SetUpdate(true);

        _scaleTweener = transform.DOScale(Vector3.one * selectedScale, scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        if (!fromEventSystem)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void Deselect()
    {
        _isSelected = false;

        _scaleTweener?.Kill();
        _colorTweener?.Kill();

        _colorTweener = targetImage.DOColor(normalColor, fadeDuration)
            .SetUpdate(true);

        _scaleTweener = transform.DOScale(Vector3.one * normalScale, scaleDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true);
    }

    public void SetAlpha(float alpha)
    {
        if (targetImage != null)
        {
            _colorTweener?.Kill();
            _colorTweener = targetImage.DOFade(alpha, fadeDuration)
                .SetUpdate(true);
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

    private void OnDestroy()
    {
        _scaleTweener?.Kill();
        _colorTweener?.Kill();
    }
}
