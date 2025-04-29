using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // Add this namespace

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

    public void OnPointerEnter(PointerEventData eventData) => Select();
    public void OnPointerExit(PointerEventData eventData) => Deselect();

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

        // Kill any ongoing tweens to prevent conflicts
        _scaleTweener?.Kill();
        _colorTweener?.Kill();

        // Color change with DOTween
        _colorTweener = targetImage.DOColor(highlightedColor, fadeDuration)
            .SetUpdate(true); // This makes it ignore Time.timeScale

        // Scale animation with DOTween
        _scaleTweener = transform.DOScale(Vector3.one * selectedScale, scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Ignore Time.timeScale

        if (!fromEventSystem)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void Deselect()
    {
        _isSelected = false;

        // Kill any ongoing tweens
        _scaleTweener?.Kill();
        _colorTweener?.Kill();

        // Color change with DOTween
        _colorTweener = targetImage.DOColor(normalColor, fadeDuration)
            .SetUpdate(true);

        // Scale animation with DOTween
        _scaleTweener = transform.DOScale(Vector3.one * normalScale, scaleDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true);
    }

    public void SetAlpha(float alpha)
    {
        if (targetImage != null)
        {
            // Use DOTween for alpha changes too
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
        // Clean up tweens when the object is destroyed
        _scaleTweener?.Kill();
        _colorTweener?.Kill();
    }
}