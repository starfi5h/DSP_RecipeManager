using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


[assembly: AssemblyTitle(RecipeManager.Plugin.GUID)]
[assembly: AssemblyProduct(RecipeManager.Plugin.NAME)]
[assembly: AssemblyVersion(RecipeManager.Plugin.VERSION)]

namespace RecipeManager
{

    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "starfi5h.plugin.RecipeManager";
        public const string NAME = "RecipeManager";
        public const string VERSION = "1.0.0";

        static Harmony harmony;
        static ManualLogSource Log;
        static ConfigEntry<string> GridIndexDictionaryJSON;

        static int selectedRecipeID = -1;
        static int GridIndex = 0;
        static Dictionary<int, int> GridIndexDictionary;
        static Dictionary<int, int> GridIndexDictionaryOrigin;

        static GameObject editModeTextObj;
        static GameObject destRecipeSelImageObj;
        static GameObject resetButtonObj;
        static GameObject editButtonObj;
        static GameObject selectedRecipeIcon;

        static Button resetButton;
        static Button editButton;
        static Text resetText;
        static Text editText;
        static bool editMode = false;

        public static void WriteGridIndexDictionary()
        {
            var dictWrapper = new DictWrapper();
            dictWrapper.FromDict(GridIndexDictionary);
            var text = JsonUtility.ToJson(dictWrapper);
            GridIndexDictionaryJSON.Value = text;
            //Log.LogDebug(text);
        }

        public static void ReadGridIndexDictionary()
        {
            var text = GridIndexDictionaryJSON.Value;
            var dictWrapper = (DictWrapper)JsonUtility.FromJson(text, typeof(DictWrapper));
            GridIndexDictionary = dictWrapper.ToDict();
            Log.LogDebug("ReadGridIndexDictionary: " + GridIndexDictionary.Count);
        }

#if DEBUG
        public void OnDestroy()
        {
            harmony.UnpatchSelf();
            foreach (var recipe in LDB.recipes.dataArray)
            {
                recipe.GridIndex = GridIndexDictionaryOrigin[recipe.ID];
            }
            Destroy(editModeTextObj);
            Destroy(destRecipeSelImageObj);
            Destroy(resetButtonObj);
            Destroy(editButtonObj);
            Destroy(selectedRecipeIcon);
            UIRoot.instance.uiGame.replicator.okButton.onRightClick -= OnOkButtonRightClick;
        }
#endif

        public void Awake()
        {
            Log = Logger;
            harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(Plugin));

            GridIndexDictionaryJSON = Config.Bind("General", "GridIndexDictionary", "{}", "JSON format string of custom recipe grid index pair");
            ReadGridIndexDictionary();
#if DEBUG
            CreateUI();
            SetRecipeGridIndex();
#endif
        }


        static void CreateUI()
        {
            //ボタンを追加
            GameObject logicbutton = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Station Window/storage-box-0/popup-box/sd-option-button-0");
            GameObject recipegroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group");
            resetButtonObj = Instantiate(logicbutton);
            editButtonObj = Instantiate(logicbutton);
            resetButtonObj.SetActive(true);
            editButtonObj.SetActive(true);

            resetButtonObj.name = "resetButtonObj";
            editButtonObj.name = "editButtonObj";

            resetButtonObj.transform.SetParent(recipegroup.transform, false);
            editButtonObj.transform.SetParent(recipegroup.transform, false);

            resetButtonObj.transform.localPosition = new Vector3(315 - 50, 55, 0);
            editButtonObj.transform.localPosition = new Vector3(295 - 50, 25, 0);

            RectTransform resetButtonRT = resetButtonObj.GetComponent<RectTransform>();
            RectTransform editButtonRT = editButtonObj.GetComponent<RectTransform>();

            resetButtonRT.sizeDelta = new Vector2(80, 25);
            editButtonRT.sizeDelta = new Vector2(100, 25);

            resetButton = resetButtonObj.GetComponent<Button>();
            editButton = editButtonObj.GetComponent<Button>();

            Image resetButtonImage = resetButtonObj.GetComponent<Image>();
            Image editButtonImage = editButtonObj.GetComponent<Image>();

            resetButtonImage.color = new Color(1.0f, 0.68f, 0.45f, 0.7f);
            editButtonImage.color = new Color(0.240f, 0.55f, 0.65f, 0.7f);

            resetText = resetButtonObj.GetComponentInChildren<Text>();
            editText = editButtonObj.GetComponentInChildren<Text>();
            resetText.text = "Reset All".Translate();
            editText.text = "Enter Edit Mode".Translate();

            resetText.name = "resetText";
            editText.name = "editText";

            resetButton.onClick.AddListener(OnClickResetButton);
            editButton.onClick.AddListener(OnClickEditButton);

            GameObject selimg = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group/sel-img");
            destRecipeSelImageObj = Instantiate(selimg);
            destRecipeSelImageObj.transform.SetParent(recipegroup.transform, false);
            destRecipeSelImageObj.SetActive(false);

            selectedRecipeIcon = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-tree/center-icon")) as GameObject;
            selectedRecipeIcon.transform.SetParent(recipegroup.transform, false);
            selectedRecipeIcon.transform.localPosition = new Vector3(200 - 50, 36, 0);
            selectedRecipeIcon.transform.Find("place-text").GetComponentInChildren<Text>().text = "Selected Recipe".Translate();
            Destroy(selectedRecipeIcon.transform.Find("vline-m").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("hline-0").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("hline-1").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("icon 2").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("text 1").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("text 2").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("time-text").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("time-text").gameObject);
            selectedRecipeIcon.SetActive(false);

            GameObject modetext = GameObject.Find("UI Root/Overlay Canvas/In Game/Function Panel/bg-trans/mode-text");
            editModeTextObj = Instantiate(modetext);
            editModeTextObj.transform.SetParent(recipegroup.transform, false);
            editModeTextObj.transform.localPosition = new Vector3(0, -3, 0);
            Destroy(editModeTextObj.GetComponent<Localizer>());
            editModeTextObj.GetComponent<Text>().text = "Edit mode".Translate();
        }

        public static void OnClickResetButton()
        {
            Log.LogInfo("Reset All");
            foreach (var recipe in LDB.recipes.dataArray)
            {
                recipe.GridIndex = GridIndexDictionaryOrigin[recipe.ID];
            }
            GridIndexDictionary.Clear();
            WriteGridIndexDictionary();
            UIRoot.instance.uiGame.replicator.RefreshRecipeIcons();
        }

        public static void OnClickEditButton()
        {
            if (!editMode)
            {
                editModeTextObj.SetActive(true);
                //Log.LogDebug("Edit mode on");
                resetButtonObj.SetActive(true);
                editText.text = "Exit Edit Mode".Translate();
                editMode = true;
                destRecipeSelImageObj.gameObject.SetActive(true);
                UIRoot.instance.uiGame.replicator.recipeSelImage.gameObject.SetActive(false);
                selectedRecipeIcon.SetActive(true);
                selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(false);
                destRecipeSelImageObj.gameObject.SetActive(false);
                UIRoot.instance.uiGame.replicator.selectedRecipe = null;

                UIRoot.instance.uiGame.replicator.treeTweener1.Play1To0Continuing();
                UIRoot.instance.uiGame.replicator.treeTweener2.Play1To0Continuing();
                UIRoot.instance.uiGame.replicator.instantItemSwitch.gameObject.SetActive(false);
                UIRoot.instance.uiGame.replicator.RefreshRecipeIcons();
            }
            else
            {
                editModeTextObj.SetActive(false);
                //Log.LogDebug("Edit mode off");
                resetButtonObj.SetActive(false);
                editText.text = "Enter Edit Mode".Translate();
                editMode = false;
                selectedRecipeID = -1;
                destRecipeSelImageObj.gameObject.SetActive(false);
                selectedRecipeIcon.SetActive(false);
                UIRoot.instance.uiGame.replicator.instantItemSwitch.gameObject.SetActive(GameMain.sandboxToolsEnabled);
                UIRoot.instance.uiGame.replicator.RefreshRecipeIcons();
                WriteGridIndexDictionary();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.RefreshRecipeIcons))]
        static void RefreshRecipeIcons_Postfix(UIReplicatorWindow __instance)
        {
            if (!editMode) return;

            // In edit mode, display all the recipe and hightlight the grid with duplicated recipes
            Array.Clear(__instance.recipeIndexArray, 0, __instance.recipeIndexArray.Length);
            Array.Clear(__instance.recipeStateArray, 0, __instance.recipeStateArray.Length);
            Array.Clear(__instance.recipeProtoArray, 0, __instance.recipeProtoArray.Length);
            int recipeColCount = __instance.queueNumTexts.Length; // vanilla:14 GenesisBook:17
            RecipeProto[] dataArray = LDB.recipes.dataArray;
            IconSet iconSet = GameMain.iconSet;
            var gridIndexSet = new HashSet<int>();
            for (int i = 0; i < dataArray.Length; i++)
            {
                int type = dataArray[i].GridIndex / 1000;
                int row = (dataArray[i].GridIndex - type * 1000) / 100 - 1;
                int col = dataArray[i].GridIndex % 100 - 1;
                int index = row * recipeColCount + col;

                if (type == __instance.currentType && index < __instance.recipeIndexArray.Length)
                {
                    int recipeId = dataArray[i].ID;
                    __instance.recipeIndexArray[index] = iconSet.recipeIconIndex[recipeId];
                    __instance.recipeProtoArray[index] = dataArray[i];
                    if (gridIndexSet.Contains(dataArray[i].GridIndex))
                    {
                        __instance.recipeStateArray[index] |= 8U; // highlight duplicated as red background
                    }
                    else if (GridIndexDictionary.ContainsKey(recipeId))
                    {
                        __instance.recipeStateArray[index] |= 4U; // highlight changed as blue background
                    }
                    gridIndexSet.Add(dataArray[i].GridIndex);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.TestMouseRecipeIndex))]
        static bool TestMouseRecipeIndex_Prefix(UIReplicatorWindow __instance)
        {
            if (selectedRecipeID == -1 || editMode == false) return true;

            if (__instance.mouseInRecipe && UIRoot.ScreenPointIntoRect(Input.mousePosition, __instance.recipeBg.rectTransform, out var vector))
            {
                int col = Mathf.FloorToInt(vector.x / 46f);
                int row = Mathf.FloorToInt(-vector.y / 46f);
                int recipeColCount = __instance.queueNumTexts.Length; // vanilla:14 GenesisBook:17
                if (col >= 0 && row >= 0 && col < recipeColCount && row < UIReplicatorWindow.recipeRowCount)
                {
                    __instance.mouseRecipeIndex = col + row * recipeColCount;
                    destRecipeSelImageObj.gameObject.SetActive(true);
                    destRecipeSelImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(col * 46 - 1, (-row * 46 + 1));
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.SetSelectedRecipeIndex))]
        static bool SetSelectedRecipeIndex_Prefix(UIReplicatorWindow __instance, int index)
        {
            GridIndex = 0;
            if (editMode == false || index == -1) return true;

            int recipeColCount = __instance.queueNumTexts.Length; // vanilla:14 GenesisBook:17
            if (selectedRecipeID == -1) // First click: Try to select the recipe to move
            {
                if (__instance.selectedRecipe != null)
                {
                    __instance.recipeSelImage.rectTransform.anchoredPosition = new Vector2((float)(index % recipeColCount * 46 - 1), (float)(-(float)(index / recipeColCount) * 46 + 1));
                    __instance.recipeSelImage.gameObject.SetActive(true);
                    selectedRecipeID = __instance.selectedRecipe.ID;
                    Log.LogDebug("origin grid selecteed : " + selectedRecipeID);
                    selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(true);
                    selectedRecipeIcon.transform.Find("icon 1").GetComponentInChildren<Image>().sprite = LDB.recipes.Select(selectedRecipeID).iconSprite;
                }
                else
                {
                    return true;
                }
            }
            else // Second click: Try to move the selectedRecipeID to target grid
            {
                destRecipeSelImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2((index % recipeColCount * 46 - 1), (-(index / recipeColCount) * 46 + 1));
                VFAudio.Create("ui-click-0", null, Vector3.zero, true, 0);
                int num1 = index / recipeColCount + 1;
                int num2 = index % recipeColCount + 1;
                GridIndex = __instance.currentType * 1000 + num1 * 100 + num2;
                Log.LogDebug("destiantion grid selecteed : " + GridIndex);

                if (__instance.selectedRecipe != null) // Swap with the existing recipe
                {
                    Log.LogInfo("Recipe" + selectedRecipeID + " => " + GridIndex + "(orignal recipe" + __instance.selectedRecipe.ID + ")");
                    int tmpGridIndex = LDB.recipes.Select(selectedRecipeID).GridIndex;
                    LDB.recipes.Select(selectedRecipeID).GridIndex = LDB.recipes.Select(__instance.selectedRecipe.ID).GridIndex;
                    LDB.recipes.Select(__instance.selectedRecipe.ID).GridIndex = tmpGridIndex;
                    GridIndexDictionary[selectedRecipeID] = GridIndex;
                    GridIndexDictionary[__instance.selectedRecipe.ID] = tmpGridIndex;
                }
                else // Move into the empty grid
                {
                    Log.LogInfo("Recipe" + selectedRecipeID + " => " + GridIndex + "(empty)");
                    LDB.recipes.Select(selectedRecipeID).GridIndex = GridIndex;
                    __instance.selectedRecipe = LDB.recipes.Select(selectedRecipeID);
                    GridIndexDictionary[selectedRecipeID] = GridIndex;

                }
                destRecipeSelImageObj.gameObject.SetActive(false);
                __instance.recipeSelImage.gameObject.SetActive(false);
                UIRoot.instance.uiGame.replicator.RefreshRecipeIcons();                
                selectedRecipeID = -1;
                selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(false);
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow._OnOpen))]
        [HarmonyPatch(typeof(UIReplicatorWindow), nameof(UIReplicatorWindow.OnInstantSwitchClick))]
        static void ShowButtons(UIReplicatorWindow __instance)
        {
            if (resetButtonObj == null)
            {
                CreateUI();
                UIRoot.instance.uiGame.replicator.okButton.onRightClick += OnOkButtonRightClick;
            }
            resetButtonObj.SetActive(false);
            editButtonObj.SetActive(!__instance.isInstantItem); // Hide edit button if sandbox free item toggle is on
        }

        [HarmonyPostfix, HarmonyPriority(Priority.Low)]
        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded))]
        static void SetRecipeGridIndex()
        {
            if (GridIndexDictionaryOrigin != null) return;
                        
            GridIndexDictionaryOrigin = new();
            int count = 0;
            foreach (var recipe in LDB.recipes.dataArray)
            {
                if (recipe == null) continue;
                GridIndexDictionaryOrigin[recipe.ID] = recipe.GridIndex; // Save the original grid for reset

                if (GridIndexDictionary.ContainsKey(recipe.ID))
                {
                    Log.LogDebug("Recipe" + recipe.ID + ": " + recipe.GridIndex + " => " + GridIndexDictionary[recipe.ID]);
                    recipe.GridIndex = GridIndexDictionary[recipe.ID];
                    count++;
                }
            }
            Log.LogInfo("Recipe grid index changed: " + count);
        }

        static void OnOkButtonRightClick(int _)
        {
            var mechaForge = GameMain.mainPlayer.mecha.forge;
            var tasks = new List<ForgeTask>();
            var tick = 0;

            if (mechaForge.tasks.Count > 0) tick = mechaForge.tasks[0].tick; // Store the progress of first task

            for (int i = 0; i < mechaForge.tasks.Count; i++)
            {
                if (mechaForge.tasks[i].parentTaskIndex >= 0) continue; // Skip the tasks that are not added by hand
                tasks.Add(mechaForge.tasks[i]);
            }
            mechaForge.CancelAllTasks();
            UIRoot.instance.uiGame.replicator.OnOkButtonClick(0, true);

            int startingIndex = mechaForge.tasks.Count;
            for (int i = 0; i < tasks.Count; i++)
            {
                if (mechaForge.AddTask(tasks[i].recipeId, tasks[i].count) == null)
                {
                    Log.LogWarning($"Can't AddTask {tasks[i].recipeId} {tasks[i].count}");
                }
            }
            if (startingIndex < mechaForge.tasks.Count) mechaForge.tasks[startingIndex].tick = tick; // Resume the progress
        }
    }
}
