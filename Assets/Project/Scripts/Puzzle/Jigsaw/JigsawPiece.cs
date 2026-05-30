// Coded by Fazar - Puzzle Level 1
using UnityEngine;

public class JigsawPiece : MonoBehaviour
{
    [Header("Target")]
    public Transform targetPoint;

    [Header("Settings")]
    public float snapDistance = 1.5f;

    [Header("State")]
    public bool isPlaced = false;
}