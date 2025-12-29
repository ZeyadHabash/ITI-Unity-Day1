using UnityEngine;

public class FractalGenerator : MonoBehaviour
{
    [SerializeField] private GameObject fractalPrefab; // cube/sphere/whatever
    [Tooltip("This is limited to 10 bec more than that crashes the pc")][SerializeField, Range(0,10)] private int fractalSize;
    [SerializeField] private float scaleFactor = 0.5f; // each child is half the size

    void Start()
    {
        SpawnFractal(Vector3.zero, fractalSize, 1f);
    }

    private void SpawnFractal(Vector3 pos, int depth, float scale)
    {
        if (depth <= 0)
            return;

        GameObject fractal = Instantiate(fractalPrefab, pos, Quaternion.identity, transform);
        fractal.transform.localScale = Vector3.one * scale;

        float childScale = scale * scaleFactor;
        float offset = (scale / 2f) + (childScale / 2f);

        SpawnFractal(pos + Vector3.up * offset, depth - 1, childScale);
        SpawnFractal(pos + Vector3.right * offset, depth - 1, childScale);
        SpawnFractal(pos + Vector3.left * offset, depth - 1, childScale);
    }
}