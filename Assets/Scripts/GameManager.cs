using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int startingEnemyHealth = 100;

    [Header("References")]
    [SerializeField] private PaperController paperController;
    [SerializeField] private MarkerSpawner markerSpawner;

    private int currentRound = 0;
    private int enemyHealth;
    private bool canFold = true;
    private int xMarkersHiddenThisRound = 0;

    void Start()
    {
        enemyHealth = startingEnemyHealth;

        if (paperController == null)
            paperController = FindFirstObjectByType<PaperController>();

        if (markerSpawner == null)
            markerSpawner = FindFirstObjectByType<MarkerSpawner>();

        StartNewRound();
    }

    void Update()
    {
        // Press Space to end round
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndRound();
        }
    }

    void StartNewRound()
    {
        currentRound++;
        canFold = true;
        xMarkersHiddenThisRound = 0;

        Debug.Log($"=== Round {currentRound} Started ===");
        Debug.Log($"Enemy Health: {enemyHealth}");

        // Spawn markers for this round
        if (markerSpawner != null)
        {
            markerSpawner.SpawnMarkersForRound(currentRound);
        }
    }

    void EndRound()
    {
        if (!canFold)
        {
            Debug.Log("Already folded this round!");
            return;
        }

        // Check for hidden X markers and calculate damage
        CheckMarkerVisibility();
        CalculateAndApplyDamage();

        canFold = false;

        // Check win condition
        if (enemyHealth <= 0)
        {
            Debug.Log("=== VICTORY! Enemy Defeated! ===");
            return;
        }

        // Start next round after a delay
        Invoke(nameof(StartNewRound), 1f);
    }

    void CheckMarkerVisibility()
    {
        if (markerSpawner == null) return;

        List<Marker> markers = markerSpawner.GetActiveMarkers();
        xMarkersHiddenThisRound = 0;

        foreach (Marker marker in markers)
        {
            // Simple visibility check - in a full implementation, 
            // this would check if the marker is covered by folded paper
            // For now, we'll use a simple raycast check
            bool isVisible = CheckMarkerVisibility(marker);

            if (!isVisible && marker.Type == Marker.MarkerType.X && marker.IsVisible)
            {
                // X marker was just hidden this round
                if (marker.SpawnRound <= currentRound)
                {
                    xMarkersHiddenThisRound++;
                    marker.Hide();
                    Debug.Log($"X marker hidden! Total hidden this round: {xMarkersHiddenThisRound}");
                }
            }
        }
    }

    bool CheckMarkerVisibility(Marker marker)
    {
        // Simple visibility check using renderer
        // In a more complex implementation, you'd check against folded geometry
        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer == null) return true;

        // For now, markers are visible unless explicitly hidden
        return marker.IsVisible;
    }

    void CalculateAndApplyDamage()
    {
        if (markerSpawner == null) return;

        List<Marker> markers = markerSpawner.GetActiveMarkers();

        // Count visible O markers
        int visibleOCount = markers.Count(m => m.Type == Marker.MarkerType.O && m.IsVisible);

        // Damage = visible O count × X markers hidden this round
        int damage = visibleOCount * xMarkersHiddenThisRound;

        if (damage > 0)
        {
            enemyHealth -= damage;
            Debug.Log($"=== DAMAGE DEALT ===");
            Debug.Log($"Visible O markers: {visibleOCount}");
            Debug.Log($"X markers hidden this round: {xMarkersHiddenThisRound}");
            Debug.Log($"Damage: {visibleOCount} × {xMarkersHiddenThisRound} = {damage}");
            Debug.Log($"Enemy Health: {enemyHealth}/{startingEnemyHealth}");
        }
        else
        {
            Debug.Log("No damage dealt this round.");
        }

        // Remove hidden X markers
        List<Marker> markersToRemove = markers.Where(m => m.Type == Marker.MarkerType.X && !m.IsVisible).ToList();
        foreach (Marker marker in markersToRemove)
        {
            markerSpawner.RemoveMarker(marker);
        }
    }

    public void OnPaperFolded()
    {
        if (!canFold)
        {
            Debug.Log("Cannot fold paper more than once per round!");
            return;
        }

        Debug.Log("Paper folded! Press Space to end round and calculate damage.");
    }
}
