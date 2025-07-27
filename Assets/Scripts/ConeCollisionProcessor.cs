using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeCollisionProcessor : MonoBehaviour
{
    [Header("Cone Scan Settings")]
    public MeshCollider coneTriggerCollider;
    public Transform coneTip;
    public float triggerEnableTime = 0.1f;

    [Header("Raycast Settings")]
    public float downRayDistance = 50f;
    public float maxScanDistanceMeters = 2.0f;
    public LayerMask groundLayerMask;
    public LayerMask detectionLayerMask;

    [Header("Radar Window Filtering (mm)")]
    public float RstartMM = 200f;
    public float RendMM = 4000f;

    [Header("Radar Settings")]
    public float dummyPeakStrengthDbsm = 20f;

    private HashSet<Collider> detectedColliders = new HashSet<Collider>();
    private List<RadarPeak> detectedPeaks = new List<RadarPeak>();

    private void Awake()
    {
        if (coneTriggerCollider != null)
            coneTriggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        detectedColliders.Add(other);
    }

    public void StartConeScan()
    {
        Debug.Log("▶▶▶ STARTING CONE SCAN ◀◀◀");
        StartCoroutine(EnableConeColliderTemporarily());
    }

    private IEnumerator EnableConeColliderTemporarily()
    {
        detectedColliders.Clear();
        detectedPeaks.Clear();

        coneTriggerCollider.enabled = true;
        Debug.Log("→ Cone trigger enabled.");

        yield return new WaitForSeconds(triggerEnableTime);

        coneTriggerCollider.enabled = false;
        Debug.Log("→ Cone trigger disabled.");

        ProcessCollisions();
    }

    private void ProcessCollisions()
    {
        Debug.Log($"→ Colliders detected: {detectedColliders.Count}");

        List<RadarPeak> tempPeaks = new List<RadarPeak>();

        foreach (var col in detectedColliders)
        {
            if (col == null) continue;

            string objName = col.name;
            int layer = col.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);
            Debug.Log($"→ Checking collider: {objName} (Layer: {layerName})");

            // Check layer mask
            if (((1 << layer) & detectionLayerMask) == 0)
            {
                Debug.Log($"    ⛔ SKIPPED: Layer not in detection mask.");
                continue;
            }

            Vector3 objectCenter = col.bounds.center;
            Debug.Log($"    → Object center: {objectCenter}");

            // Ground check
            if (Physics.Raycast(objectCenter, Vector3.down, out RaycastHit groundHit, downRayDistance, groundLayerMask))
            {
                GameObject groundObject = groundHit.collider.gameObject;
                Debug.Log($"    ✅ Ground hit: {groundObject.name} (Tag: {groundObject.tag})");

                if (groundObject.CompareTag("Trash") || groundObject.CompareTag("Bin"))
                {
                    // Raycast from cone tip
                    Vector3 dir = (objectCenter - coneTip.position).normalized;
                    Debug.Log($"    → Raycasting from cone tip towards object.");

                    if (Physics.Raycast(coneTip.position, dir, out RaycastHit beamHit, maxScanDistanceMeters, detectionLayerMask))
                    {
                        Debug.Log($"    → Raycast hit object: {beamHit.collider.name}, Distance: {beamHit.distance * 1000f:F1} mm");

                        if (beamHit.collider == col)
                        {
                            float distanceMM = beamHit.distance * 1000f;

                            if (distanceMM >= RstartMM && distanceMM <= RendMM)
                            {
                                tempPeaks.Add(new RadarPeak
                                {
                                    distanceMM = distanceMM,
                                    strengthDbsm = dummyPeakStrengthDbsm,
                                    objectName = col.name
                                });

                                Debug.DrawLine(coneTip.position, beamHit.point, Color.green, 2f);
                                Debug.Log($"    ✅ PEAK RECORDED: {col.name} @ {distanceMM:F1} mm");
                            }
                            else
                            {
                                Debug.Log($"    ⚠️ DISCARDED: {col.name} @ {distanceMM:F1} mm (Outside Rstart={RstartMM}–Rend={RendMM})");
                            }
                        }
                        else
                        {
                            Debug.Log($"    ⚠️ RAY OBSTRUCTED: {col.name} blocked by {beamHit.collider.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"    ⛔ Raycast missed {col.name}");
                    }
                }
                else
                {
                    Debug.Log($"    ⛔ Ground object not tagged Trash or Bin. Tag was: {groundObject.tag}");
                }
            }
            else
            {
                Debug.Log($"    ⛔ MID-AIR: {col.name} has no ground hit beneath it.");
            }
        }

        tempPeaks.Sort((a, b) => a.distanceMM.CompareTo(b.distanceMM));
        for (int i = 0; i < Mathf.Min(3, tempPeaks.Count); i++)
            detectedPeaks.Add(tempPeaks[i]);

        Debug.Log(BuildReadableRadarPayload());
    }

    private string BuildReadableRadarPayload()
    {
        int numPeaks = detectedPeaks.Count;

        string payload = $"▼▼▼ RADAR PAYLOAD ▼▼▼\n";
        payload += $"Radar Status: 1\n";
        payload += $"Number of Peaks: {numPeaks}\n";

        for (int i = 0; i < 3; i++)
        {
            if (i < detectedPeaks.Count)
            {
                var p = detectedPeaks[i];
                payload += $"Peak {i + 1}: Distance = {p.distanceMM:F1} mm, Strength = {p.strengthDbsm:F1}, Object = {p.objectName}\n";
            }
            else
            {
                payload += $"Peak {i + 1}: -\n";
            }
        }

        payload += $"▲▲▲ END OF PAYLOAD ▲▲▲";

        return payload;
    }

    private struct RadarPeak
    {
        public float distanceMM;
        public float strengthDbsm;
        public string objectName;
    }
}
