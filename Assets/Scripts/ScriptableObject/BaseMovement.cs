using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Actions/Movement")]
public class BaseMovement : ScriptableObject
{
    public string moveName;
    public int moveID;
    public MovementType movementType;

    public float movementScore;

    public enum MovementType { goToTarget, runAway, standStill };
}
