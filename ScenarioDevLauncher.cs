using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioDevLauncher : MonoBehaviour
{
    public bool IsActive = false;
    public int SelectedScenario = 0;

    private void Awake()
    {
        if (IsActive)
        {
            PlayerSessionData.IsScenariosActive = true;
            PlayerSessionData.SelectedScenario = SelectedScenario;
        }
    }
    private void OnDisable()
    {
        if (IsActive)
        {
            PlayerSessionData.IsScenariosActive = false;
            PlayerSessionData.SelectedScenario = -1;
        }
    }
}
