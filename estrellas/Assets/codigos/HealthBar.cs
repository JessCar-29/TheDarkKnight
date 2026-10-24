
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private Image fillImage;

    void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
    }

    void LateUpdate()
    {
        if (target == null || fillImage == null)
            return;

        if (target.Max <= 0)
            return;

        fillImage.fillAmount =
            Mathf.Clamp01((float)target.Current / target.Max);
    }
}
