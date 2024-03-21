using RTSEngine.Lobby;
using System.Collections.Generic;

public static class PlayerSessionData
{
    public static int PlayerID = -1;

    public static int PlayerScenarioPoints = 0;

    public static bool IsScenariosActive = false;

    public static int SelectedScenario = -1;

    public static string SelectedMapName = "";

    public static Scenario ActiveScenario = null;

    public static List<Objective> Objectives = new ();

    public static int currentScenarioPoints = 0;

    public static int scenarioPlayerCount = 0;

    // Tracking scores
    public static int scenariowins = 0;
    public static int rmpwins = 0;
    public static int cmpwins = 0;
    public static int spwins = 0;
    public static int skwins = 0;

    public static int scenarioloses = 0;
    public static int rmploses = 0;
    public static int cmploses = 0;
    public static int sploses = 0;
    public static int skloses = 0;

    public static List<ILobbyFactionSlot> PlayerSlots = new();

    public static bool useCeaseFire = true;
    public static int ceaseTimer = 300;

}
