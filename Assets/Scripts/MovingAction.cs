using UnityEngine;

public class MovingAction
{
    public Cube movingCube { get; set; }
    public Rigidbody movingRigidbody { get; set; }
    public Cube targetCube { get; set; }
    public Rigidbody targetRigidbody { get; set; }
    public AttackResult result { get; set; }
}
