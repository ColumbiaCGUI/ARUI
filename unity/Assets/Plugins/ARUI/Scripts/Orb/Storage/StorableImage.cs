using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StorableImage : MonoBehaviour
{
    private Image _image;

    private Sprite _previewSprite;
    private Image _previewImage;

    private Vector3 _originalScale;
    private Vector3 _boxScale;

    private bool _isLookedAt = false;
    private Color CurrentTargetColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private Color _targetLookingColor = new Color(1f, 1f, 1f, 0.9f);
    private Color _targetNotLookingColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public void Initialize(Image original, Vector3 boxscale, Vector3 originalScale)
    {
        _image = original;

        transform.localScale = boxscale;
        _originalScale = originalScale;
        _boxScale = boxscale;

        _previewSprite = Resources.Load<Sprite>(StringResources.storage_preview_path);

        gameObject.AddComponent<Canvas>();
        _previewImage = gameObject.AddComponent<Image>();
        _previewImage.sprite = _previewSprite;

        _previewImage.rectTransform.sizeDelta = original.rectTransform.sizeDelta;
    }

    public void SetPreviewMode(bool previewmode, OrbStorageBox CurrentStorage)
    {
        _previewImage.enabled = previewmode;
        _image.enabled = !previewmode;

        if (previewmode)
        {
            _previewImage.sprite = _previewSprite;

            if (CurrentStorage != null)
            {
                gameObject.transform.localScale = _boxScale;
            }
            else
            {
                gameObject.transform.localScale = _originalScale;
            }
        }
    }

    public void UpdateVisibility(bool isLookedAt, OrbStorageBox currentStorage)
    {
        _isLookedAt = isLookedAt;

        if (_isLookedAt && CurrentTargetColor==_targetNotLookingColor)
        {
            CurrentTargetColor = _targetLookingColor;
            StartCoroutine(ChangeSpriteColor(CurrentTargetColor,0.5f));
        } else if (!_isLookedAt && CurrentTargetColor == _targetLookingColor)
        {
            CurrentTargetColor = _targetNotLookingColor;
            StartCoroutine(ChangeSpriteColor(CurrentTargetColor, 0.5f));
        }
    }

    private IEnumerator ChangeSpriteColor(Color targetColor, float duration)
    {
        Color initialColor = _image.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            _image.color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _image.color = targetColor;
    }
}