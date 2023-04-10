using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using LocalizationUtilities;

namespace MoreFruit
{
[BepInPlugin("Plugin.MoreFruit", "MoreFruit", "1.0.0")]
public class MoreFruit : BaseUnityPlugin
{

	private void Awake()
	{
        LocalizationStringUtility.Init(
            Config.Bind<bool>("Debug", "LogCardInfo", false, "If true, will output the localization keys for the cards")
                .Value,
            Info.Location,
            Logger
        );
            var harmony = new Harmony(this.Info.Metadata.GUID);
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            try
            {
                var load = new HarmonyMethod(typeof(MoreFruit).GetMethod("SomePatch"));
                var method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingFlags);
                if (method == null)
                {
                    method = typeof(GameLoad).GetMethod("LoadGameData", bindingFlags);
                }
                if (method != null)
                    harmony.Patch(method, postfix: load);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
            }
            

            //Plugin startup logic
            //Harmony.CreateAndPatchAll(typeof(MoreFruit));
            Logger.LogInfo("Plugin MoreFruit is loaded!");
        }
        static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();
		
        //修改采摘时间List
		static List<String> 植株丛List = new List<String>{
            "菠萝丛",
            "西红柿藤",
            "火龙果树",
            "荔枝树",
            "柠檬树",
            "奇异果树",
            "西瓜藤",
        };
        //果汁列表
		static List<String> 果汁水果名称List = new List<String>
		{
			"菠萝",
			"柠檬",
			"番茄",
			"西瓜",
			"奇异果",
			"火龙果",
			"荔枝",
		};
        //果汁GUID
        static List<String> 果汁GuidList = new List<String>
        {
            "0b7c3c07ea56426f802fd9085f14687e",//1
            "8db3fb26c4fe4e9b9abc179dece7dfe3",//2
            "3a4e3ee642c744afb5e754f992861014",//3
            "3d6546503cf240319ce8eafc2c50e8f6",//4
            "32cb55a50f034433a52f1a0a18955f81",//5
            "f6d5033eeab44cfc9ccd92e2e18b7b70",//6
            "227c4fe3effc4af6900eb422da3e6eb4",//7
        };

		static List<String> 水井水窖 = new List<String>
		{
			"f27eba0b8db0f5a4da4f552db987006c",//水井
			"efcd6982c566b5c4ab2851da091618ab"//水窖
		};
		public static CardData utc(String uniqueID)
	{
		return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
	}

		//添加果汁卡
        public static CardData 果汁模板;
		static List<CardData> 果汁列表 = new List<CardData>();
		public static void 生成果汁(String 水果名称,int 序号)
        {

			CardData 果汁 = ScriptableObject.CreateInstance<CardData>();
			果汁 = Instantiate(果汁模板);
			果汁.UniqueID = 果汁GuidList[序号];;
			果汁.name = "Guil-更多水果_" + 水果名称 + "汁";
			果汁.Init();
			果汁.CardDescription.DefaultText = 水果名称 + "榨成的果汁，可以喝。";
            果汁.CardDescription.SetLocalizationInfo();	
            果汁.CardDescription.ParentObjectID = 果汁.UniqueID;

			Texture2D texture2D = new Texture2D(200, 300); 
            string text4 = System.Environment.CurrentDirectory + "\\BepInEx\\plugins\\Guil-更多水果\\Resource\\Picture\\" + 水果名称 + "汁.png";
			texture2D.LoadImage(File.ReadAllBytes(text4));
			Sprite sp = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero);
			果汁.CardImage = sp;

			果汁.CardName.DefaultText = 水果名称 + "汁";
			果汁.CardName.ParentObjectID = 果汁.UniqueID;
            
			果汁.CardType = CardTypes.Item;

            GameLoad.Instance.DataBase.AllData.Add(果汁);
			
			果汁列表.Add(果汁);
	}

		//添加到酿造台
		public static CardData 酿造台;
		public static CardData 酿造台_通电;
        private static void 添加榨汁(String 水果名称,int 序号,bool 是否通电)
        {
            card_dict.TryGetValue("Guil-更多水果_" + 水果名称, out CardData 水果);

			if (!水果) {
                Debug.Log("没有get到水果——" + 水果名称);
                return;
            }

			LocalizedString name1 = new LocalizedString{
				DefaultText = "榨汁",
				ParentObjectID = 是否通电 ? 酿造台_通电.UniqueID : 酿造台.UniqueID,
				//Todo:  This changed to a direct key
				//localizedString.SetLocalizationInfo();

				LocalizationKey = "Guil-更多水果_榨汁"
		    };
			LocalizedString name2 = new LocalizedString{
				DefaultText = "将水果榨成果汁",
				ParentObjectID = 是否通电 ? 酿造台_通电.UniqueID : 酿造台.UniqueID
			};
			name2.SetLocalizationInfo();

			CardOnCardAction action = new CardOnCardAction(name1, name2, 是否通电?0:1);
			Array.Resize(ref action.CompatibleCards.TriggerCards, 1);
            action.CompatibleCards.TriggerCards[0] = 水果;
            action.GivenCardChanges.ModType = CardModifications.Transform;
            if (序号 < 果汁列表.Count && 果汁列表[序号]) {
				action.GivenCardChanges.TransformInto = 果汁列表[序号];
			}
            else {
                Debug.Log("没有果汁"+ 序号.ToString());
            }
			action.StackCompatible = true;
			
			GameStat stat01 = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
		    if (stat01 == null) {
				Debug.Log("没有stat！");
		    }			
            else {
                StatModifier DLFH = new StatModifier
                {
                    Stat = stat01
				};
				DLFH.ValueModifier.x = 0.5f;
                DLFH.ValueModifier.y = 0.5f;
				
				Array.Resize(ref action.StatModifications, 1);				
				action.StatModifications[0] = DLFH;     
                
			}
			if (是否通电 == false) {
			    Array.Resize(ref 酿造台.CardInteractions, 酿造台.CardInteractions.Length + 1);
			    酿造台.CardInteractions[酿造台.CardInteractions.Length - 1] = action;
		    }
		    else {
			    Array.Resize(ref 酿造台_通电.CardInteractions, 酿造台_通电.CardInteractions.Length + 1);
			    酿造台_通电.CardInteractions[酿造台_通电.CardInteractions.Length - 1] = action;
		    }
		
	}

		//添加烹饪
		public static List<CardData> 炉子列表 = new List<CardData>();
		private static void 添加烹饪(String 烹饪前guid, string 烹饪后guid, CardData 炉子) {
			CardData 烹饪前 = utc(烹饪前guid);
			CardData 烹饪后 = utc(烹饪后guid);

			CookingRecipe cr = new CookingRecipe();
			cr.ActionName.DefaultText = "烹饪";
			cr.ActionName.LocalizationKey = "Guil-更多水果_烹饪";
			Array.Resize(ref cr.CompatibleCards, 1);
			cr.CompatibleCards[0] = 烹饪前;

			CardDrop cd = new CardDrop();
			cd.DroppedCard = 烹饪后;
			cd.Quantity = new Vector2Int(1, 1);
			Array.Resize(ref cr.Drops, 1);
			cr.Drops[0] = cd;

			Traverse.Create(cr).Field("Duration").SetValue(2);

			GameStat stat01 = UniqueIDScriptable.GetFromID<GameStat>("93c4433b9c1640343b87576d355e53b4");
			if (stat01 == null) {
				Debug.Log("没有stat！");
			}
			else {
				StatModifier DLFH = new StatModifier
				{
					Stat = stat01
				};
				DLFH.ValueModifier.x = 1f;
				DLFH.ValueModifier.y = 1f;
				Array.Resize(ref cr.StatModifications, 1);
				cr.StatModifications[0] = DLFH;
			}

		Array.Resize(ref 炉子.CookingRecipes, 炉子.CookingRecipes.Length + 1);
			炉子.CookingRecipes[炉子.CookingRecipes.Length - 1] = cr;
	}

	private static void 添加交互(string GivenGuid, string ReceiGuid, string ActionName, string ActionDescription, int duration, string speacal = "", string ProduceGuid = "")
		{
			CardData GivenCard = utc(GivenGuid);
			CardData ReceiCard = utc(ReceiGuid);
			if(GivenCard == null | ReceiCard == null) { return; }

			LocalizedString name1 = new LocalizedString
			{
				DefaultText = ActionName,
				ParentObjectID = "",
			};
			
			name1.SetLocalizationInfo();
			
			LocalizedString name2 = new LocalizedString
			{
				DefaultText = ActionDescription,
				ParentObjectID = "",
			};
			
			name2.SetLocalizationInfo();
			
			CardOnCardAction action = new CardOnCardAction(name1, name2, duration);

			Array.Resize(ref action.CompatibleCards.TriggerCards, 1);
			action.CompatibleCards.TriggerCards[0] = GivenCard;

			action.GivenCardChanges.ModType = CardModifications.Destroy;
			if (ProduceGuid != "") {
				CardData ProduceCard = utc(ProduceGuid);
				if (ProduceCard != null) {
					CardsDropCollection cdc = new CardsDropCollection();
					cdc.CollectionName = "产出";
					cdc.CollectionWeight = 1;

					CardDrop cd = new CardDrop();
					cd.DroppedCard = ProduceCard;
					cd.Quantity = new Vector2Int(1, 1);
					CardDrop[] cds = new CardDrop[] { cd };
					Traverse.Create(cdc).Field("DroppedCards").SetValue(cds);

					action.ProducedCards = new CardsDropCollection[] { cdc };
				}			
			}
			if(speacal.IndexOf("减水")  >= 0) {
				action.ReceivingCardChanges.ModType = CardModifications.DurabilityChanges;
				action.ReceivingCardChanges.ModifyLiquid = true;
				action.ReceivingCardChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
			}

			if(ReceiCard.CardInteractions != null) {
				Array.Resize(ref ReceiCard.CardInteractions, ReceiCard.CardInteractions.Length + 1);
				ReceiCard.CardInteractions[ReceiCard.CardInteractions.Length - 1] = action;
			}
	}

		public static void SomePatch()
		{
			for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++) {
				if (GameLoad.Instance.DataBase.AllData[i] is CardData) {
					card_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as CardData;
				}
			}
			//修改采摘时间
			for (int m = 0; m < 植株丛List.Count; m++) {
				if (card_dict.TryGetValue("Guil-更多水果_" + 植株丛List[m], out CardData shu)) {
					shu.DismantleActions[0].UseMiniTicks = MiniTicksBehavior.CostsAMiniTick;
				}
			}

			//水井、水窖增加冰瓜槽
			foreach (string uid in 水井水窖) {
				CardData shui = utc(uid);
				Array.Resize(ref shui.InventorySlots, 4);
				shui.InventorySlotsText.DefaultText = "西瓜";
				shui.InventorySlotsText.LocalizationKey = "watermelon";
			}
			//生成更多果汁
			果汁模板 = utc("784f07839d6d11eda7cc047c16184f06");
			int x = 0;
			if (果汁模板 != null) {
				foreach (string 果汁水果名称 in 果汁水果名称List) {
					生成果汁(果汁水果名称, x);
					x++;
				}
			}

			//添加到酿造台
			酿造台 = utc("cca3b9ca970c11eda154047c16184f06");
			酿造台_通电 = utc("c5e2c690a16c11ed8e65c475ab46ec3f");
			if (酿造台 && 酿造台_通电) {
				for (int m = 0; m < 果汁水果名称List.Count; m++) {
					添加榨汁(果汁水果名称List[m], m, false);
					添加榨汁(果汁水果名称List[m], m, true);
				}
			}
			else {
				Debug.Log("没有酿造台！");
			}

			//添加炉子列表
			foreach (KeyValuePair<string, CardData> kvp in card_dict) {
				CardData card = kvp.Value;
				if (card == null) {
					Debug.Log("没有这卡：" + kvp.Key);
				}
				else {

					if (card.CardType == CardTypes.Base | card.CardType == CardTypes.Location | card.CardType == CardTypes.Item) {
						if (card.CardTags != null) {
							if (card.CardTags.Length > 0) {
								foreach (CardTag tag in card.CardTags) {
									if (tag != null) {
										if (tag.name == "tag_Fire") {
											炉子列表.Add(card);
											break;
										}
									}
								}
							}
						}
					}
				}
			}
			//对每个炉子列表加东西
			foreach(CardData 炉子 in 炉子列表) {
				添加烹饪("bd4900a0a0144ead834be820f8afac8a", "bf3d13d1872d4fd4b77a370e97c74aa4", 炉子);
			}

			添加交互("7ef9b514a20e11ed8801c475ab46ec3f", "871b363d97a511edb18250e085c43d2a", "混合", "制作柠檬茶", 0, "减水", "5481d599322f41d3b88249442ec4e8c0");

						
		}
	}
}
