using UMA;
using UnityEngine;

namespace Static_Interface.API.PlayerFramework.ModelImpl
{
    public class UMAModelController : PlayerModelController
    {
        public static UMAGeneratorBase Generator;
        public static SlotLibrary SlotLibrary;
        public static OverlayLibrary OverlayLibrary;
        public static RaceLibrary RaceLibrary;
        public static GameObject UMAObject;
        private const int NumberOfSlots = 20;

        public static void Init()
        {
            UMAObject = Object.Instantiate(Resources.Load<GameObject>("UMA"));
            Generator = UMAObject.GetComponentInChildren<UMAGenerator>();
            SlotLibrary = UMAObject.GetComponentInChildren<SlotLibrary>();
            OverlayLibrary = UMAObject.GetComponentInChildren<OverlayLibrary>();
            RaceLibrary = UMAObject.GetComponentInChildren<RaceLibrary>();

            RegisterPlayerModelInternal(null, new UMAModelController());
        }

        public override string Name { get; protected set; } = "UMA";

        protected override PlayerModel LoadModel(Player player)
        {
            GameObject uma = new GameObject();
            var dynamicAvatar = uma.AddComponent<UMADynamicAvatar>();
            dynamicAvatar.Initialize();

            var umaData = dynamicAvatar.umaData;
            umaData.umaGenerator = Generator;

            umaData.umaRecipe.slotDataList = new SlotData[NumberOfSlots];

            var dna = new UMADnaHumanoid();
            var tutorialDna = new UMADnaTutorial();
            umaData.umaRecipe.AddDna(dna);
            umaData.umaRecipe.AddDna(tutorialDna);

            CreateMale(umaData);

            dynamicAvatar.UpdateNewRace();
            var coll = uma.AddComponent<CapsuleCollider>();
            coll.height = umaData.characterHeight;
            coll.radius = umaData.characterRadius;
            coll.center = new Vector3(0, 0.8f, 0);
            var rigid = uma.AddComponent<Rigidbody>();
            rigid.freezeRotation = true;

            rigid.mass = umaData.characterMass;
            UMAModel model = player.gameObject.AddComponent<UMAModel>();
            model.InternalModel = uma;
            return model;
        }

        private void CreateMale(UMAData umaData)
        {
            var umaRecipe = umaData.umaRecipe;
            umaRecipe.SetRace(RaceLibrary.GetRace("HumanMale"));

            umaData.umaRecipe.slotDataList[0] = SlotLibrary.InstantiateSlot("MaleFace");
            umaData.umaRecipe.slotDataList[0].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleHead02"));

            umaData.umaRecipe.slotDataList[1] = SlotLibrary.InstantiateSlot("MaleEyes");
            umaData.umaRecipe.slotDataList[1].AddOverlay(OverlayLibrary.InstantiateOverlay("EyeOverlay"));

            umaData.umaRecipe.slotDataList[2] = SlotLibrary.InstantiateSlot("MaleInnerMouth");
            umaData.umaRecipe.slotDataList[2].AddOverlay(OverlayLibrary.InstantiateOverlay("InnerMouth"));

            umaData.umaRecipe.slotDataList[3] = SlotLibrary.InstantiateSlot("MaleTorso");
            umaData.umaRecipe.slotDataList[3].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleBody02"));

            umaData.umaRecipe.slotDataList[4] = SlotLibrary.InstantiateSlot("MaleHands");
            umaData.umaRecipe.slotDataList[4].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleBody02"));

            umaData.umaRecipe.slotDataList[5] = SlotLibrary.InstantiateSlot("MaleLegs");
            umaData.umaRecipe.slotDataList[5].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleBody02"));

            umaData.umaRecipe.slotDataList[6] = SlotLibrary.InstantiateSlot("MaleFeet");
            umaData.umaRecipe.slotDataList[6].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleBody02"));

            umaData.umaRecipe.slotDataList[3].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleUnderwear01"));
            umaData.umaRecipe.slotDataList[5].AddOverlay(OverlayLibrary.InstantiateOverlay("MaleUnderwear01"));

            //todo: hair
            //umaData.umaRecipe.slotDataList[7] = SlotLibrary.InstantiateSlot("");
            //umaData.umaRecipe.slotDataList[7].AddOverlay(OverlayLibrary.InstantiateOverlay(""));
        }
    }
}