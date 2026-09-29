using UnityEngine;


[CreateAssetMenu(fileName = "IncidentData", menuName = "Scriptable Objects/Situation/IncidentData")]
public class IncidentData : SituationData {

    [Header("Incident Data")]
    [SerializeField] private bool isRepeating = false;
    public bool IsRepeating => isRepeating;
}
