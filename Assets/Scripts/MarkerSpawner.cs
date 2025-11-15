using UnityEngine;
using System.Collections.Generic;

public class MarkerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject oMarkerPrefab;
    [SerializeField] private GameObject xMarkerPrefab;

    [Header("O Marker Spawn Rate")]
    [SerializeField] private int oMarkersPerSpawn = 1;
    [SerializeField] private int oSpawnEveryNRounds = 1;

    [Header("X Marker Spawn Rate")]
    [SerializeField] private int xMarkersPerSpawn = 2;
    [SerializeField] private int xSpawnEveryNRounds = 1;

    private PaperController paperController;
    private List<Marker> activeMarkers = new List<Marker>();

    void Start()
    {
        paperController = FindFirstObjectByType<PaperController>();

        if (oMarkerPrefab == null)
        {
            Debug.LogWarning("O Marker Prefab not assigned! Creating default.");
            oMarkerPrefab = CreateDefaultMarker(Marker.MarkerType.O);
        }

        if (xMarkerPrefab == null)
        {
            Debug.LogWarning("X Marker Prefab not assigned! Creating default.");
            xMarkerPrefab = CreateDefaultMarker(Marker.MarkerType.X);
        }
    }

    GameObject CreateDefaultMarker(Marker.MarkerType type)
    {
        GameObject prefab = new GameObject(type == Marker.MarkerType.O ? "OMarker" : "XMarker");

        // Add sphere mesh
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(prefab.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * 0.5f;

        // Set color
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (type == Marker.MarkerType.O)
            renderer.material.color = Color.green;
        else
            renderer.material.color = Color.red;

        // Add marker component
        prefab.AddComponent<Marker>();

        return prefab;
    }

    public void SpawnMarkersForRound(int roundNumber)
    {
        // Spawn O markers
        if (roundNumber % oSpawnEveryNRounds == 0)
        {
            for (int i = 0; i < oMarkersPerSpawn; i++)
            {
                SpawnMarker(Marker.MarkerType.O, roundNumber);
            }
        }

        // Spawn X markers
        if (roundNumber % xSpawnEveryNRounds == 0)
        {
            for (int i = 0; i < xMarkersPerSpawn; i++)
            {
                SpawnMarker(Marker.MarkerType.X, roundNumber);
            }
        }
    }

    void SpawnMarker(Marker.MarkerType type, int roundNumber)
    {
        if (paperController == null)
        {
            Debug.LogError("PaperController not found!");
            return;
        }

        Vector3 position = paperController.GetRandomPositionOnPaper();
        GameObject prefab = type == Marker.MarkerType.O ? oMarkerPrefab : xMarkerPrefab;

        GameObject markerObj = Instantiate(prefab, position, Quaternion.identity);
        markerObj.transform.SetParent(paperController.transform);

        Marker marker = markerObj.GetComponent<Marker>();
        if (marker == null)
            marker = markerObj.AddComponent<Marker>();

        marker.Initialize(type, roundNumber);
        activeMarkers.Add(marker);

        Debug.Log($"Spawned {type} marker at round {roundNumber}");
    }

    public List<Marker> GetActiveMarkers()
    {
        return new List<Marker>(activeMarkers);
    }

    public void RemoveMarker(Marker marker)
    {
        if (activeMarkers.Contains(marker))
        {
            activeMarkers.Remove(marker);
            Destroy(marker.gameObject);
        }
    }
}
