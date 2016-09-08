using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
namespace CoreDebugger
{
    internal static class CoreDebugger
    {
        private static Menu _myMenu;
        private static readonly Dictionary<int, int> Counters = new Dictionary<int, int>();

        private static bool EntityManager
        {
            get { return _myMenu["EntityManager"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool MyDamageStats
        {
            get { return _myMenu["MyDamageStats"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool IsValidTarget
        {
            get { return _myMenu["IsValidTarget"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool BuffInstance
        {
            get { return _myMenu["BuffInstance"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool TargetDamageStats
        {
            get { return _myMenu["TargetDamageStats"].Cast<CheckBox>().CurrentValue; }
        }
        private static bool StreamingMode
        {
            get { return _myMenu["StreamingMode"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool HealthPrediction
        {
            get { return _myMenu["HealthPrediction"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool CheckPrediction
        {
            get { return _myMenu["Prediction"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool CheckSpellbook
        {
            get { return _myMenu["Spellbook"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool CheckMissileClient
        {
            get { return _myMenu["MissileClient"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool CheckOrbwalker
        {
            get { return _myMenu["Orbwalker"].Cast<CheckBox>().CurrentValue; }
        }

        private static void Main()
        {
            Loading.OnLoadingComplete += delegate { Initialize(); };
        }

        private static void Initialize()
        {
            _myMenu = MainMenu.AddMenu("CoreDebugger", "CoreDebugger");
            _myMenu.AddGroupLabel("General");
            _myMenu.Add("MyDamageStats", new CheckBox("My damage stats", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("TargetDamageStats", new CheckBox("Target damage stats", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("EntityManager", new CheckBox("EntityManager properties", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("HealthPrediction", new CheckBox("HealthPrediction properties", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("IsValidTarget", new CheckBox("IsValidTarget properties", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("BuffInstance", new CheckBox("BuffInstance properties", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("Prediction", new CheckBox("Prediction", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("StreamingMode", new CheckBox("Streaming Mode", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("Spellbook", new CheckBox("Spellbook", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("MissileClient", new CheckBox("MissileClient", false)).OnValueChange += OnOnValueChange;
            _myMenu.Add("Orbwalker", new CheckBox("Orbwalker", false)).OnValueChange += OnOnValueChange;
            _myMenu["StreamingMode"].Cast<CheckBox>().CurrentValue = false;
            _myMenu.AddGroupLabel("AutoAttack");
            _myMenu.Add("autoAttackDamage", new CheckBox("Print autoattack damage")).OnValueChange += OnOnValueChange;
            foreach (var value in _myMenu.LinkedValues.Values.Select(i => i as CheckBox).Where(i => i != null))
            {
                value.CurrentValue = false;
            }
            var autoAttacking = false;
            AttackableUnit.OnDamage += delegate (AttackableUnit sender, AttackableUnitDamageEventArgs args)
            {
                if (args.Source.IsMe)
                {
                    var baseTarget = args.Target as Obj_AI_Base;
                    if (baseTarget != null)
                    {
                        if (autoAttacking)
                        {
                            if (_myMenu["autoAttackDamage"].Cast<CheckBox>().CurrentValue)
                            {
                                Chat.Print("Real Damage: " + args.Damage + ", SDK Damage: " + Player.Instance.GetAutoAttackDamage(baseTarget, true));
                            }
                            autoAttacking = false;
                        }
                    }
                }
            };
            Obj_AI_Base.OnBasicAttack += delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
            {
                if (sender.IsMe)
                {
                    autoAttacking = true;
                }
            };
            Player.OnPostIssueOrder += delegate (Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
            {
                if (sender.IsMe)
                {
                    if (StreamingMode)
                    {
                        Hud.ShowClick(args.Target != null ? ClickType.Attack : ClickType.Move, args.TargetPosition);
                    }
                }
            };
            Drawing.OnEndScene += delegate
            {
                Counters.Clear();
                if (MyDamageStats)
                {
                    DrawText(Player.Instance,
                        "TotalAttackDamage: " + Player.Instance.TotalAttackDamage + ", PercentArmorPenetrationMod: " + Player.Instance.PercentArmorPenetrationMod + ", FlatArmorPenetrationMod: " +
                        Player.Instance.FlatArmorPenetrationMod + ", PercentBonusArmorPenetrationMod: " + Player.Instance.PercentBonusArmorPenetrationMod);
                    DrawText(Player.Instance,
                        "TotalMagicalDamage: " + Player.Instance.TotalMagicalDamage + ", PercentMagicPenetrationMod: " + Player.Instance.PercentMagicPenetrationMod + ", FlatMagicPenetrationMod: " +
                        Player.Instance.FlatMagicPenetrationMod);
                    DrawText(Player.Instance, "Crit: " + Player.Instance.Crit + ", FlatCritChanceMod: " + Player.Instance.FlatCritChanceMod);
                }
                if (CheckOrbwalker)
                {
                    DrawText(Player.Instance, "CanAttack: " + Player.Instance.CanAttack);
                    DrawText(Player.Instance, "IsChanneling: " + Player.Instance.Spellbook.IsChanneling);
                    DrawText(Player.Instance, "IsAutoAttacking: " + Player.Instance.Spellbook.IsAutoAttacking);
                    DrawText(Player.Instance, "CastEndTime: " + Player.Instance.Spellbook.CastEndTime);
                }
                if (IsValidTarget)
                {
                    var targets = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValid && i.VisibleOnScreen);
                    foreach (var target in targets)
                    {
                        DrawText(target, "IsValidTarget: " + target.IsValidTarget());
                        DrawText(target, "IsDead: " + target.IsDead);
                        DrawText(target, "IsVisible: " + target.IsVisible);
                        DrawText(target, "IsTargetable: " + target.IsTargetable);
                        DrawText(target, "IsInvulnerable: " + target.IsInvulnerable);
                        DrawText(target, "IsHPBarRendered: " + target.IsHPBarRendered);
                    }
                }
                if (EntityManager)
                {
                    var targets = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValidTarget() && i.VisibleOnScreen);
                    foreach (var target in targets)
                    {
                        DrawText(target, "Type: " + target.Type);
                        DrawText(target, "BaseSkinName: " + target.BaseSkinName);
                        DrawText(target, "Team: " + target.Team);
                        DrawText(target, "IsEnemy: " + target.IsEnemy);
                        DrawText(target, "TotalShieldHealth: " + target.TotalShieldHealth());
                        DrawText(target, "HPRegenRate: " + target.HPRegenRate);
                        DrawText(target, "MaxHealth: " + target.MaxHealth);
                        if (target is Obj_AI_Minion)
                        {
                            DrawText(target, "IsMinion: " + target.IsMinion);
                            DrawText(target, "IsMonster: " + target.IsMonster);
                        }
                    }
                }
                if (HealthPrediction)
                {
                    var targets = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValidTarget() && i.IsAlly && i.VisibleOnScreen && (i is Obj_AI_Minion || i is Obj_AI_Turret));
                    foreach (var target in targets)
                    {
                        DrawText(target, "IsRanged: " + target.IsRanged);
                        DrawText(target, "Health: " + target.Health);
                        DrawText(target, "TotalAttackDamage: " + target.TotalAttackDamage);
                        DrawText(target, "AttackCastDelay: " + target.AttackCastDelay);
                        DrawText(target, "AttackDelay: " + target.AttackDelay);
                        DrawText(target, "MissileSpeed: " + target.BasicAttack.MissileSpeed);
                        if (target is Obj_AI_Minion)
                        {
                            DrawText(target, "PercentDamageToBarracksMinionMod: " + target.PercentDamageToBarracksMinionMod);
                            DrawText(target, "FlatDamageReductionFromBarracksMinionMod: " + target.FlatDamageReductionFromBarracksMinionMod);
                        }
                    }
                    DrawText(Player.Instance, "Ping: " + Game.Ping);
                }
                if (TargetDamageStats)
                {
                    foreach (var target in ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValid && i.VisibleOnScreen))
                    {
                        DrawText(target, "Armor: " + target.Armor + ", SpellBlock: " + target.SpellBlock + ", BaseArmor: " + target.CharData.Armor);
                    }
                }
                if (BuffInstance)
                {
                    foreach (var target in ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValid))
                    {
                        foreach (var buff in target.Buffs)
                        {
                            var endTime = Math.Max(0, buff.EndTime - Game.Time);
                            var stringEndTime = endTime > 1000 ? "Infinite" : Convert.ToString(endTime, CultureInfo.InvariantCulture);
                            DrawText(target,
                                "IsActive : " + buff.IsActive + ", IsValid: " + buff.IsValid + ", HasBuff: " + target.HasBuff(buff.DisplayName) + ", Type: " + buff.Type + ", Name: " + buff.Name +
                                ", DisplayName: " + buff.DisplayName + ", Count: " +
                                buff.Count + (!string.IsNullOrEmpty(buff.SourceName) ? ", SourceName: " + buff.SourceName : "") + ", Caster: " + buff.Caster.Name +
                                (buff.Caster is Obj_AI_Base ? ", CasterBaseSkinName: " + ((Obj_AI_Base)buff.Caster).BaseSkinName : "") + ", RemainingTime: " +
                                stringEndTime);
                        }
                    }
                }
                if (CheckPrediction)
                {
                    var targets = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValidTarget() && i.VisibleOnScreen);
                    foreach (var target in targets)
                    {
                        DrawText(target, "IsMoving: " + target.IsMoving);
                        DrawText(target, "PathLength: " + target.Path.Length);
                        DrawText(target, "BoundingRadius: " + target.BoundingRadius);
                        DrawText(target, "MovementBlockedDebuffDuration: " + Math.Max(0f, target.GetMovementBlockedDebuffDuration()));
                        DrawText(target, "CastEndTimeLeft: " + Math.Max(0f, target.Spellbook.CastEndTime - Game.Time));
                        DrawText(target, "CastTimeLeft: " + Math.Max(0f, target.Spellbook.CastTime - Game.Time));
                        DrawText(target, "IsChanneling: " + target.Spellbook.IsChanneling);
                    }
                }
                if (CheckSpellbook)
                {
                    var targets = ObjectManager.Get<Obj_AI_Base>().Where(i => i.IsValidTarget() && i.VisibleOnScreen);
                    foreach (var target in targets)
                    {
                        foreach (var spell in target.Spellbook.Spells)
                        {
                            if (spell != null && !spell.Name.Contains("Unknown") && !spell.Name.Contains("BaseSpell"))
                            {
                                DrawText(target, "Name: " + spell.Name + ", Slot: " + spell.Slot + ", State: " + spell.State + ", ToggleState: " + spell.ToggleState + ", Level: " + spell.Level + ", Cooldown: " + spell.Cooldown + ", CooldownExpires: " + Math.Max(0f, spell.CooldownExpires - Game.Time) + ", Ammo: " + spell.Ammo + ", CastRange: " + spell.SData.CastRange + ", CastRangeDisplayOverride: " + spell.SData.CastRangeDisplayOverride);
                            }
                        }
                    }
                }
                if (CheckMissileClient)
                {
                    var missiles = ObjectManager.Get<MissileClient>().Where(i => i.IsValid && !i.IsDead);
                    foreach (var missile in missiles)
                    {
                        DrawText(missile, "Slot: " + missile.Slot);
                        var caster = missile.SpellCaster;
                        if (caster != null)
                        {
                            DrawText(missile, "SpellCaster: " +caster.BaseSkinName);
                        }
                        var target = missile.Target as Obj_AI_Base;
                        var targetIsValid = target != null;
                        if (targetIsValid)
                        {
                            DrawText(missile, "Target: " + target.BaseSkinName);
                        }
                        DrawText(missile, "Name: " + missile.SData.Name);
                        DrawText(missile, "StartPosition: " + missile.StartPosition);
                        DrawText(missile, "EndPosition: " + missile.EndPosition);
                        var missileTravelFixed = missile.SData.MissileFixedTravelTime;
                        if (missileTravelFixed > 0)
                        {
                            DrawText(missile, "MissileFixedTravelTime: " + missile.SData.MissileFixedTravelTime);
                        }
                        else
                        {
                            DrawText(missile, "MissileSpeed: " + missile.SData.MissileSpeed);
                            if (missile.SData.LineWidth > 0)
                            {
                                DrawText(missile, "LineWidth: " + missile.SData.LineWidth);
                            }
                        }

                    }
                }
            };
        }

        private static void OnOnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (args.NewValue)
            {
                foreach (var value in _myMenu.LinkedValues.Values.Select(i => i as CheckBox).Where(i => i != null))
                {
                    if (sender.SerializationId != value.SerializationId)
                    {
                        value.CurrentValue = false;
                    }
                }
            }
        }

        private static void DrawText(GameObject target, string text)
        {
            if (!Counters.ContainsKey(target.NetworkId))
            {
                Counters.Add(target.NetworkId, 0);
            }
            else
            {
                Counters[target.NetworkId]++;
            }
            var targetPosition = new Vector2(0, 30 + Counters[target.NetworkId] * 18) + target.Position.WorldToScreen();
            Drawing.DrawText(targetPosition, System.Drawing.Color.AliceBlue, text, 10);
        }
    }
}
