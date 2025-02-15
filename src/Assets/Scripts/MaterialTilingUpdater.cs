using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialTilingUpdater : MonoBehaviour
{
    private Renderer rend;
    [SerializeField] private Vector2 tilingMultiplier = new Vector2(1, 1);
    [SerializeField] private bool xx = true;

    private Vector3 lastScale;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        UpdateTiling();
        lastScale = transform.localScale;
    }

    private void Update()
    {
        if (transform.localScale != lastScale)
        {
            UpdateTiling();
            lastScale = transform.localScale;
        }
    }

    private void UpdateTiling()
    {
        Vector3 scale = transform.lossyScale;
        float scaleXMultiplier = tilingMultiplier.x;

        float scaleX = scaleXMultiplier * (xx ? scale.x : scale.z);
        rend.material.mainTextureScale = new Vector2(scaleX, scale.y * tilingMultiplier.y);
    }
}
