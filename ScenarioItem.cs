using MPUIKIT;
using RTSEngine.Faction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioItem : MonoBehaviour
{
    public ScenariosUIManager ScenariosUIManager = null;
    public int ScenarioItemIndex = -1;
    public bool IsSelected = false;
    public ScenarioMap ScenarioMap = ScenarioMap.Select_Map;
    public int ScenarioIndexInMap = -1;
    public TextMeshProUGUI ScenarioName = null;
    public Image ScenarioThumbnail = null;
    public int ScenarioPlayerCount = 0;
    public TextMeshProUGUI ScenarioExcerpt = null;
    public FactionTypeInfo ScenarioFaction = null;
    public Image ScenarioFactionLogo = null;
    public string ScenarioBriefing = "";
    public string[] ScenarioKeyInfo = new string[0];
    public Objectives ScenarioObjectives = new();

    public Image ScenarioItemPanel = null;
    public MPImage MPImage { private set; get; }

    public void Start()
    {
        if (this.ScenarioItemPanel != null)
        {
            this.MPImage = this.ScenarioItemPanel as MPImage;
        }
    }

    public void OnMouseHover()
    {
        if(this.MPImage != null)
        {
            this.MPImage.OutlineWidth = 3;
        }
    }

    public void OnMouseLeave()
    {
        if (this.MPImage != null && !this.IsSelected)
        {
            this.MPImage.OutlineWidth = 0;
        }
    }

    public void OnMouseClick()
    {
        if(!this.IsSelected)
        {
            if(this.ScenariosUIManager != null)
            {
                this.ScenariosUIManager.RemoveSelected();
            }
            this.IsSelected = true;
            if (this.MPImage != null)
            {
                this.MPImage.OutlineWidth = 3;
            }
            string briefTitle = ScenarioName.text+": Mission Brief";
            this.ScenariosUIManager.ScenarioBriefingTitle.text = briefTitle;
            this.ScenariosUIManager.ScenarioBriefingTitle.gameObject.SetActive(true);
            this.ScenariosUIManager.SelectedScenarioBriefing.text = this.ScenarioBriefing;
            this.ScenariosUIManager.SelectedScenarioKeyInfoTitle.gameObject.SetActive(true);

            this.ScenariosUIManager.SelectedMapIndex = ((int)ScenarioMap) - 1;
            this.ScenariosUIManager.SelectedScenario = this.ScenarioItemIndex;
            this.ScenariosUIManager.SelectedScenarioInMap = this.ScenarioIndexInMap;
            this.ScenariosUIManager.SelectedScenarioMapName = ScenarioMap.ToString();

            int lineCount = 0;
            int total = this.ScenarioKeyInfo.Length;
            this.ScenariosUIManager.SelectedScenarioKeyInfo.text = "";
            foreach (string line in this.ScenarioKeyInfo)
            {
                lineCount++;
                if(lineCount < total)
                {
                    this.ScenariosUIManager.SelectedScenarioKeyInfo.text += line + "\n";
                }
                else
                {
                    this.ScenariosUIManager.SelectedScenarioKeyInfo.text += line;
                }
                
            }
            this.ScenariosUIManager.SelectedScenarioObjectivesTitle.gameObject.SetActive(true);
            lineCount = 0;
            total = this.ScenarioObjectives.OjectivesLsit.Count;
            this.ScenariosUIManager.SelectedScenarioObjectives.text = "";
            foreach (Objective objective in this.ScenarioObjectives.OjectivesLsit)
            {
                lineCount++;
                if (lineCount < total)
                {
                    this.ScenariosUIManager.SelectedScenarioObjectives.text += objective.ObjectiveTitle + "\n";
                }
                else
                {
                    this.ScenariosUIManager.SelectedScenarioObjectives.text += objective.ObjectiveTitle;
                }
            }
            if(this.ScenariosUIManager.StartScenarioButton != null)
            {
                this.ScenariosUIManager.StartScenarioButton.interactable = true;
            }
        }
    }

}
