using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MoreFruit;

[BepInPlugin("Plugin.MoreFruit", "MoreFruit", "1.0.0")]
public class MoreFruit : BaseUnityPlugin
{
	private static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

	private static List<string> 植株丛List = new List<string> { "菠萝丛", "西红柿藤", "火龙果树", "荔枝树", "柠檬树", "奇异果树", "西瓜藤" };

	private static List<string> 果汁水果名称List = new List<string> { "菠萝", "柠檬", "番茄", "西瓜", "奇异果", "火龙果", "荔枝" };

	private static List<string> 果汁GuidList = new List<string> { "0b7c3c07ea56426f802fd9085f14687e", "8db3fb26c4fe4e9b9abc179dece7dfe3", "3a4e3ee642c744afb5e754f992861014", "3d6546503cf240319ce8eafc2c50e8f6", "32cb55a50f034433a52f1a0a18955f81", "f6d5033eeab44cfc9ccd92e2e18b7b70", "227c4fe3effc4af6900eb422da3e6eb4" };

	private static List<string> 水井水窖 = new List<string> { "f27eba0b8db0f5a4da4f552db987006c", "efcd6982c566b5c4ab2851da091618ab" };

	public static CardData 果汁模板;

	private static List<CardData> 果汁列表 = new List<CardData>();

	public static CardData 酿造台;

	public static CardData 酿造台_通电;

	public static List<CardData> 炉子列表 = new List<CardData>();

	private void Awake()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		Harmony val = new Harmony(((BaseUnityPlugin)this).Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod val2 = new HarmonyMethod(typeof(MoreFruit).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				val.Patch((MethodBase)method, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", new object[2]
			{
				"GameLoadLoadOptionsPostfix",
				ex.ToString()
			});
		}
		((BaseUnityPlugin)this).Logger.LogInfo((object)"Plugin MoreFruit is loaded!");
	}

	public static CardData utc(string uniqueID)
	{
		return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
	}

	public static void 生成果汁(string 水果名称, int 序号)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		CardData val = ScriptableObject.CreateInstance<CardData>();
		val = Object.Instantiate<CardData>(果汁模板);
		((UniqueIDScriptable)val).UniqueID = 果汁GuidList[序号];
		((Object)val).name = "Guil-更多水果_" + 水果名称 + "汁";
		((UniqueIDScriptable)val).Init();
		val.CardDescription.DefaultText = 水果名称 + "榨成的果汁，可以喝。";
		val.CardDescription.ParentObjectID = ((UniqueIDScriptable)val).UniqueID;
		Texture2D val2 = new Texture2D(200, 300);
		string path = Environment.CurrentDirectory + "\\BepInEx\\plugins\\Guil-更多水果\\Resource\\Picture\\" + 水果名称 + "汁.png";
		ImageConversion.LoadImage(val2, File.ReadAllBytes(path));
		Sprite cardImage = Sprite.Create(val2, new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), Vector2.zero);
		val.CardImage = cardImage;
		val.CardName.DefaultText = 水果名称 + "汁";
		val.CardName.ParentObjectID = ((UniqueIDScriptable)val).UniqueID;
		val.CardType = (CardTypes)0;
		GameLoad.Instance.DataBase.AllData.Add((UniqueIDScriptable)(object)val);
		果汁列表.Add(val);
	}

	private static void 添加榨汁(string 水果名称, int 序号, bool 是否通电)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		card_dict.TryGetValue("Guil-更多水果_" + 水果名称, out var value);
		if (!Object.op_Implicit((Object)(object)value))
		{
			Debug.Log((object)("没有get到水果——" + 水果名称));
			return;
		}
		LocalizedString val = default(LocalizedString);
		val.DefaultText = "榨汁";
		val.ParentObjectID = (是否通电 ? ((UniqueIDScriptable)酿造台_通电).UniqueID : ((UniqueIDScriptable)酿造台).UniqueID);
		val.LocalizationKey = "Guil-更多水果_榨汁";
		LocalizedString val2 = val;
		val = default(LocalizedString);
		val.DefaultText = "将水果榨成果汁";
		val.ParentObjectID = (是否通电 ? ((UniqueIDScriptable)酿造台_通电).UniqueID : ((UniqueIDScriptable)酿造台).UniqueID);
		LocalizedString val3 = val;
		CardOnCardAction val4 = new CardOnCardAction(val2, val3, (!是否通电) ? 1 : 0);
		Array.Resize(ref val4.CompatibleCards.TriggerCards, 1);
		val4.CompatibleCards.TriggerCards[0] = value;
		val4.GivenCardChanges.ModType = (CardModifications)2;
		if (序号 < 果汁列表.Count && Object.op_Implicit((Object)(object)果汁列表[序号]))
		{
			val4.GivenCardChanges.TransformInto = 果汁列表[序号];
		}
		else
		{
			Debug.Log((object)("没有果汁" + 序号));
		}
		((CardAction)val4).StackCompatible = true;
		GameStat fromID = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
		if ((Object)(object)fromID == (Object)null)
		{
			Debug.Log((object)"没有stat！");
		}
		else
		{
			StatModifier val5 = default(StatModifier);
			val5.Stat = fromID;
			StatModifier val6 = val5;
			val6.ValueModifier.x = 0.5f;
			val6.ValueModifier.y = 0.5f;
			Array.Resize(ref ((CardAction)val4).StatModifications, 1);
			((CardAction)val4).StatModifications[0] = val6;
		}
		if (!是否通电)
		{
			Array.Resize(ref 酿造台.CardInteractions, 酿造台.CardInteractions.Length + 1);
			酿造台.CardInteractions[酿造台.CardInteractions.Length - 1] = val4;
		}
		else
		{
			Array.Resize(ref 酿造台_通电.CardInteractions, 酿造台_通电.CardInteractions.Length + 1);
			酿造台_通电.CardInteractions[酿造台_通电.CardInteractions.Length - 1] = val4;
		}
	}

	private static void 添加烹饪(string 烹饪前guid, string 烹饪后guid, CardData 炉子)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		CardData val = utc(烹饪前guid);
		CardData droppedCard = utc(烹饪后guid);
		CookingRecipe val2 = new CookingRecipe();
		val2.ActionName.DefaultText = "烹饪";
		val2.ActionName.LocalizationKey = "Guil-更多水果_烹饪";
		Array.Resize(ref val2.CompatibleCards, 1);
		val2.CompatibleCards[0] = val;
		CardDrop val3 = default(CardDrop);
		val3.DroppedCard = droppedCard;
		val3.Quantity = new Vector2Int(1, 1);
		Array.Resize(ref val2.Drops, 1);
		val2.Drops[0] = val3;
		Traverse.Create((object)val2).Field("Duration").SetValue((object)2);
		GameStat fromID = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
		if ((Object)(object)fromID == (Object)null)
		{
			Debug.Log((object)"没有stat！");
		}
		else
		{
			StatModifier val4 = default(StatModifier);
			val4.Stat = fromID;
			StatModifier val5 = val4;
			val5.ValueModifier.x = 1f;
			val5.ValueModifier.y = 1f;
			Array.Resize(ref val2.StatModifications, 1);
			val2.StatModifications[0] = val5;
		}
		Array.Resize(ref 炉子.CookingRecipes, 炉子.CookingRecipes.Length + 1);
		炉子.CookingRecipes[炉子.CookingRecipes.Length - 1] = val2;
	}

	private static void 添加交互(string GivenGuid, string ReceiGuid, string ActionName, string ActionDescription, int duration, string speacal = "", string ProduceGuid = "")
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		CardData val = utc(GivenGuid);
		CardData val2 = utc(ReceiGuid);
		if (((Object)(object)val == (Object)null) | ((Object)(object)val2 == (Object)null))
		{
			return;
		}
		LocalizedString val3 = default(LocalizedString);
		val3.DefaultText = ActionName;
		val3.ParentObjectID = "";
		val3.LocalizationKey = "Guil-更多水果_Dummy";
		LocalizedString val4 = val3;
		val3 = default(LocalizedString);
		val3.DefaultText = ActionDescription;
		val3.ParentObjectID = "";
		val3.LocalizationKey = "Guil-更多水果_Dummy";
		LocalizedString val5 = val3;
		CardOnCardAction val6 = new CardOnCardAction(val4, val5, duration);
		Array.Resize(ref val6.CompatibleCards.TriggerCards, 1);
		val6.CompatibleCards.TriggerCards[0] = val;
		val6.GivenCardChanges.ModType = (CardModifications)3;
		if (ProduceGuid != "")
		{
			CardData val7 = utc(ProduceGuid);
			if ((Object)(object)val7 != (Object)null)
			{
				CardsDropCollection val8 = new CardsDropCollection();
				val8.CollectionName = "产出";
				val8.CollectionWeight = 1;
				CardDrop val9 = default(CardDrop);
				val9.DroppedCard = val7;
				val9.Quantity = new Vector2Int(1, 1);
				CardDrop[] value = (CardDrop[])(object)new CardDrop[1] { val9 };
				Traverse.Create((object)val8).Field("DroppedCards").SetValue((object)value);
				((CardAction)val6).ProducedCards = (CardsDropCollection[])(object)new CardsDropCollection[1] { val8 };
			}
		}
		if (speacal.IndexOf("减水") >= 0)
		{
			((CardAction)val6).ReceivingCardChanges.ModType = (CardModifications)1;
			((CardAction)val6).ReceivingCardChanges.ModifyLiquid = true;
			((CardAction)val6).ReceivingCardChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
		}
		if (val2.CardInteractions != null)
		{
			Array.Resize(ref val2.CardInteractions, val2.CardInteractions.Length + 1);
			val2.CardInteractions[val2.CardInteractions.Length - 1] = val6;
		}
	}

	public static void SomePatch()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Invalid comparison between Unknown and I4
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Invalid comparison between Unknown and I4
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Invalid comparison between Unknown and I4
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				Dictionary<string, CardData> dictionary = card_dict;
				string name = ((Object)GameLoad.Instance.DataBase.AllData[i]).name;
				UniqueIDScriptable obj = GameLoad.Instance.DataBase.AllData[i];
				dictionary[name] = (CardData)(object)((obj is CardData) ? obj : null);
			}
		}
		for (int j = 0; j < 植株丛List.Count; j++)
		{
			if (card_dict.TryGetValue("Guil-更多水果_" + 植株丛List[j], out var value))
			{
				((CardAction)value.DismantleActions[0]).UseMiniTicks = (MiniTicksBehavior)1;
			}
		}
		foreach (string item in 水井水窖)
		{
			CardData val = utc(item);
			Array.Resize(ref val.InventorySlots, 4);
			val.InventorySlotsText.DefaultText = "西瓜";
		}
		果汁模板 = utc("784f07839d6d11eda7cc047c16184f06");
		int num = 0;
		if ((Object)(object)果汁模板 != (Object)null)
		{
			foreach (string 果汁水果名称 in 果汁水果名称List)
			{
				生成果汁(果汁水果名称, num);
				num++;
			}
		}
		酿造台 = utc("cca3b9ca970c11eda154047c16184f06");
		酿造台_通电 = utc("c5e2c690a16c11ed8e65c475ab46ec3f");
		if (Object.op_Implicit((Object)(object)酿造台) && Object.op_Implicit((Object)(object)酿造台_通电))
		{
			for (int k = 0; k < 果汁水果名称List.Count; k++)
			{
				添加榨汁(果汁水果名称List[k], k, 是否通电: false);
				添加榨汁(果汁水果名称List[k], k, 是否通电: true);
			}
		}
		else
		{
			Debug.Log((object)"没有酿造台！");
		}
		foreach (KeyValuePair<string, CardData> item2 in card_dict)
		{
			CardData value2 = item2.Value;
			if ((Object)(object)value2 == (Object)null)
			{
				Debug.Log((object)("没有这卡：" + item2.Key));
			}
			else
			{
				if (!(((int)value2.CardType == 1) | ((int)value2.CardType == 2) | ((int)value2.CardType == 0)) || value2.CardTags == null || value2.CardTags.Length == 0)
				{
					continue;
				}
				CardTag[] cardTags = value2.CardTags;
				foreach (CardTag val2 in cardTags)
				{
					if ((Object)(object)val2 != (Object)null && ((Object)val2).name == "tag_Fire")
					{
						炉子列表.Add(value2);
						break;
					}
				}
			}
		}
		foreach (CardData item3 in 炉子列表)
		{
			添加烹饪("bd4900a0a0144ead834be820f8afac8a", "bf3d13d1872d4fd4b77a370e97c74aa4", item3);
		}
		添加交互("7ef9b514a20e11ed8801c475ab46ec3f", "871b363d97a511edb18250e085c43d2a", "混合", "制作柠檬茶", 0, "减水", "5481d599322f41d3b88249442ec4e8c0");
	}
}
