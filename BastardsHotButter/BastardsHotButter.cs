using Bastards.AddonHelpers;
using BetterCore.Utils;
using HotButter;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BastardsHotButter {
    public class BastardsHotButter : MBSubModuleBase {

        public static string ModName { get; private set; } = "BastardsHotButter";
        private bool isLoaded = false;

        //FIRST
        protected override void OnSubModuleLoad() {
            try {
                base.OnSubModuleLoad();

                BastardCampaignEvents.AddAction_OnPlayerBastardConceptionAttempt(delegate (Hero otherHero)
                {
                    MBInformationManager.ShowSceneNotification(new PompaSceneNotificationItem(Hero.MainHero.IsFemale ? otherHero : Hero.MainHero, Hero.MainHero.IsFemale ? Hero.MainHero : otherHero, Hero.MainHero.PartyBelongedTo.LastVisitedSettlement, delegate (Hero hero1, bool someBool, Hero hero2)
                    {
                    }, PompaTipi.PAVYON, null));
                });
            } catch (Exception e) {
                NotifyHelper.WriteError(ModName, "OnSubModuleLoad threw exception " + e);
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot() {
            try {
                base.OnBeforeInitialModuleScreenSetAsRoot();

                if (isLoaded)
                    return;

                ModName = base.GetType().Assembly.GetName().Name;

                NotifyHelper.WriteMessage(ModName + " Loaded.", MsgType.Good);

                isLoaded = true;
            } catch (Exception e) {
                NotifyHelper.WriteError(ModName, "OnBeforeInitialModuleScreenSetAsRoot threw exception " + e);
            }
        }
    }
}
