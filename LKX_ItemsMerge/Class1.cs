﻿using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

namespace LKX_ItemsMerge
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 自动时节结束合并道具
        /// </summary>
        public bool autoMerge;

        /// <summary>
        /// 是否开启指定合并大小
        /// </summary>
        public bool enabledSize;

        /// <summary>
        /// 合并最大耐久值
        /// </summary>
        public uint itemsSize;

        /// <summary>
        /// 药品拆分数量
        /// </summary>
        public uint drugsCount;

        /// <summary>
        /// 药品拆分大小
        /// </summary>
        public int drugsSize;

        /// <summary>
        /// 药品拆分品级
        /// </summary>
        public List<int> drugsLevel = new List<int>();

        /// <summary>
        /// 合并道具类型
        /// </summary>
        public List<int> mergeType = new List<int>();

        /// <summary>
        /// 道具品级
        /// </summary>
        public List<int> itemLevel = new List<int>();

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        //public static List<int> itemsKey = new List<int>();

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(159f / 256f, 20f / 256f, 29f / 256f);
            GUILayout.Label("如果status亮红灯代表mod失效！", redLabelStyle);
            Main.settings.autoMerge = GUILayout.Toggle(Main.settings.autoMerge, "开启结束时节时自动合并道具");
            Main.settings.enabledSize = GUILayout.Toggle(Main.settings.enabledSize, "开启指定合并大小限制");

            if (Main.settings.enabledSize)
            {
                string itemSize = GUILayout.TextField(Main.settings.itemsSize.ToString());
                if (itemSize == "" || itemSize == null) itemSize = "0";
                if (GUI.changed) Main.settings.itemsSize = uint.Parse(itemSize);
            }

            if (GUILayout.Button("手动点击合并"))
            {
                Main.RunningMergeItems();
            }

            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.SetGUIToToggle(31, "药品", ref Main.settings.mergeType);
            Main.SetGUIToToggle(22, "工具", ref Main.settings.mergeType);
            Main.SetGUIToToggle(30, "毒药", ref Main.settings.mergeType);
            Main.SetGUIToToggle(37, "酒水", ref Main.settings.mergeType);
            Main.SetGUIToToggle(41, "茶叶", ref Main.settings.mergeType);
            GUILayout.EndHorizontal();

            GUILayout.Label("选择合并的酒水，茶叶品级。仅针对酒水茶叶有效");
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.SetGUIToToggle(1, "九品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(2, "八品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(3, "七品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(4, "六品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(5, "五品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(6, "四品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(7, "三品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(8, "二品", ref Main.settings.itemLevel);
            Main.SetGUIToToggle(9, "一品", ref Main.settings.itemLevel);
            GUILayout.EndHorizontal();

            GUILayout.Label("");

            GUILayout.Label("拆分药品一般用于送礼，拆分只判断单个药品耐久。建议和合并大小限制配合使用。");
            GUILayout.Label("一个例子：有2个100耐久药品，拆分大小为5，拆分数量为20。此时拆解会成功。");
            GUILayout.Label("一个例子：有2个100耐久药品，拆分大小为5，拆分数量为40。此时拆解会失败。");
            if (GUILayout.Button("手动拆分药品"))
            {
                Main.ResolveMergeDrugs();
            }

            GUILayout.Label("选择拆分的大小。");
            Main.settings.drugsSize = GUILayout.SelectionGrid(Main.settings.drugsSize, new string[] { "不拆", "最大2", "最大3", "最大4", "最大5" }, 5);
            GUILayout.Label("选择允许拆分的品级。");
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.SetGUIToToggle(1, "九品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(2, "八品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(3, "七品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(4, "六品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(5, "五品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(6, "四品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(7, "三品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(8, "二品", ref Main.settings.drugsLevel);
            Main.SetGUIToToggle(9, "一品", ref Main.settings.drugsLevel);
            GUILayout.EndHorizontal();
            GUILayout.Label("拆分的数量。");
            string drugsItemCount = GUILayout.TextField(Main.settings.drugsCount.ToString());
            if (drugsItemCount == "" || drugsItemCount == null) drugsItemCount = "0";
            if (GUI.changed) Main.settings.drugsCount = uint.Parse(drugsItemCount);
            
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        /// <summary>
        /// 生成GUI开关
        /// </summary>
        /// <param name="index">道具类型index 看item的index 5</param>
        /// <param name="name">开关名称</param>
        static void SetGUIToToggle(int index, string name, ref List<int> field)
        {
            bool status = GUILayout.Toggle(field.Contains(index), name);
            if (GUI.changed)
            {
                if (status)
                {
                    if (!field.Contains(index)) field.Add(index);
                }
                else
                {
                    if (field.Contains(index)) field.Remove(index);
                }
            }
        }

        /// <summary>
        /// 拆分药品
        /// </summary>
        public static void ResolveMergeDrugs()
        {
            if (!Main.enabled || Main.settings.drugsCount <= 0) return;

            DateFile df = DateFile.instance;
            Dictionary<int, int> items = new Dictionary<int, int>();
            Dictionary<int, int> itemsId = new Dictionary<int, int>();
            if (df.actorItemsDate.TryGetValue(df.mianActorId, out itemsId))
            {
                List<int> buffer = itemsId.Keys.ToList();
                foreach (int itemId in buffer)
                {
                    string id = df.GetItemDate(itemId, 999), surpluses = df.GetItemDate(itemId, 901), limit = df.GetItemDate(itemId, 902),
                        level = df.GetItemDate(itemId, 8), type = df.GetItemDate(itemId, 5);
                    
                    if (!Main.settings.drugsLevel.Contains(int.Parse(level)) && type != "31") continue;
                    if (int.Parse(surpluses) < (Main.settings.drugsCount * Main.settings.drugsSize)) continue;

                    if (!items.ContainsKey(int.Parse(id))) items.Add(int.Parse(id), itemId);
                    
                }
                Main.logger.Log(items.Count.ToString());
                if (items.Count <= 0)
                {
                    Main.logger.Log("拆分失败：没有找到足够拆分的药品，合并后再试试。");
                }
                else
                {
                    foreach (KeyValuePair<int, int> item in items)
                    {
                        Dictionary<int, string> baseItem = df.itemsDate[item.Value];
                        int size = Main.settings.drugsSize;
                        if (Main.settings.drugsSize <= 0) size = 2;
                        if (Main.settings.drugsSize > 5) size = 5;

                        for (int i = 0; i < Main.settings.drugsCount; i++)
                        {
                            if (int.Parse(df.itemsDate[item.Value][901]) == size) break;

                            int makeItemId = df.MakeNewItem(item.Key);
                            df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                            df.itemsDate[makeItemId][901] = size.ToString();
                            df.itemsDate[makeItemId][902] = size.ToString();
                            df.itemsDate[item.Value][901] = (int.Parse(df.itemsDate[item.Value][901]) - size).ToString();
                            df.itemsDate[item.Value][902] = (int.Parse(df.itemsDate[item.Value][902]) - size).ToString();
                        }
                    }
                    Main.logger.Log("拆分成功。");
                }
            }

            Main.logger.Log("拆分失败：没有进入游戏存档，无法读取数据。");
        }

        /// <summary>
        /// 执行合并道具
        /// </summary>
        public static void RunningMergeItems()
        {
            if (!Main.enabled || Main.settings.mergeType.Count <= 0) return;

            DateFile df = DateFile.instance;
            Dictionary<int, int[]> items = new Dictionary<int, int[]>();
            Dictionary<int, int> itemsId = new Dictionary<int, int>();
            if (df.actorItemsDate.TryGetValue(df.mianActorId, out itemsId))
            {
                List<int> buffer = itemsId.Keys.ToList();
                foreach (int itemId in buffer)
                {
                    string id = df.GetItemDate(itemId, 999), surpluses = df.GetItemDate(itemId, 901), limit = df.GetItemDate(itemId, 902),
                        level = df.GetItemDate(itemId, 8), type = df.GetItemDate(itemId, 5);

                    if (!Main.settings.mergeType.Contains(int.Parse(type))) continue;
                    if ((type == "37" || type == "41") && !Main.settings.itemLevel.Contains(int.Parse(level))) continue;
                    if (Main.settings.enabledSize && int.Parse(df.itemsDate[itemId][902]) >= Main.settings.itemsSize) continue;
                    
                    if (items.ContainsKey(int.Parse(id)))
                    {
                        items[int.Parse(id)][0] += int.Parse(surpluses);
                        items[int.Parse(id)][1] += int.Parse(limit);
                    }
                    else
                    {
                        items.Add(int.Parse(id), new int[] { int.Parse(surpluses), int.Parse(limit) });
                    }
                    
                    df.LoseItem(df.mianActorId, itemId, itemsId[itemId], true);
                }
                
                foreach (KeyValuePair<int, int[]> item in items)
                {
                    if (Main.settings.enabledSize && Main.settings.itemsSize > 0)
                    {
                        int sizeCount = (item.Value[1] / int.Parse(Main.settings.itemsSize.ToString()));
                        int itemNaiJiu = item.Value[0], itemMaxNaiJiu = item.Value[1];
                        
                        for (int i = 0; i <= sizeCount; i++)
                        {
                            int makeItemId = df.MakeNewItem(item.Key);
                            df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                            if (itemNaiJiu > Main.settings.itemsSize)
                            {
                                df.itemsDate[makeItemId][901] = Main.settings.itemsSize.ToString();
                                df.itemsDate[makeItemId][902] = Main.settings.itemsSize.ToString();
                                itemNaiJiu -= int.Parse(Main.settings.itemsSize.ToString());
                                itemMaxNaiJiu -= int.Parse(Main.settings.itemsSize.ToString());
                            }
                            else
                            {
                                df.itemsDate[makeItemId][901] = itemNaiJiu.ToString();
                                df.itemsDate[makeItemId][902] = itemMaxNaiJiu >= Main.settings.itemsSize ? Main.settings.itemsSize.ToString() : itemMaxNaiJiu.ToString();
                            }
                        }
                    }
                    else
                    {
                        int makeItemId = df.MakeNewItem(item.Key);
                        df.GetItem(df.mianActorId, makeItemId, 1, false, -1, 0);
                        df.itemsDate[makeItemId][901] = item.Value[0].ToString();
                        df.itemsDate[makeItemId][902] = item.Value[1].ToString();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 判断是否时节结束时执行
    /// </summary>
    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public static class MergeItems_For_SaveDateFile_SaveSaveDate
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (Main.enabled && Main.settings.autoMerge && __instance.saveSaveDate) Main.RunningMergeItems();
        }
    }
}
