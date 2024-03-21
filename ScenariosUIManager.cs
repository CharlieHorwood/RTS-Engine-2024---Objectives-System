using RTSEngine.Faction;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class ScenariosUIManager : MonoBehaviour
{

    public GameObject ScenarioItemPrefab = null;
    public List<ScenarioUI> ScenarioUI = new();

    public int PlayerCurrentScenarioPoints = 0;
    public TextMeshProUGUI PlayerCurrentPointsOutput = null;

    public HorizontalLayoutGroup ScenariosContainer = null;
    public TextMeshProUGUI ScenarioBriefingTitle = null;
    public TextMeshProUGUI SelectedScenarioBriefing = null;
    public TextMeshProUGUI SelectedScenarioObjectivesTitle = null;
    public TextMeshProUGUI SelectedScenarioObjectives = null;
    public TextMeshProUGUI SelectedScenarioKeyInfoTitle = null;
    public TextMeshProUGUI SelectedScenarioKeyInfo = null;

    public Button StartScenarioButton = null;
    public int SelectedScenario = -1;
    public int SelectedMapIndex = -1;
    public int SelectedScenarioInMap = -1;
    public string SelectedScenarioMapName = "";
    public int SliderActiveIndex = 0;

    public Button PrevSlideButton = null;
    public Button NextSlideButton = null;

    public GameObject LoadingPanel = null;
    public Image loadingBar = null;

    public void Start()
    {
        int counter = -1;
        if(this.ScenariosContainer != null && this.ScenarioUI.Count > 0 && this.ScenarioItemPrefab != null)
        {
            foreach(ScenarioUI scenario in this.ScenarioUI)
            {
                counter++;
                GameObject ScenarioItemNew = Instantiate(this.ScenarioItemPrefab,this.ScenariosContainer.transform);
                if(ScenarioItemNew != null)
                {
                    if(ScenarioItemNew.TryGetComponent<ScenarioItem>(out var item))
                    {
                        item.ScenarioItemIndex = counter;
                        item.ScenariosUIManager = this;
                        item.ScenarioIndexInMap = scenario.ScenarioIndexInMap;
                        item.ScenarioName.text = scenario.ScenarioName;
                        item.ScenarioMap = scenario.ScenarioMap;
                        item.ScenarioThumbnail.sprite = scenario.ScenarioThumbnail;
                        item.ScenarioExcerpt.text = scenario.ScenarioExcerpt;
                        item.ScenarioFactionLogo.sprite = scenario.ScenarioFactionLogo;
                        item.ScenarioBriefing = scenario.ScenarioBriefing;
                        item.ScenarioObjectives = scenario.ScenarioObjectives;
                        item.ScenarioKeyInfo = scenario.ScenarioKeyInfo;
                        item.ScenarioPlayerCount = scenario.ScenarioPlayerCount;
                    }
                }
            }
        }
        if(this.ScenarioUI.Count <= 5)
        {
            if (this.PrevSlideButton != null)
                this.PrevSlideButton.interactable = false;

            if (this.NextSlideButton != null)
                this.NextSlideButton.interactable = false;
        }
    }

    public void LaunchScenario()
    {
        PlayerSessionData.SelectedMapName = this.SelectedScenarioMapName;
        PlayerSessionData.SelectedScenario = this.SelectedScenario;
        PlayerSessionData.IsScenariosActive = true;
        PlayerSessionData.scenarioPlayerCount = this.ScenarioUI.ElementAtOrDefault(this.SelectedScenario).ScenarioPlayerCount;
        PlayerSessionData.Objectives = this.ScenarioUI.ElementAtOrDefault(this.SelectedScenarioInMap).ScenarioObjectives.OjectivesLsit;

        ScenarioUI activeOne = this.ScenarioUI.ElementAtOrDefault(this.SelectedScenario);

        PlayerSessionData.ActiveScenario = new(
            activeOne.ScenarioIndex,
            activeOne.ScenarioPrepTime,
            activeOne.ScenarioName,
            activeOne.ScenarioExcerpt,
            activeOne.ScenarioPlayerCount,
            activeOne.ScenarioFaction,
            activeOne.ScenarioNPCFaction,
            null,
            new(),
            new(),
            new(),
            new(),
            null,
            new()
        );
        PlayerSessionData.ActiveScenario.Objectives.OjectivesLsit = PlayerSessionData.Objectives;


        if (this.LoadingPanel != null)
        {
            this.LoadingPanel.SetActive(true);
        }
        StartCoroutine(LoadSceneAsync(this.SelectedScenarioMapName));

    }

    public void RemoveSelected()
    {
        foreach(Transform itemTransform in this.ScenariosContainer.transform)
        {
            if (itemTransform.TryGetComponent<ScenarioItem>(out var item))
            {
                item.IsSelected = false;
                item.MPImage.OutlineWidth = 0;
            }
        }
    }

    public void SlideItems(string direction)
    {
        if(direction == SlideDirection.right.ToString() && this.ScenariosContainer.padding.left < 0)
        {
            this.SliderActiveIndex++;
            int leftStart = this.ScenariosContainer.padding.left;
            int leftOffset = leftStart - 265;
            StartCoroutine(SlidePaddingInDirection(this.ScenariosContainer.padding.left, leftOffset, 1, this.ScenariosContainer));

        }
        else if(direction == SlideDirection.left.ToString() && this.ScenariosContainer.padding.left < 0)
        {
            this.SliderActiveIndex--;
            int leftStart = this.ScenariosContainer.padding.left;
            int leftOffset = leftStart + 265;
            StartCoroutine(SlidePaddingInDirection(this.ScenariosContainer.padding.left, leftOffset, 1, this.ScenariosContainer));
        }
    }

    public IEnumerator SlidePaddingInDirection(int from, int to, float speed, HorizontalLayoutGroup tra)
    {
        var t = 0f;
        RectOffset rectOffset = new RectOffset(
            tra.padding.left,
            tra.padding.top,
            tra.padding.right,
            tra.padding.bottom
        );
        while (t < 1f)
        {
            t += speed * Time.deltaTime;
            rectOffset.left = (int)Mathf.Lerp(from,to,t);
            tra.padding = rectOffset;
            yield return null;
        }
    }

    IEnumerator LoadSceneAsync(string levelName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        float progress = 0;
        while (!op.isDone)
        {
            progress = Mathf.Clamp01(op.progress / .9f);
            if (this.loadingBar != null)
            {
                this.loadingBar.fillAmount = progress;
            }
            yield return null;
        }
    }

    public enum SlideDirection
    {
        none,
        left,
        right
    }

    public bool IsWholeNumber(float number)
    {
        return Mathf.Approximately(number,Mathf.RoundToInt(number));
    }

    public IEnumerator PulseSeconds(TextMeshProUGUI btn_text,float from, float speed)
    {
        float timer = from;
        //btn_text.color = new(255, 168, 0,0);
        while(timer > 0)
        {
            timer -= Time.deltaTime * speed;
            //btn_text.fontSize = Mathf.Lerp(40, 30, timer);
            btn_text.color = Vector4.Lerp(new(255, 168, 0,0), new(255, 168, 0,255), timer);
            
        }
        yield return null;
    }
}

[Serializable]
public class ScenarioUI
{
    public int ScenarioIndex = -1;
    public ScenarioMap ScenarioMap;
    public int ScenarioIndexInMap = -1;
    public string ScenarioName = "";
    public int ScenarioPrepTime = 120;
    public Sprite ScenarioThumbnail = null;
    public string ScenarioExcerpt = "";
    public int ScenarioPlayerCount = 0;
    public FactionTypeInfo ScenarioFaction = null;
    public FactionTypeInfo ScenarioNPCFaction = null;
    
    public List<GameObject> NPCManagers = new();
    public Sprite ScenarioFactionLogo = null;
    public string ScenarioBriefing = "";
    public string[] ScenarioKeyInfo = new string[0];
    public Objectives ScenarioObjectives = new();
    public ScenarioUI(
        int ScenarioIndex,
        ScenarioMap ScenarioMap,
        int ScenarioIndexInMap,
        int ScenarioPlayerCount,
        string ScenarioName,
        int ScenarioPrepTime,
        Sprite ScenarioThumbnail,
        string ScenarioExcerpt,
        FactionTypeInfo ScenarioFaction,
        FactionTypeInfo ScenarioNPCFaction,
        Sprite ScenarioFactionLogo,
        string ScenarioBriefing,
        string[] ScenarioKeyInfo,
        Objectives ScenarioObjectives)
    {
        this.ScenarioIndex = ScenarioIndex;
        this.ScenarioMap = ScenarioMap;
        this.ScenarioIndexInMap = ScenarioIndexInMap;
        this.ScenarioPlayerCount = ScenarioPlayerCount;
        this.ScenarioName = ScenarioName;
        this.ScenarioPrepTime = ScenarioPrepTime;
        this.ScenarioThumbnail = ScenarioThumbnail;
        this.ScenarioExcerpt = ScenarioExcerpt;
        this.ScenarioFaction = ScenarioFaction;
        this.ScenarioNPCFaction = ScenarioNPCFaction;
        this.ScenarioFactionLogo = ScenarioFactionLogo;
        this.ScenarioBriefing = ScenarioBriefing;
        this.ScenarioObjectives = ScenarioObjectives;
        this.ScenarioKeyInfo = ScenarioKeyInfo;
    }
}

public enum ScenarioMap
{
    Select_Map,
    Goldhorn_Island_V3,
    Dandrum_Pandrum_v2
}