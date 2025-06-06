using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New InputData", menuName = "MyGame/InputData")]
public class InputData : ScriptableObject
{
    [SerializeField]private List<CellsControl.CellTypeAndCoordinates> InputPointsList;

    public void InitializeInputPointsList()
    {
        InputPointsList = new List<CellsControl.CellTypeAndCoordinates>();
    }

    public List<CellsControl.CellTypeAndCoordinates> GetInputPointsList()
    {
        return InputPointsList;
    }
}
