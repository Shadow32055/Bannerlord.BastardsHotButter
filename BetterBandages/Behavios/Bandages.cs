﻿using BetterCore.Utils;
using System;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using BetterBandages.Localization;
using TaleWorlds.Core;

namespace BetterBandages.Behaviors {
    public class Bandages : MissionBehavior {

        private static MissionTime bandageTime;
        private static MissionTime bleedInterval;
        private static MissionTime bleedDuration;

        private static bool activlyBandaging = false;
        private static bool isBleeding = false;
        private static int bleedStack;

        private int bandageCount;

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void AfterStart() {
            base.AfterStart();
            bandageCount = BetterBandages.Settings.BandageAmount;
        }

        public override void OnMissionTick(float dt) {
            base.OnMissionTick(dt);
            try {
                //Check if we are in a mission
                if (Mission.Current != null && Mission.Current.MainAgent != null) {

                    if (Mission.Current.MainAgent.Health > 0) {
                        if (activlyBandaging) {
                            if (IsMoving(Mission.Current.MainAgent.MovementVelocity)) {
                                NotifyHelper.ChatMessage(new TextObject(RefValues.CancelBandaging).ToString(), MsgType.Alert);
                                activlyBandaging = false;
                            }

                            if (bandageTime.IsPast) {
                                float healAmount = HealthHelper.GetMaxHealAmount(BetterBandages.Settings.BandageHealAmount, Mission.MainAgent);

                                Mission.Current.MainAgent.Health += healAmount;
                                bandageCount--;

                                if (isBleeding) {

                                    if (BetterBandages.Settings.BandageClearsBleedStack) {
                                        bleedStack = 0;
                                    } else {
                                        bleedStack--;
                                    }

                                    if (bleedStack == 0) {
                                        isBleeding = false;
                                        NotifyHelper.ChatMessage(new TextObject(RefValues.RemoveBleed).ToString(), MsgType.Good);
                                    } else {
                                        NotifyHelper.ChatMessage(new TextObject(RefValues.RemoveBleedStack).ToString(), MsgType.Good);
                                    }
                                }
                                activlyBandaging = false;

                                string text;
                                if (bandageCount < 2) {
                                    text = new TextObject(RefValues.BandageApplied) +  " " + bandageCount.ToString() + " " + new TextObject(RefValues.SingleBandLeft); ;
                                } else {
                                    text = new TextObject(RefValues.BandageApplied) + " " + bandageCount.ToString() + " " + new TextObject(RefValues.MultipleBandLeft);
                                }

                                NotifyHelper.ChatMessage(text, MsgType.Good);
                            }
                        }

                        if (Input.IsKeyPressed(BetterBandages.BandageKey)) {
                            UseBandage();
                        }

                        if (isBleeding && BetterBandages.Settings.BandageBleedEnabled) {
                            if (bleedInterval.IsPast) {
                                Mission.Current.MainAgent.Health -= ((BetterBandages.Settings.BandageBleedDamagePercent * bleedStack) * Mission.MainAgent.HealthLimit);

                                if (Mission.MainAgent.Health <= 0) {
                                    Mission.KillAgentCheat(Mission.MainAgent);
                                }

                                bleedInterval = MissionTime.SecondsFromNow(BetterBandages.Settings.BandageBleedInterval);
                            }
                        }

                        if (bleedDuration.IsPast) {
                            isBleeding = false;
                        }
                    }
                }
            } catch (Exception e) {
                NotifyHelper.ReportError(BetterBandages.ModName, "Problem with bandages, cause: " + e);
            }
        }

        private static bool IsMoving(Vec2 velocity) {
            Vec2 vec = Vec2.Zero - new Vec2(1, 1);
            Vec2 vec2 = Vec2.Zero + new Vec2(1, 1);

            return velocity.x < vec.x || velocity.x > vec2.x || velocity.y < vec.y || velocity.y > vec2.y;
        }

        public void UseBandage() {
            if (!IsMoving(Mission.Current.MainAgent.MovementVelocity)) {
                if (bandageCount > 0) {
                    if (!activlyBandaging) {
                        if (Mission.Current.MainAgent.Health != Mission.Current.MainAgent.HealthLimit) {
                            bandageTime = MissionTime.SecondsFromNow(BetterBandages.Settings.BandageTime);
                            activlyBandaging = true;
                            NotifyHelper.ChatMessage(new TextObject(RefValues.ApplyingBandage) + " " + BetterBandages.Settings.BandageTime.ToString() + " " + new TextObject(RefValues.SecondsText), MsgType.Good);
                        } else {
                            NotifyHelper.ChatMessage(new TextObject(RefValues.FullHealth) + " " + bandageCount.ToString() + " " + new TextObject(RefValues.BandageText), MsgType.Alert);
                        }
                    } else {
                        NotifyHelper.ChatMessage(new TextObject(RefValues.AlreadyApplying).ToString(), MsgType.Alert);
                    }
                } else {
                    NotifyHelper.ChatMessage(new TextObject(RefValues.OutOfBandages).ToString(), MsgType.Alert);
                }
            } else {
                NotifyHelper.ChatMessage(new TextObject(RefValues.CantBandage).ToString(), MsgType.Alert);
            }
        }
        public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty) {
            base.OnScoreHit(affectedAgent, affectorAgent, attackerWeapon, isBlocked, isSiegeEngineHit, blow, collisionData, damagedHp, hitDistance, shotDifficulty);
        
       
            try {
                if (BetterBandages.Settings.BandageBleedEnabled) {
                    if (affectedAgent == null) {
                        return;
                    }

                    if (isBlocked)
                        return;

                    if (affectedAgent == Mission.MainAgent) {
                        //NotifyHelper.ChatMessage("Considering bleed", MsgType.Risk);
                        if (HealthHelper.GetHealthPercentage(affectedAgent) < BetterBandages.Settings.BandageBleedTiggerThreshold) {
                            if (MathHelper.RandomChance(BetterBandages.Settings.BandageBleedChance)) {
                                bleedInterval = MissionTime.SecondsFromNow(BetterBandages.Settings.BandageBleedInterval);
                                bleedDuration = MissionTime.SecondsFromNow(BetterBandages.Settings.BandageBleedDuration);
                                isBleeding = true;

                                if (BetterBandages.Settings.BandageBleedStackEnabled) {
                                    if (bleedStack < BetterBandages.Settings.BandageBleedStackSize) {
                                        bleedStack++;
                                    }
                                } else {
                                    bleedStack = 1;
                                }
                                NotifyHelper.ChatMessage(bleedStack.ToString() + new TextObject(RefValues.ApplyBleedMsg) + " " + BetterBandages.Settings.BandageBleedDuration.ToString() + " " + new TextObject(RefValues.SecondsText), MsgType.Warning);
                            }
                        }
                    }
                }

            } catch (Exception e){
                NotifyHelper.ReportError(BetterBandages.ModName, "OnAgentHit threw exception " + e);
            }


        }
    }
}
