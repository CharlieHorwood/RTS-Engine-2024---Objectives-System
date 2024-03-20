using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RTSEngine.Entities;

[Serializable]
public class Objective
{
    public int ObjectiveID = -1;
    public string ObjectiveTitle = "";
    public string ObjectiveDesc = "";

    public int ObjectivePoints = 0;

    public ObjectiveStatus ObjectiveStatus = ObjectiveStatus.none;
    public ObjectiveType ObjectiveType = ObjectiveType.none;
    public ObjectiveCondition ObjectiveCondition = ObjectiveCondition.none;


    public List<ObjectiveUnit> ObjectiveUnitsList = new();
    public List<ObjectiveBuilding> ObjectiveBuildingList = new();

    public int ObjectiveUnitCount = 0;
    public int ObjectiveBuildingCount = 0;

    public int currentUnitsCount = 0;
    public int currentBuildingsCount = 0;
    public List<string> unitsChecklist = new();
    public List<string> buildingsChecklist = new();
    public int currentResourcesCollected = 0;

    public float ObjectiveSurviveTime = 0;
    public float ObjectiveLocationTimer = 0;
    public int ObjectiveCollectAmount = 0;

    public Objective()
    {
        
    }

}

[Serializable]
public class ObjectiveBuilding
{
    public Transform BuildingSpawnPosition = null;
    [HideInInspector] public Building ScenarioBuilding = null;
    public ObjectiveBuilding() { }
}

[Serializable]
public class ObjectiveUnit
{
    public Transform UnitSpawnPosition = null;
    [HideInInspector] public Building ScenarioUnit = null;
    public ObjectiveUnit() { }
}