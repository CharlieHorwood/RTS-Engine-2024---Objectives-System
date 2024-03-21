using RTSEngine.BuildingExtension;
using RTSEngine.Faction;
using RTSEngine.Game;
using RTSEngine.Selection;
using RTSEngine.UnitExtension;
using RTSEngine.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;
using RTSEngine.Event;
using TMPro;
using System.Collections;
using RTSEngine.Cameras;


// MO: This should just inherit ScenePrepManager and extend it, alot of duplicate code
public class RTSEObjectiveSystem : MonoBehaviour, IPreRunGameService
{
    [HideInInspector]
    public int TabID = 0;
    public bool IsActive = false;

    protected IGameManager GameMgr { private set; get; }
    protected IGlobalEventPublisher GlobalEvents { private set; get; }
    public IBuildingManager buildingMgr { private set; get; }
    public IUnitManager unitMgr { private set; get; }
    protected IMainCameraController mainCameraController { private set; get; }
    public ISelectionManager selectionMgr { private set; get; }

    private List<IFactionSlot> updatedSlots = new();

    private int activeScenario = -1;
    public Transform ScenarioMarkersParent = null;
    public List<Scenario> Scenarios = new();

    public Scenario ActiveScenario;

    public GameObject ObjectiveStatusDisplay = null;
    public TextMeshProUGUI ObjectiveTitle = null;
    public TextMeshProUGUI ObjectiveDescription = null;
    public TextMeshProUGUI ObjectiveCurrentUpdate = null;
    public TextMeshProUGUI ObjectiveStatusText = null;

    private bool ObjectiveStatusOpen = false;
    private float timer = 0;

    public List<FactionSlot> defaultFactionSlots = new ();

    [Header("Scenario Introduction and information panel")]
    public GameObject ScenarioIntroPanel = null;
    public TextMeshProUGUI ScenarioIntroTitle = null;
    public TextMeshProUGUI ScenarioIntroDescription = null;
    public TextMeshProUGUI ScenarioPlayerFaction = null;
    public TextMeshProUGUI ScenarioNPCFaction = null;
    public RectTransform ScenarioPlayerEntities = null;
    public RectTransform ScenarrioObjectivesList = null;

    private bool HasPaused = false;

    public void Init(IGameManager GameMgr)
    {
        this.GameMgr = GameMgr;
        this.GlobalEvents = GameMgr.GetService<IGlobalEventPublisher>();
        this.buildingMgr = GameMgr.GetService<IBuildingManager>();
        this.unitMgr = GameMgr.GetService<IUnitManager>();
        this.mainCameraController = GameMgr.GetService<IMainCameraController>();
        //if (this.IsActive && CNPlayerSessionData.IsScenariosActive)
        //{
            this.GameMgr.GameStartRunning += PrepareScenarios;

            // Events for tracking Objectives
            this.GlobalEvents.BuildingBuiltGlobal += BuildingBuiltObjective;
            this.GlobalEvents.BuildingDeadGlobal += BuildingDestroyedObjective;

            this.GlobalEvents.UnitInitiatedGlobal += UnitTrainedObjective;
            this.GlobalEvents.UnitDeadGlobal += UnitDestroyedObjective;
            this.GameMgr.SetState(GameStateType.pause);
            if(this.ScenarioIntroPanel != null)
            {
                this.ScenarioIntroPanel.SetActive(true);
            }
        //}

    }

    public void Update()
    {
        if (ObjectiveStatusOpen)
        {
            timer += Time.deltaTime;
            if(timer > this.GameMgr.PeaceTimer.DefaultValue || this.GameMgr.InPeaceTime!)
            {
                ObjectiveStatusOpen = false;
                StartCoroutine(MoveUIPanel(this.ObjectiveStatusDisplay.transform.position,new Vector3(340, -20,0), this.ObjectiveStatusDisplay.transform,1, false));
            }
        }
    }

    public void PrepareScenarios(IGameManager gameMgr, EventArgs eventArgs)
    {
        if (PlayerSessionData.IsScenariosActive)
        {
            ActiveScenario = this.Scenarios.Find(CNS => CNS.scenarioIndex == PlayerSessionData.SelectedScenario);
            if (ActiveScenario != null)
            {
                // Lets try to ccopy over the static objectives list
                ActiveScenario.Objectives.OjectivesLsit = PlayerSessionData.Objectives;

                // Setup local player buildings and units first
                IFactionSlot PlayerSlot = this.GameMgr.GetFactionSlot(0);
                this.updatedSlots.Add(PlayerSlot);
                string factionCode = PlayerSlot.Data.type.Key.ToString();
                    // Pause game to allow scenario info to be displayed

                    if(this.ScenarioIntroPanel != null)
                    {
                        if(this.ScenarioIntroTitle != null)
                        {
                            this.ScenarioIntroTitle.text = ActiveScenario.scenarioName;
                        }
                        if(this.ScenarioIntroDescription != null)
                        {
                            this.ScenarioIntroDescription.text = ActiveScenario.scenarioDesc;
                        }
                        if (this.ScenarioPlayerEntities != null && ActiveScenario.localPlayerEntities.Count > 0)
                        {
                            foreach(var entity in ActiveScenario.localPlayerEntities)
                            {
                                GameObject playerEntity = Instantiate(new GameObject(), this.ScenarioPlayerEntities.transform);
                                playerEntity.AddComponent<TextMeshProUGUI>();
                                if(playerEntity.TryGetComponent(out TextMeshProUGUI txtmp))
                                {
                                    txtmp = this.ScenarioIntroDescription;
                                    txtmp.text = entity.entity.name;
                                }
                            }
                            
                        }
                        if(this.ScenarrioObjectivesList != null && ActiveScenario.Objectives.OjectivesLsit.Count > 0)
                        {
                            foreach (var objective in ActiveScenario.Objectives.OjectivesLsit)
                            {
                                GameObject scenarioObjective = Instantiate(new GameObject(), this.ScenarrioObjectivesList.transform);
                                scenarioObjective.AddComponent<TextMeshProUGUI>();
                                if (scenarioObjective.TryGetComponent(out TextMeshProUGUI txtmp))
                                {
                                    txtmp = this.ScenarioIntroDescription;
                                    txtmp.text = objective.ObjectiveTitle + " : " + objective.ObjectiveDesc;
                                }
                            }
                        }

                        this.ScenarioIntroPanel.SetActive(true);
                    }

                    this.GameMgr.PeaceTimer.Reload(ActiveScenario.matchStartDelay);

                    // Prepare screen for scenario info display
                    if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
                    {

                    }
                    
                    if(ActiveScenario.localPlayerEntities.Count > 0)
                    {
                        int index = -1;
                        foreach(var playerEntity in ActiveScenario.localPlayerEntities)
                        {
                            if (index == 0)
                            {
                                mainCameraController.PanningHandler.LookAt(playerEntity.entityPosition.position, smooth: false);
                            }

                            index++;
                            if(playerEntity.entity.Type == EntityType.unit)
                            {
                                IUnit playerUnit = (IUnit)unitMgr.CreateUnitLocal(
                                    (playerEntity.entity as IUnit),
                                    playerEntity.entityPosition.position,
                                    playerEntity.entityPosition.rotation,
                                    new InitUnitParameters
                                    {
                                        factionID = 0,
                                        playerCommand = false
                                    }
                                );
                                // do other stuffs here with fog of war or owt!!
                            }
                            else if(playerEntity.entity.Type == EntityType.building)
                            {
                                IBuilding scenarioBuilding = buildingMgr.CreatePlacedBuildingLocal(
                                    playerEntity.entity as IBuilding,
                                    playerEntity.entityPosition.position,
                                    playerEntity.entityPosition.rotation,
                                    new InitBuildingParameters
                                    {
                                        buildingCenter = (playerEntity.entity as IBuilding).BorderComponent,
                                        factionID = 0,
                                        isBuilt = true,
                                        setInitialHealth = true,
                                        initialHealth = playerEntity.entity.Health.MaxHealth,
                                        playerCommand = false
                                    }
                                );
                            }
                        }
                    }
                    
                    if (ActiveScenario.ScenarioBuildings.Count > 0)
                    {
                        foreach (var buildingPrefab in ActiveScenario.ScenarioBuildings)
                        {
                            IBuilding scenarioBuilding = buildingMgr.CreatePlacedBuildingLocal(
                                buildingPrefab.building as IBuilding,
                                buildingPrefab.buildingPosition.position,
                                buildingPrefab.buildingPosition.rotation,
                                new InitBuildingParameters
                                {
                                    buildingCenter = buildingPrefab.building.BorderComponent,
                                    factionID = 1,
                                    isBuilt = true,
                                    setInitialHealth = true,
                                    initialHealth = 6000,
                                    playerCommand = false
                                }
                            );
                        }
                    }

                    if(ActiveScenario.ScenarioUnits.Count > 0)
                    {
                        foreach (ScenarioUnits unit in ActiveScenario.ScenarioUnits)
                        {
                            if (unit != null)
                            {
                                IUnit spawner = (IUnit)unitMgr.CreateUnitLocal(
                                    unit.unit,
                                    unit.unitPosition.position,
                                    unit.unitPosition.rotation,
                                    new InitUnitParameters
                                    {
                                        factionID = 1,
                                        playerCommand = false
                                    }
                                );
                            }
                        }
                    }

            }
        }
    }

    // Objectives Event Methods
    public void BuildingBuiltObjective(IBuilding building,EventArgs eventArgs)
    {
        if(building.FactionID == this.GameMgr.LocalFactionSlotID && !building.CNBuilding.advPlcr.placedAtStart)
        {
            if(ActiveScenario.Objectives.OjectivesLsit.Count > 0)
            {
                foreach(Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                {
                    if(
                        objective.ObjectiveStatus == ObjectiveStatus.active && 
                        objective.ObjectiveType == ObjectiveType.create && 
                        objective.ObjectiveCondition == ObjectiveCondition.anyBuildings)
                    {
                        if(objective.ObjectiveBuildingCount > 0)
                        {
                            objective.currentBuildingsCount++;
                            objective.ObjectiveBuildingCount--;
                        }
                        else if(objective.ObjectiveBuildingCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.create &&
                        objective.ObjectiveCondition == ObjectiveCondition.buildingList)
                    {
                        if (objective.ObjectiveBuildingCount > 0)
                        {
                            objective.currentBuildingsCount++;
                            objective.ObjectiveBuildingCount--;
                        }
                        else if (objective.ObjectiveBuildingCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }
                }
            }
        }
    }
    public void BuildingDestroyedObjective(IBuilding building,EventArgs eventArgs)
    {
        if (building.FactionID != this.GameMgr.LocalFactionSlotID)
        {
            if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
            {
                foreach (Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                {
                    if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.destroy &&
                        objective.ObjectiveCondition == ObjectiveCondition.anyBuildings)
                    {
                        if (objective.ObjectiveBuildingCount > 0)
                        {
                            objective.currentBuildingsCount++;
                            objective.ObjectiveBuildingCount--;
                        }
                        else if (objective.ObjectiveBuildingCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle+" - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.destroy &&
                        objective.ObjectiveCondition == ObjectiveCondition.buildingList)
                    {
                        if (objective.ObjectiveBuildingCount > 0)
                        {
                            objective.currentBuildingsCount++;
                            objective.ObjectiveBuildingCount--;
                        }
                        else if (objective.ObjectiveBuildingCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }
                }
            }
        }
    }

    public void UnitTrainedObjective(IUnit unit, EventArgs eventArgs)
    {
        if (unit.FactionID == this.GameMgr.LocalFactionSlotID && !unit.CNUnit.spawnedAtStart)
        {
            if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
            {
                foreach (Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                {
                    if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.create &&
                        objective.ObjectiveCondition == ObjectiveCondition.anyUnits)
                    {
                        objective.currentUnitsCount++;
                        objective.ObjectiveUnitCount--;
                        if (objective.ObjectiveUnitCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.create &&
                        objective.ObjectiveCondition == ObjectiveCondition.unitList)
                    {
                        objective.currentUnitsCount++;
                        objective.ObjectiveUnitCount--;
                        if (objective.ObjectiveUnitCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }
                }
            }
        }
    }
    public void UnitDestroyedObjective(IUnit unit, EventArgs eventArgs)
    {
        if (unit.FactionID != this.GameMgr.LocalFactionSlotID)
        {
            if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
            {
                foreach (Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                {
                    if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.destroy &&
                        objective.ObjectiveCondition == ObjectiveCondition.anyUnits)
                    {
                        if (objective.ObjectiveUnitCount > 0)
                        {
                            objective.currentUnitsCount++;
                            objective.ObjectiveUnitCount--;
                        }
                        else if (objective.ObjectiveUnitCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.destroy &&
                        objective.ObjectiveCondition == ObjectiveCondition.unitList)
                    {
                        if (objective.ObjectiveUnitCount > 0)
                        {
                            objective.currentUnitsCount++;
                            objective.ObjectiveUnitCount--;
                        }
                        else if (objective.ObjectiveUnitCount == 0)
                        {
                            objective.ObjectiveStatus = ObjectiveStatus.complete;
                            Debug.Log(objective.ObjectiveTitle + " - Complete");
                            PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                        }
                        ShowObjectiveStatus(objective);
                        ObjectiveStatusOpen = true;
                    }
                }
            }
        }
    }

    public void UpdateCaptureObjective(EntityType type,IEntity captured)
    {
        if(type != EntityType.none || type != EntityType.resource)
        {
            if(type == EntityType.building)
            {

                if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
                {
                    foreach (Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                    {
                        if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.capture &&
                        objective.ObjectiveCondition == ObjectiveCondition.anyBuildings)
                        {
                            if (objective.ObjectiveBuildingCount > 0)
                            {
                                objective.currentBuildingsCount++;
                                objective.ObjectiveBuildingCount--;
                            }
                            else if (objective.ObjectiveBuildingCount == 0)
                            {
                                objective.ObjectiveStatus = ObjectiveStatus.complete;
                                Debug.Log(objective.ObjectiveTitle + " - Complete");
                                PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                            }
                            ShowObjectiveStatus(objective);
                            ObjectiveStatusOpen = true;
                        }
                        else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.capture &&
                        objective.ObjectiveCondition == ObjectiveCondition.buildingList)
                        {
                            if (objective.ObjectiveBuildingCount > 0)
                            {
                                objective.currentBuildingsCount++;
                                objective.ObjectiveBuildingCount--;
                            }
                            else if (objective.ObjectiveBuildingCount == 0)
                            {
                                objective.ObjectiveStatus = ObjectiveStatus.complete;
                                Debug.Log(objective.ObjectiveTitle + " - Complete");
                                PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                            }
                            ShowObjectiveStatus(objective);
                            ObjectiveStatusOpen = true;
                        }
                    }
                }
            }
            else if(type == EntityType.unit)
            {

                if (ActiveScenario.Objectives.OjectivesLsit.Count > 0)
                {
                    foreach (Objective objective in ActiveScenario.Objectives.OjectivesLsit)
                    {
                        if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.capture &&
                        objective.ObjectiveCondition == ObjectiveCondition.anyUnits)
                        {
                            if (objective.ObjectiveUnitCount > 0)
                            {
                                objective.currentUnitsCount++;
                                objective.ObjectiveUnitCount--;
                            }
                            else if (objective.ObjectiveUnitCount == 0)
                            {
                                objective.ObjectiveStatus = ObjectiveStatus.complete;
                                Debug.Log(objective.ObjectiveTitle + " - Complete");
                                PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                            }
                            ShowObjectiveStatus(objective);
                            ObjectiveStatusOpen = true;
                        }
                        else if (
                        objective.ObjectiveStatus == ObjectiveStatus.active &&
                        objective.ObjectiveType == ObjectiveType.capture &&
                        objective.ObjectiveCondition == ObjectiveCondition.unitList)
                        {
                            if (objective.ObjectiveUnitCount > 0)
                            {
                                objective.currentUnitsCount++;
                                objective.ObjectiveUnitCount--;
                            }
                            else if (objective.ObjectiveUnitCount == 0)
                            {
                                objective.ObjectiveStatus = ObjectiveStatus.complete;
                                Debug.Log(objective.ObjectiveTitle + " - Complete");
                                PlayerSessionData.currentScenarioPoints += objective.ObjectivePoints;
                            }
                            ShowObjectiveStatus(objective);
                            ObjectiveStatusOpen = true;
                        }
                    }
                }
            }

        }
    }

    public void ShowObjectiveStatus(Objective objective)
    {
        if (this.ObjectiveStatusDisplay != null)
        {
            if (this.ObjectiveTitle != null)
                this.ObjectiveTitle.text = objective.ObjectiveTitle;

            if (this.ObjectiveDescription != null)
                this.ObjectiveDescription.text = objective.ObjectiveDesc;

            if (this.ObjectiveStatusText != null)
            {
                switch (objective.ObjectiveStatus)
                {
                    case ObjectiveStatus.active:
                        this.ObjectiveStatusText.text = "Status: Active";
                        break;
                    case ObjectiveStatus.complete:
                        this.ObjectiveStatusText.text = "Status: Completed";
                        break;
                    case ObjectiveStatus.failed:
                        this.ObjectiveStatusText.text = "Status: Failed";
                        break;
                }
            }
            string typeStart = "";
            switch (objective.ObjectiveType)
            {
                case ObjectiveType.create:
                    switch (objective.ObjectiveCondition)
                    {
                        case ObjectiveCondition.anyUnits:
                            typeStart = "Train "+objective.ObjectiveUnitCount+" of any unit type ";
                            break;
                        case ObjectiveCondition.anyBuildings:
                            typeStart = "Build "+objective.ObjectiveBuildingCount+" structures";
                            break;
                        case ObjectiveCondition.unitList:
                            typeStart = "Train " + objective.ObjectiveUnitsList.Count + " of the below units ";
                            break;
                        case ObjectiveCondition.buildingList:
                            typeStart = "Build " + objective.ObjectiveBuildingList.Count + " structures";
                            break;
                    }
                    break;
                case ObjectiveType.destroy:
                    switch (objective.ObjectiveCondition)
                    {
                        case ObjectiveCondition.anyUnits:
                            typeStart = "Destroy " + objective.ObjectiveUnitCount + " enemy units";
                            break;
                        case ObjectiveCondition.anyBuildings:
                            typeStart = "Destroy " + objective.ObjectiveBuildingCount + " enemy structures";
                            break;
                        case ObjectiveCondition.unitList:
                            typeStart = "Destroy the units listed below";
                            break;
                        case ObjectiveCondition.buildingList:
                            typeStart = "Destroy the structures listed below";
                            break;
                    }
                    break;
                case ObjectiveType.capture:
                    typeStart = "";
                    break;
                case ObjectiveType.locate:
                    typeStart = "";
                    break;
                case ObjectiveType.survive:
                    typeStart = "Survive for some minutes";
                    break;
            }

            StartCoroutine(MoveUIPanel(this.ObjectiveStatusDisplay.transform.position, new Vector3(-20, -20, 0), this.ObjectiveStatusDisplay.transform, 3, true));
        }
    }

    public void StartScenarioInScene()
    {
        if(this.GameMgr != null)
        {
            this.ScenarioIntroPanel.SetActive(false);
            this.GameMgr.SetState(GameStateType.running);
            this.GameMgr.PeaceTimer.Reload(ActiveScenario.matchStartDelay);
        }
    }

    IEnumerator MoveUIPanel(Vector3 start, Vector3 end, Transform moving, float speed, bool sent)
    {
        var t = 0f;

        while (t < 1)
        {
            t += speed * Time.deltaTime;
            if (sent == false)
            {
                ObjectiveStatusOpen = true;
            }
            moving.position = Vector3.Lerp(start, end, t * speed);
            yield return null;
        }
    }


    public void Disable()
    {
        if (this.IsActive)
        {
            this.GameMgr.GameStartRunning -= PrepareScenarios;

            this.GlobalEvents.BuildingBuiltGlobal -= BuildingBuiltObjective;
            this.GlobalEvents.BuildingDeadGlobal -= BuildingDestroyedObjective;

            this.GlobalEvents.UnitInitiatedGlobal -= UnitTrainedObjective;
            this.GlobalEvents.UnitDeadGlobal -= UnitDestroyedObjective;

            if(this.ScenarioPlayerEntities.childCount > 0)
            {
                foreach(Transform child in this.ScenarioPlayerEntities)
                {
                    if (child != null)
                        GameObject.Destroy(child.gameObject);
                }
            }
            if(this.ScenarrioObjectivesList.childCount > 0)
            {
                foreach (Transform child in this.ScenarrioObjectivesList)
                {
                    if (child != null)
                        GameObject.Destroy(child.gameObject);
                }
            }
        }
    }
}


[Serializable]
public class Objectives
{
    public List<Objective> OjectivesLsit = new();
    public Objectives() { }
}

public enum ObjectiveType
{
    none,
    create,
    capture,
    destroy,
    survive,
    locate,
    collect
}
public enum ObjectiveCondition
{
    none,
    anyUnits,
    anyBuildings,
    unitList,
    buildingList,
    survive,
    collectAmount
}
public enum ObjectiveStatus
{
    none,
    active,
    complete,
    failed
}