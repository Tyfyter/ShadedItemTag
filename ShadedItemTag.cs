using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ShadedItemTag
{
	public class ShadedItemTag : Mod {
		public override void Load() {
			LocalizedText newTranslation = Language.GetOrRegister("Mods.ShadedItemTag.TooltipTag");
			typeof(LocalizedText).GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newTranslation, "si");
			ChatManager.Register<ShadedItemTagHandler>(new string[]{
				"si",
				"shadeditem"
			});
		}
	}
	public class ShadedItemTagHandler : ITagHandler {
		public class ShadedItemSnippet : TextSnippet {
			public Item _item;
			public ShadedItemSnippet(Item item) {
				_item = item;
			}

			public override void OnHover() {
				Main.HoverItem = _item.Clone();
				Main.instance.MouseText(_item.Name, _item.rare, 0);
			}

			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f) {
				float num = 1f;
				float num2 = 1f;
				if (Main.netMode != NetmodeID.Server && !Main.dedServ) {
					Main.instance.LoadItem(_item.type);
					Texture2D value = TextureAssets.Item[_item.type].Value;
					if (Main.itemAnimations[_item.type] != null) {
						Main.itemAnimations[_item.type].GetFrame(value);
					} else {
						value.Frame();
					}
				}
				num2 *= scale;
				num *= num2;
				if (num > 0.75f) {
					num = 0.75f;
				}

				if (!justCheckingString && color != Color.Black) {
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = scale * num;
					ItemSlot.Draw(spriteBatch, ref _item, 14, position - new Vector2(10f) * scale * num, color);
					Main.inventoryScale = inventoryScale;
				}
				size = new Vector2(32f) * scale * num;
				return true;
			}

			public override float GetStringLength(DynamicSpriteFont font) {
				return 32f * Scale * 0.65f;
			}
		}

		TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
			Item item = new Item();
			if (int.TryParse(text, out var result) && result < ItemLoader.ItemCount) {
				item.netDefaults(result);
			}
			if (ItemID.Search.TryGetId(text, out result)) {
				item.netDefaults(result);
			}
			if (item.type <= 0) {
				return new TextSnippet(text);
			}
			item.stack = 1;
			if (options != null) {
				string[] array = options.Split(',');
				for (int i = 0; i < array.Length; i++) {
					if (array[i].Length == 0) {
						continue;
					}
					switch (array[i][0]) {
						case 'd':
						item = ItemIO.FromBase64(array[i].Substring(1));
						break;
						case 's':
						case 'x': {
							if (int.TryParse(array[i].Substring(1), out var result3)) {
								item.stack = Utils.Clamp(result3, 1, item.maxStack);
							}
							break;
						}
						case 'p': {
							if (int.TryParse(array[i].Substring(1), out var result2)) {
								item.Prefix((byte)Utils.Clamp(result2, 0, PrefixLoader.PrefixCount));
							}
							break;
						}
					}
				}
			}
			string str = "";
			if (item.stack > 1) {
				str = " (" + item.stack + ")";
			}
			return new ShadedItemSnippet(item) {
				Text = "[" + item.AffixName() + str + "]",
				CheckForHover = true,
				DeleteWhole = true
			};
		}

		public static string GenerateTag(Item I) {
			string str = "[si";
			if (I.ModItem != null || GlobalTypeLookups<GlobalItem>.GetGlobalsForType(I.type).Length > 0) {
				str = str + "/d" + ItemIO.ToBase64(I);
			} else {
				if (I.prefix != 0) {
					str = str + "/p" + I.prefix;
				}
				if (I.stack != 1) {
					str = str + "/s" + I.stack;
				}
			}
			return str + ":" + I.netID + "]";
		}
	}
}