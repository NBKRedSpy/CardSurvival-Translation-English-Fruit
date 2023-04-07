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
		Harmony harmony = new Harmony(base.Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod postfix = new HarmonyMethod(typeof(MoreFruit).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				harmony.Patch(method, null, postfix);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
		}
		base.Logger.LogInfo("Plugin MoreFruit is loaded!");
	}

	public static CardData utc(string uniqueID)
	{
		return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
	}

	public static void 生成果汁(string 水果名称, int 序号)
	{
		CardData cardData = ScriptableObject.CreateInstance<CardData>();
		cardData = UnityEngine.Object.Instantiate(果汁模板);
		cardData.UniqueID = 果汁GuidList[序号];
		cardData.name = "Guil-更多水果_" + 水果名称 + "汁";
		cardData.Init();
		cardData.CardDescription.DefaultText = 水果名称 + "榨成的果汁，可以喝。";
		cardData.CardDescription.ParentObjectID = cardData.UniqueID;
		Texture2D texture2D = new Texture2D(200, 300);
		string path = Environment.CurrentDirectory + "\\BepInEx\\plugins\\Guil-更多水果\\Resource\\Picture\\" + 水果名称 + "汁.png";
		texture2D.LoadImage(File.ReadAllBytes(path));
		Sprite cardImage = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
		cardData.CardImage = cardImage;
		cardData.CardName.DefaultText = 水果名称 + "汁";
		cardData.CardName.ParentObjectID = cardData.UniqueID;
		cardData.CardType = CardTypes.Item;
		GameLoad.Instance.DataBase.AllData.Add(cardData);
		果汁列表.Add(cardData);
	}

	private static void 添加榨汁(string 水果名称, int 序号, bool 是否通电)
	{
		card_dict.TryGetValue("Guil-更多水果_" + 水果名称, out var value);
		if (!value)
		{
			Debug.Log("没有get到水果——" + 水果名称);
			return;
		}
		LocalizedString localizedString = default(LocalizedString);
		localizedString.DefaultText = "榨汁";
		localizedString.ParentObjectID = (是否通电 ? 酿造台_通电.UniqueID : 酿造台.UniqueID);
		localizedString.LocalizationKey = "Guil-更多水果_榨汁";
		LocalizedString localizedString2 = localizedString;
		localizedString = default(LocalizedString);
		localizedString.DefaultText = "将水果榨成果汁";
		localizedString.ParentObjectID = (是否通电 ? 酿造台_通电.UniqueID : 酿造台.UniqueID);
		LocalizedString desc = localizedString;
		CardOnCardAction cardOnCardAction = new CardOnCardAction(localizedString2, desc, (!是否通电) ? 1 : 0);
		Array.Resize(ref cardOnCardAction.CompatibleCards.TriggerCards, 1);
		cardOnCardAction.CompatibleCards.TriggerCards[0] = value;
		cardOnCardAction.GivenCardChanges.ModType = CardModifications.Transform;
		if (序号 < 果汁列表.Count && (bool)果汁列表[序号])
		{
			cardOnCardAction.GivenCardChanges.TransformInto = 果汁列表[序号];
		}
		else
		{
			Debug.Log("没有果汁" + 序号);
		}
		cardOnCardAction.StackCompatible = true;
		GameStat fromID = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
		if (fromID == null)
		{
			Debug.Log("没有stat！");
		}
		else
		{
			StatModifier statModifier = default(StatModifier);
			statModifier.Stat = fromID;
			StatModifier statModifier2 = statModifier;
			statModifier2.ValueModifier.x = 0.5f;
			statModifier2.ValueModifier.y = 0.5f;
			Array.Resize(ref cardOnCardAction.StatModifications, 1);
			cardOnCardAction.StatModifications[0] = statModifier2;
		}
		if (!是否通电)
		{
			Array.Resize(ref 酿造台.CardInteractions, 酿造台.CardInteractions.Length + 1);
			酿造台.CardInteractions[酿造台.CardInteractions.Length - 1] = cardOnCardAction;
		}
		else
		{
			Array.Resize(ref 酿造台_通电.CardInteractions, 酿造台_通电.CardInteractions.Length + 1);
			酿造台_通电.CardInteractions[酿造台_通电.CardInteractions.Length - 1] = cardOnCardAction;
		}
	}

	private static void 添加烹饪(string 烹饪前guid, string 烹饪后guid, CardData 炉子)
	{
		CardData cardData = utc(烹饪前guid);
		CardData droppedCard = utc(烹饪后guid);
		CookingRecipe cookingRecipe = new CookingRecipe();
		cookingRecipe.ActionName.DefaultText = "烹饪";
		cookingRecipe.ActionName.LocalizationKey = "Guil-更多水果_烹饪";
		Array.Resize(ref cookingRecipe.CompatibleCards, 1);
		cookingRecipe.CompatibleCards[0] = cardData;
		CardDrop cardDrop = default(CardDrop);
		cardDrop.DroppedCard = droppedCard;
		cardDrop.Quantity = new Vector2Int(1, 1);
		Array.Resize(ref cookingRecipe.Drops, 1);
		cookingRecipe.Drops[0] = cardDrop;
		Traverse.Create(cookingRecipe).Field("Duration").SetValue(2);
		GameStat fromID = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
		if (fromID == null)
		{
			Debug.Log("没有stat！");
		}
		else
		{
			StatModifier statModifier = default(StatModifier);
			statModifier.Stat = fromID;
			StatModifier statModifier2 = statModifier;
			statModifier2.ValueModifier.x = 1f;
			statModifier2.ValueModifier.y = 1f;
			Array.Resize(ref cookingRecipe.StatModifications, 1);
			cookingRecipe.StatModifications[0] = statModifier2;
		}
		Array.Resize(ref 炉子.CookingRecipes, 炉子.CookingRecipes.Length + 1);
		炉子.CookingRecipes[炉子.CookingRecipes.Length - 1] = cookingRecipe;
	}

	private static void 添加交互(string GivenGuid, string ReceiGuid, string ActionName, string ActionDescription, int duration, string speacal = "", string ProduceGuid = "")
	{
		CardData cardData = utc(GivenGuid);
		CardData cardData2 = utc(ReceiGuid);
		if ((cardData == null) | (cardData2 == null))
		{
			return;
		}
		LocalizedString localizedString = default(LocalizedString);
		localizedString.DefaultText = ActionName;
		localizedString.ParentObjectID = "";
		localizedString.LocalizationKey = "Guil-更多水果_Dummy";
		LocalizedString localizedString2 = localizedString;
		localizedString = default(LocalizedString);
		localizedString.DefaultText = ActionDescription;
		localizedString.ParentObjectID = "";
		localizedString.LocalizationKey = "Guil-更多水果_Dummy";
		LocalizedString desc = localizedString;
		CardOnCardAction cardOnCardAction = new CardOnCardAction(localizedString2, desc, duration);
		Array.Resize(ref cardOnCardAction.CompatibleCards.TriggerCards, 1);
		cardOnCardAction.CompatibleCards.TriggerCards[0] = cardData;
		cardOnCardAction.GivenCardChanges.ModType = CardModifications.Destroy;
		if (ProduceGuid != "")
		{
			CardData cardData3 = utc(ProduceGuid);
			if (cardData3 != null)
			{
				CardsDropCollection cardsDropCollection = new CardsDropCollection();
				cardsDropCollection.CollectionName = "产出";
				cardsDropCollection.CollectionWeight = 1;
				CardDrop cardDrop = default(CardDrop);
				cardDrop.DroppedCard = cardData3;
				cardDrop.Quantity = new Vector2Int(1, 1);
				CardDrop[] value = new CardDrop[1] { cardDrop };
				Traverse.Create(cardsDropCollection).Field("DroppedCards").SetValue(value);
				cardOnCardAction.ProducedCards = new CardsDropCollection[1] { cardsDropCollection };
			}
		}
		if (speacal.IndexOf("减水") >= 0)
		{
			cardOnCardAction.ReceivingCardChanges.ModType = CardModifications.DurabilityChanges;
			cardOnCardAction.ReceivingCardChanges.ModifyLiquid = true;
			cardOnCardAction.ReceivingCardChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
		}
		if (cardData2.CardInteractions != null)
		{
			Array.Resize(ref cardData2.CardInteractions, cardData2.CardInteractions.Length + 1);
			cardData2.CardInteractions[cardData2.CardInteractions.Length - 1] = cardOnCardAction;
		}
	}

	public static void SomePatch()
	{
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				card_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as CardData;
			}
		}
		for (int j = 0; j < 植株丛List.Count; j++)
		{
			if (card_dict.TryGetValue("Guil-更多水果_" + 植株丛List[j], out var value))
			{
				value.DismantleActions[0].UseMiniTicks = MiniTicksBehavior.CostsAMiniTick;
			}
		}
		foreach (string item in 水井水窖)
		{
			CardData cardData = utc(item);
			Array.Resize(ref cardData.InventorySlots, 4);
			cardData.InventorySlotsText.DefaultText = "西瓜";
		}
		果汁模板 = utc("784f07839d6d11eda7cc047c16184f06");
		int num = 0;
		if (果汁模板 != null)
		{
			foreach (string 果汁水果名称 in 果汁水果名称List)
			{
				生成果汁(果汁水果名称, num);
				num++;
			}
		}
		酿造台 = utc("cca3b9ca970c11eda154047c16184f06");
		酿造台_通电 = utc("c5e2c690a16c11ed8e65c475ab46ec3f");
		if ((bool)酿造台 && (bool)酿造台_通电)
		{
			for (int k = 0; k < 果汁水果名称List.Count; k++)
			{
				添加榨汁(果汁水果名称List[k], k, 是否通电: false);
				添加榨汁(果汁水果名称List[k], k, 是否通电: true);
			}
		}
		else
		{
			Debug.Log("没有酿造台！");
		}
		foreach (KeyValuePair<string, CardData> item2 in card_dict)
		{
			CardData value2 = item2.Value;
			if (value2 == null)
			{
				Debug.Log("没有这卡：" + item2.Key);
			}
			else
			{
				if (!((value2.CardType == CardTypes.Base) | (value2.CardType == CardTypes.Location) | (value2.CardType == CardTypes.Item)) || value2.CardTags == null || value2.CardTags.Length == 0)
				{
					continue;
				}
				CardTag[] cardTags = value2.CardTags;
				foreach (CardTag cardTag in cardTags)
				{
					if (cardTag != null && cardTag.name == "tag_Fire")
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
