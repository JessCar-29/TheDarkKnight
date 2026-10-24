
using System.Collections;
using UnityEngine;

public class CoinFlyAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinUI;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;

    [Header("Animacion")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float backStrength = 1.2f;
    [SerializeField] private float delayBetweenCoins = 0.08f;
    [SerializeField] private int coinCount = 1;

    [Header("Opcional")]
    [SerializeField] private bool useUIOverlay = true;

    public Transform PuntoRecoleccion;

    // Llama este metodo desde tu sistema de recoleccion.

    [ContextMenu("Probar Recoleccion")]
    public void ProbarRecoleccion()
    {
        PlayCoinAnimation(PuntoRecoleccion);
    }
    public void PlayCoinAnimation(Transform collectionPoint)
    {
        if (collectionPoint == null || coinPrefab == null ||
            coinUI == null)
        {
            Debug.LogWarning(
                "CoinFlyAnimation: faltan referencias.");
            return;
        }

        StartCoroutine(SpawnCoins(collectionPoint));
    }

    private IEnumerator SpawnCoins(Transform collectionPoint)
    {
        for (int i = 0; i < coinCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab);

            Vector3 startPosition = collectionPoint.position;
            Vector3 endPosition = GetTargetPosition();

            coin.transform.position = startPosition;

            StartCoroutine(AnimateCoin(
                coin, startPosition, endPosition));

            if (delayBetweenCoins > 0f)
                yield return new WaitForSeconds(delayBetweenCoins);
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (canvas != null &&
            canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    worldCamera, coinUI.position);

            RectTransform canvasRect =
                canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out Vector2 localPoint);

            return canvasRect.TransformPoint(localPoint);
        }

        return coinUI.position;
    }

    private IEnumerator AnimateCoin(
        GameObject coin,
        Vector3 start,
        Vector3 end)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (coin == null)
                yield break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Ease In Out Back
            float easedT = EaseInOutBack(t, backStrength);

            // Trayectoria en arco
            Vector3 position = Vector3.LerpUnclamped(
                start, end, easedT);

            position.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            coin.transform.position = position;

            // Escala con efecto de salida y llegada
            float scale = Mathf.Lerp(1f, 0.45f, t);
            coin.transform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        if (coin != null)
            Destroy(coin);

        // Conecta aquí tu contador si necesitas sumar
        // la moneda al llegar al destino.
    }

    private float EaseInOutBack(float t, float strength)
    {
        float c1 = strength;
        float c2 = c1 * 1.525f;

        if (t < 0.5f)
        {
            float x = 2f * t;
            return 0.5f * (
                x * x * ((c2 + 1f) * x - c2));
        }
        else
        {
            float x = 2f * t - 2f;
            return 0.5f * (
                x * x * ((c2 + 1f) * x + c2) + 2f);
        }
    }
}
