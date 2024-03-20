using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RTSEngine.Game;
using System;
using RTSEngine.Faction;
using RTSEngine.Entities;
using RTSEngine.NPC;

[Serializable]
public class Scenario
{
    public int scenarioIndex = 0;
    public int matchStartDelay = 60;
    public string scenarioName = "";
    public string scenarioDesc = "";
    public int maxPlayers = 2;
    public FactionTypeInfo playerFactionType = null;
    public FactionTypeInfo npcFactionType = null;
    public NPCType NPCDifficulty = null;
    public List<BasicNPCManager> NPCMgrs = new List<BasicNPCManager>();
    public List<ScenarioPlayerEntities> localPlayerEntities = new();
    public List<ScenarioBuildings> ScenarioBuildings = new ();
    public List<ScenarioUnits> ScenarioUnits = new();
    public ScenarioCivilians ScenarioCivilians;
    public Objectives Objectives = new();
    public Scenario(
        int scenarioIndex,
        int matchStartDelay,
        string scenarioName,
        string scenarioDesc,
        int maxPlayers,
        FactionTypeInfo playerFactionType,
        FactionTypeInfo npcFactionType,
        NPCType NPCDifficulty,
        List<BasicNPCManager> NPCMgrs,
        List<ScenarioPlayerEntities> localPlayerEntities,
        List<ScenarioBuildings> ScenarioBuildings,
        List<ScenarioUnits> ScenarioUnits,
        ScenarioCivilians ScenarioCivilians,
        Objectives Objectives
    )
    {
        this.scenarioIndex = scenarioIndex;
        this.matchStartDelay = matchStartDelay;
        this.scenarioName = scenarioName;
        this.scenarioDesc = scenarioDesc;
        this.maxPlayers = maxPlayers;
        this.playerFactionType = playerFactionType;
        this.npcFactionType = npcFactionType;
        this.NPCDifficulty = NPCDifficulty;
        this.NPCMgrs = NPCMgrs;
        this.localPlayerEntities = localPlayerEntities;
        this.ScenarioBuildings = ScenarioBuildings;
        this.ScenarioUnits = ScenarioUnits;
        this.ScenarioCivilians = ScenarioCivilians;
        this.Objectives = Objectives;
    }
}

[Serializable]
public class ScenarioPlayerEntities
{
    [SerializeField]
    public Entity entity = null;
    public Transform entityPosition = null;

    public ScenarioPlayerEntities(Entity entity, Transform entityPosition)
    {
        this.entity = entity;
        this.entityPosition = entityPosition;
    }
}

[Serializable]
public class ScenarioBuildings
{
    [SerializeField]
    public Building building = null;
    public Transform buildingPosition = null;

    public bool spawnUnits = false;
    public List<Unit> units = new();

    public ScenarioBuildings(Building building, Transform buildingPosition, bool spawnUnits, List<Unit> units)
    {
        this.building = building;
        this.buildingPosition = buildingPosition;
        this.spawnUnits = spawnUnits;
        this.units = units;
    }
}

[Serializable]
public class ScenarioUnits
{
    [SerializeField]
    public Unit unit = null;

    public Transform unitPosition = null;
    public ScenarioUnits(Unit unit, Transform unitPosition)
    {
        this.unit = unit;
        this.unitPosition = unitPosition;
    }
}

[Serializable]
public class ScenarioCivilians
{
    public List<ScenarioBuildings> CivilianBuildings = new();
    public List<ScenarioUnits> CivilainUnits = new();
    public ScenarioCivilians(List<ScenarioBuildings> CivilianBuildings, List<ScenarioUnits> CivilainUnits)
    {
        this.CivilianBuildings = CivilianBuildings;
        this.CivilainUnits = CivilainUnits;
    }
}