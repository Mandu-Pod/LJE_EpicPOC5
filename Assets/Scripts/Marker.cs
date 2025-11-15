using UnityEngine;

public class Marker : MonoBehaviour
{
    public enum MarkerType { O, X }

    [SerializeField] private MarkerType markerType;
    [SerializeField] private bool isVisible = true;

    private int spawnRound;

    public MarkerType Type => markerType;
    public bool IsVisible => isVisible;
    public int SpawnRound => spawnRound;

    public void Initialize(MarkerType type, int round)
    {
        markerType = type;
        spawnRound = round;
        isVisible = true;
    }

    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        GetComponent<Renderer>().enabled = visible;
    }

    public void Hide()
    {
        SetVisibility(false);
    }
}
