using System.Linq;

using UnityEngine;
using UnityEditor;

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using Wenzil.Console;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace AzMonsterPack
{
    public class NewSprites : MonoBehaviour
    {
        private static Mod mod;

        public const int DeathbringerCareerIndex = 241;


        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<NewSprites>();

            ConsoleCommandsDatabase.RegisterCommand("export_texture_properties", "Exports the properties for all the records of the texture as XML", "EXPORT_TEXTURE_PROPERTIES <id> <out_id>", ExportTextureProperties);

            mod.IsReady = true;
        }

        private void Awake()
        {
            QuestMachine.Instance.FoesTable.AddIntoTable(new string[] { "241, Deathbringer" });

            List<MobileEnemy> enemies = EnemyBasics.Enemies.ToList();
            enemies.Add(new MobileEnemy()
            {
                ID = DeathbringerCareerIndex,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1000,
                FemaleTexture = 1000,
                CorpseTexture = EnemyBasics.CorpseTexture(405, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyDaedraLordMove,
                BarkSound = (int)SoundClips.EnemyOrcWarlordBark,
                AttackSound = (int)SoundClips.EnemyOrcWarlordAttack,
                ParrySounds = true,
                MapChance = 2,
                LootTableKey = "Q",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 40,
                PrimaryAttackAnimFrames2 = new int[] { 4, -1, 5, 0 },
                HasSpellAnimation = false,
                Team = MobileTeams.Magic,
            });

            DaggerfallEntity.RegisterCustomCareerTemplate(DeathbringerCareerIndex, new DFCareer()
            {
                Name = "Deathbringer",
                HitPointsPerLevel = 40,
                Strength = 90,
                Intelligence = 80,
                Willpower = 80,
                Agility = 50,
                Endurance = 70,
                Personality = 40,
                Speed = 60,
                Luck = 70,
                PrimarySkill1 = DFCareer.Skills­.LongBlade,
                PrimarySkill2 = DFCareer.Skills.Alteration,
                PrimarySkill3 = DFCareer.Skills.Destruction,
                MajorSkill1 = DFCareer.Skills.Restoration,
                MajorSkill2 = DFCareer.Skills.Dragonish,
                MajorSkill3 = DFCareer.Skills.Daedric,
                MinorSkill1 = DFCareer.Skills.Dodging,
                MinorSkill2 = DFCareer.Skills.Medical,
                MinorSkill3 = DFCareer.Skills.Mysticism,
                MinorSkill4 = DFCareer.Skills.Streetwise,
                MinorSkill5 = DFCareer.Skills.BluntWeapon,
                HumanoidAttackModifier = DFCareer.AttackModifier.Bonus,
                ForbiddenMaterials = DFCareer.MaterialFlags.Silver | DFCareer.MaterialFlags.Elven | DFCareer.MaterialFlags.Adamantium,
                ForbiddenShields = DFCareer.ShieldFlags.TowerShield | DFCareer.ShieldFlags.KiteShield | DFCareer.ShieldFlags.RoundShield,
                ForbiddenArmors = DFCareer.ArmorFlags.Leather,
                SpellPointMultiplier = DFCareer.SpellPointMultipliers.Times_3_00,
                SpellPointMultiplierValue = 3.0f,
                Regeneration = DFCareer.RegenerationFlags.Always,
                AcuteHearing = true,
                AdrenalineRush = true,
            });

            EnemyBasics.Enemies = enemies.ToArray();

            EnemyEntity.OnLootSpawned += OnEnemySpawned;
        }

        void OnEnemySpawned(object sender, EnemyLootSpawnedEventArgs args)
        {
            var enemyEntity = sender as EnemyEntity;
            if (enemyEntity == null)
                return;

            switch(enemyEntity.CareerIndex)
            {
                case DeathbringerCareerIndex:
                    SetupDeathbringer(enemyEntity, args);
                    break;
            }     
        }

        static byte[] DeathbringerSpells = 
        {
            0x34, // Vampiric Touch (15-30)
            0x0A, // Free Action
            0x07, // Wizard's Fire (13-27)
            0x1C, // Far Silence (50% for 3 rounds)
            0x16, // Spell Shield (50% for 3 rounds)
            0x61 // Balyna's Balm (5-15)
        };

        static byte[] EnemyClass1_2 =
        {
            0x03, // Frostbite (8-14)
            0x08, // Shock (11-25)
            0x61, // Balyna's Balm (5-15)
        };

        static byte[] EnemyClass3_5 =
        {
            0x03, // Frostbite (8-14)
            0x08, // Shock (11-25)
            0x61, // Balyna's Balm (5-15)
            0x07, // Wizard's Fire (13-27)
        };

        static byte[] EnemyClass6_8 =
        {
            0x08, // Shock (11-25)
            0x61, // Balyna's Balm (5-15)
            0x07, // Wizard's Fire (13-27)
            0x10, // Ice Bolt (25-59)
            0x1D, // Toxic Cloud (25-49)
        };

        static byte[] EnemyClass9_11 =
        {
            0x61, // Balyna's Balm (5-15)
            0x07, // Wizard's Fire (13-27)
            0x10, // Ice Bolt (25-59)
            0x1D, // Toxic Cloud (25-49)
            0x1F, // Lightning (35-58)
        };

        static byte[] EnemyClass12_14 =
        {
            0x07, // Wizard's Fire (13-27)
            0x10, // Ice Bolt (25-59)
            0x1D, // Toxic Cloud (25-49)
            0x1F, // Lightning (35-58)
            0x0A, // Free Action
        };

        static byte[] EnemyClass15_17 =
        {
            0x10, // Ice Bolt (25-59)
            0x1D, // Toxic Cloud (25-49)
            0x1F, // Lightning (35-58)
            0x0A, // Free Action
            0x0E, // Fireball (52-85)
        };

        static byte[] EnemyClass18 =
        {
            0x1D, // Toxic Cloud (25-49)
            0x1F, // Lightning (35-58)
            0x0A, // Free Action
            0x0E, // Fireball (52-85)
            0x14, // Ice Storm (52-81)
        };

        static byte[][] EnemyClassSpells =
        {
            EnemyClass1_2, // 1-2
            EnemyClass3_5, // 3-5
            EnemyClass6_8, // 6-8
            EnemyClass9_11, // 9-11
            EnemyClass12_14, // 12-14
            EnemyClass15_17, // 15-17
            EnemyClass18 // 18+
        };

        void SetupDeathbringer(EnemyEntity enemyEntity, EnemyLootSpawnedEventArgs args)
        {
            var career = args.EnemyCareer;

            var player = GameManager.Instance.PlayerEntity;
            var level = player.Level;

            enemyEntity.Level = level;
            enemyEntity.MaxHealth = FormulaHelper.RollEnemyClassMaxHealth(level, career.HitPointsPerLevel);

            DaggerfallUnityItem weapon = ItemBuilder.CreateWeapon(Weapons.Claymore, RandomDeathbringerWeaponMaterial(level));
            enemyEntity.ItemEquipTable.EquipItem(weapon, alwaysEquip: true, playEquipSounds: false);
            enemyEntity.Items.AddItem(weapon);

            AddDeathbringerArmor(enemyEntity, Armor.Cuirass);
            AddDeathbringerArmor(enemyEntity, Armor.Greaves);
            AddDeathbringerArmor(enemyEntity, Armor.Boots);
            AddDeathbringerArmor(enemyEntity, Armor.Helm);
            AddDeathbringerArmor(enemyEntity, Armor.Left_Pauldron);
            AddDeathbringerArmor(enemyEntity, Armor.Right_Pauldron);

            int spellTable = Mathf.Clamp(level / 3 + 1, 0, 6);

            enemyEntity.SetEnemySpells(DeathbringerSpells);
        }

        WeaponMaterialTypes RandomDeathbringerWeaponMaterial(int level)
        {
            WeaponMaterialTypes type = FormulaHelper.RandomMaterial(level);
            switch(type)
            {
                case WeaponMaterialTypes.Steel:
                case WeaponMaterialTypes.Dwarven:
                case WeaponMaterialTypes.Mithril:
                case WeaponMaterialTypes.Orcish:
                    return WeaponMaterialTypes.Ebony;
                case WeaponMaterialTypes.Daedric:
                default:
                    return type;
            }
        }

        void AddDeathbringerArmor(EnemyEntity enemyEntity, Armor armor)
        {
            var player = GameManager.Instance.PlayerEntity;

            DaggerfallUnityItem item = ItemBuilder.CreateArmor(player.Gender, player.Race, armor, ArmorMaterialTypes.Silver);
            enemyEntity.ItemEquipTable.EquipItem(item, alwaysEquip: true, playEquipSounds: false);
            enemyEntity.Items.AddItem(item);
            enemyEntity.UpdateEquippedArmorValues(item, equipping: true);
        }

        static string ExportTextureProperties(string[] args)
        {
            if (args.Length < 2)
                return "usage: EXPORT_TEXTURE_PROPERTIES <id> <out_id>";

            if (!int.TryParse(args[0], out int inputArchive))
                return "error: <id> must be an integer";

            TextureFile texture = new TextureFile();
            if (!texture.Load(Path.Combine(DaggerfallUnity.Instance.Arena2Path, TextureFile.IndexToFileName(inputArchive)), FileUsage.UseMemory, true))
                return $"error: Could not open archive {inputArchive}";

            // Ensure writeable
            Directory.CreateDirectory(mod.PersistentDataDirectory);
            Directory.CreateDirectory(Path.Combine(mod.PersistentDataDirectory, "Textures"));

            int recordCount = texture.RecordCount;
            for(int record = 0; record < recordCount; ++record)
            {
                DFSize scale = texture.GetScale(record);
                DFPosition offset = texture.GetOffset(record);

                bool hasCustomScale = scale.Width != 0 || scale.Height != 0;
                bool hasCustomOffset = offset.X != 0 || offset.Y != 0;
                if (hasCustomScale || hasCustomOffset)
                {
                    string xmlPath = Path.Combine(mod.PersistentDataDirectory, "Textures", $"{args[1]}_{record}-0.xml");

                    using (FileStream outXml = new FileStream(xmlPath, FileMode.OpenOrCreate))
                    using (XmlWriter xmlWriter = XmlWriter.Create(outXml))
                    {
                        xmlWriter.WriteStartElement("info");

                        if (hasCustomScale)
                        {
                            float normX = 1 + scale.Width / 256.0f;
                            float normY = 1 + scale.Height / 256.0f;

                            
                            xmlWriter.WriteElementString("scaleX", normX.ToString());
                            xmlWriter.WriteElementString("scaleY", normY.ToString());
                            
                        }

                        if(hasCustomOffset)
                        {
                            xmlWriter.WriteElementString("offsetX", offset.X.ToString());
                            xmlWriter.WriteElementString("offsetY", offset.Y.ToString());
                        }

                        xmlWriter.WriteEndElement();
                    }
                }
            }

            return $"Properties exported to {mod.PersistentDataDirectory}";
        }
            

    }
}
