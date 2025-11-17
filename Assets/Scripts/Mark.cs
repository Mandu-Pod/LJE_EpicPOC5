using UnityEngine;

public enum MarkType
{
    O,
    X
}

public class Mark : MonoBehaviour
{
    public MarkType Type { get; private set; }
    
    public void Initialize(MarkType type)
    {
        Type = type;
    }
    
    public Vector2 GetPosition()
    {
        return transform.position;
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}