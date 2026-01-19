using System.IO;
using Game.Features.Wheel.Core;
using UnityEngine;

public static class SaveSystem
{
    private const string FileName = "session_data.json";

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    #region Public API

    public static void SaveGameContext(GameContext context)
    {
        try
        {
            context.CaptureSaveData();

            var json = JsonUtility.ToJson(context, true);
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e}");
        }
    }

    public static GameContext LoadGameContext()
    {
        if (!File.Exists(SavePath))
        {
            return CreateAndSaveDefault();
        }

        try
        {
            var json = File.ReadAllText(SavePath);
            var context = JsonUtility.FromJson<GameContext>(json);

            if (context == null)
                return CreateAndSaveDefault();

            context.ApplySaveData();
            return context;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Load failed, recreating save: {e}");
            return CreateAndSaveDefault();
        }
    }

    #endregion

    #region Internal

    private static GameContext CreateAndSaveDefault()
    {
        var context = CreateDefaultContext();
        SaveGameContext(context);
        return context;
    }

    private static GameContext CreateDefaultContext()
    {
        var context = new GameContext(
            new UserDataModel(),
            new WheelSpinProgressModel()
        );

        // Default inventory / progress
        context.userDataModel.AddInventoryItem(1, 25);
        context.userDataModel.AddInventoryItem(2, 50);

        context.CaptureSaveData();
        return context;
    }

    #endregion
}