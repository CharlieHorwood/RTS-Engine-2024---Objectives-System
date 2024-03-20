using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

using RTSEngine;
using RTSEngine.Event;
using RTSEngine.Entities;
using RTSEngine.UnitExtension;
using RTSEngine.Movement;
using RTSEngine.Attack;
using RTSEngine.EntityComponent;
using RTSEngine.Health;
using RTSEngine.Faction;
using RTSEngine.Game;
public class ObjectiveEntity : MonoBehaviour, IEntityPreInitializable
{
    protected IGameManager GameMgr { private set; get; }
    protected RTSEObjectiveSystem ObjectiveSystem { private set; get; }
    protected IEntity Entity { private set; get; }

    public IUnit UnitRef { private set; get; }
    public IBuilding BuildingRef { private set; get; }
    public IResource ResourceRef { private set; get; }

    protected bool IsObjective { private set; get; } = false;

    protected Converter Converter { private set; get; }

    public void OnEntityPreInit(IGameManager GameMgr, IEntity Entity)
    {
        this.GameMgr = GameMgr;
        this.ObjectiveSystem = GameMgr.GetService<RTSEObjectiveSystem>();
        this.Entity = Entity;
        switch (Entity.Type)
        {
            case EntityType.unit:
                this.UnitRef = Entity as IUnit;

               if(TryGetComponent(out Converter convertor)){
                    this.Converter = convertor;
                }

                break;
            case EntityType.building:
                this.BuildingRef = Entity as IBuilding;
                break;
            case EntityType.resource:
                this.ResourceRef = Entity as IResource;
                break;
        }
        if(IsObjective)
        {
            if(this.Converter != null)
            {
                this.Converter.TargetUpdated += TargetConverted;
            }
        }
    }

    public void TargetConverted(IEntityTargetComponent TargetComp, TargetDataEventArgs EventArgs)
    {
        if(EventArgs.Data.instance.FactionID == this.Entity.FactionID && this.ObjectiveSystem != null && this.ObjectiveSystem.IsActive)
        {
            this.ObjectiveSystem.UpdateCaptureObjective(EventArgs.Data.instance.Type, EventArgs.Data.instance);
        }
    }

    public void Disable()
    {
        if (IsObjective)
        {
            if (this.Converter != null)
            {
                this.Converter.TargetUpdated -= TargetConverted;
            }
        }
    }
}
