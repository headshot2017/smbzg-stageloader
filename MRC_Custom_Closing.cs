using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SMBZG.MovementRush.Cinematic
{
    public class MRC_Custom_Closing : IMovementRushTransitionCinematic
    {
        public GameObject Prefab_SimpleCharacter;

        private List<GameObject> GeneratedObjectList = new List<GameObject>();

        private Coroutine Coroutine_Cinematic;

        private bool _IsDirectionOfRushRight;

        private void CleanUp()
        {
            while (GeneratedObjectList.Count > 0)
            {
                if (GeneratedObjectList[0] != null)
                {
                    Destroy(GeneratedObjectList[0]);
                }

                GeneratedObjectList.RemoveAt(0);
            }
        }

        public override void PlayCinematic(bool isDirectionOfRushRight, CharacterControl Attacker, CharacterControl Victim)
        {
            BattleBackgroundManager BackgroundManager = (BattleBackgroundManager)typeof(BattleController).GetField("BackgroundManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);

            _IsDirectionOfRushRight = isDirectionOfRushRight;
            if (Coroutine_Cinematic != null)
            {
                BackgroundManager.StopCoroutine(Coroutine_Cinematic);
                CleanUp();
            }

            Coroutine_Cinematic = BackgroundManager.StartCoroutine(CinematicWork(Attacker, Victim));
        }

        private IEnumerator CinematicWork(CharacterControl Attacker, CharacterControl Victim)
        {
            BattleBackgroundManager BackgroundManager = (BattleBackgroundManager)typeof(BattleController).GetField("BackgroundManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            MovementRushManager MovementRushManager = (MovementRushManager)typeof(BattleController).GetField("MovementRushManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            BattleCameraManager CameraManager = (BattleCameraManager)typeof(BattleController).GetField("CameraManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BattleController.instance);
            StageData ActiveStage = (StageData)typeof(BattleBackgroundManager).GetField("ActiveStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);

            int faceDir = (_IsDirectionOfRushRight ? 1 : (-1));
            Vector3 position = Victim.CharacterGO.transform.position;
            Vector2 groundPositionViaRaycast = Victim.CharacterGO.GetGroundPositionViaRaycast();
            bool isAirRush = MovementRushManager.ActiveMovementRush.MovementRushType == MovementRushManager.MovementRushTypeENUM.Air;
            if (isAirRush)
            {
                BattleBackgroundData ActiveBattleBackgroundData = ActiveStage.BattleBackgroundDataList.GetRandom();
                typeof(BattleBackgroundManager).GetMethod("SetGroundLevel", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(float) }, null).Invoke(BackgroundManager, new object[] { Victim.transform.position.y - 50f });
                typeof(BattleBackgroundManager).GetMethod("FloorCollider_SetActive", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(bool) }, null).Invoke(BackgroundManager, new object[] { true });
                typeof(BattleBackgroundManager).GetMethod("TransitionBattleBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(BattleBackgroundData), typeof(float) }, null).Invoke(BackgroundManager, new object[] { ActiveBattleBackgroundData, 1f });
                typeof(BattleBackgroundManager).GetMethod("SpeedLines_SetRotation", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(float) }, null).Invoke(BackgroundManager, new object[] { -45 * faceDir + ((!_IsDirectionOfRushRight) ? 180 : 0) });
                AudioSource sfx_whitleDrop = SoundCache.ins.PlaySound(SoundCache.ins.Battle_WhistleDrop);
                typeof(BaseCharacter).GetProperty("HitStun", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, 80f);
                Victim.CharacterGO.ResetGravity();
                typeof(BaseCharacter).GetField("DragOverride", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, null);
                Victim.CharacterGO.SetVelocity(30 * faceDir, -20f);
                typeof(BaseCharacter).GetField("IsFacingRight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, _IsDirectionOfRushRight);
                CameraManager.SetTargetGroup(Victim.transform);
                bool victimHitTheFloor = false;
                Victim.CharacterGO.AddOnContactGroundWhileHurtCallback(delegate
                {
                    victimHitTheFloor = true;
                });
                float elapsed2 = 0f;
                float prevElapsed2 = 0f;
                while (!victimHitTheFloor)
                {
                    if (prevElapsed2 < 0.25f && 0.25f <= elapsed2)
                    {
                        Attacker.UnloadCharacterObject();
                    }

                    prevElapsed2 = elapsed2;
                    yield return null;
                    elapsed2 += BattleController.instance.ActorDeltaTime;
                }

                if (sfx_whitleDrop != null)
                {
                    sfx_whitleDrop.Stop();
                }

                SoundCache.ins.PlaySound(SoundCache.ins.Battle_land_explosively);
                CameraManager.SetShake(0.5f, 0.3f);
                Vector2 groundPositionViaRaycast2 = Victim.CharacterGO.GetGroundPositionViaRaycast();
                EffectSprite effectSprite = EffectSprite.Create(groundPositionViaRaycast2, EffectSprite.Sprites.DustExplosion, isFacingRight: true, 1f, 0f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true);
                effectSprite.transform.localScale = effectSprite.transform.localScale * 2f;
                DustPoofEffect.Create(effectSprite.transform.position + Vector3.left, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: true, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 3f, 0.25f);
                DustPoofEffect.Create(effectSprite.transform.position, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: false, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 5f, 0.25f);
                DustPoofEffect.Create(effectSprite.transform.position + Vector3.right, DustPoofEffect.Animations.DustPoof_Gray, isFacingRight: true, 3f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true, 3f, 0.25f);
                if (ActiveBattleBackgroundData.Prefab_Crater != null)
                {
                    GameObject crater = Instantiate(ActiveBattleBackgroundData.Prefab_Crater, groundPositionViaRaycast2, Quaternion.identity);
                    crater.SetActive(true);
                }

                typeof(BaseCharacter).GetProperty("HitStun", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, 2f);
                Victim.CharacterGO.SetVelocity(0f, -5f);
                typeof(BaseCharacter).GetField("IsFacingRight", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, _IsDirectionOfRushRight);
                BattleController.instance.SetBackgroundTreadmillEffect(BattleController.BackgroundTreadmillEffectENUM.Default);
                CameraManager.SetShake(1f, 0.25f);
                CameraManager.SetFocusZoom(4f);
                yield return new BattleController.WaitForActorSeconds(1.5f);
                CameraManager.SetFocusZoom(null);
                CameraManager.SetMovementSpeed(30f);
            }
            else
            {
                BattleBackgroundData ActiveBattleBackgroundData = (BattleBackgroundData)typeof(BattleBackgroundManager).GetField("ActiveBattleBackgroundData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);

                BattleController.instance.SetBackgroundTreadmillEffect(BattleController.BackgroundTreadmillEffectENUM.Default);
                Victim.UnloadCharacterObject();
                Color color = Color.black;
                Color endColor = Color.white;
                if (Victim != null)
                {
                    color = Victim.ParticipantDataReference.CurrentCharacterData.PrimaryColor;
                    endColor = Victim.ParticipantDataReference.CurrentCharacterData.SecondaryColor;
                }

                SimpleCharacterSwooshEffect simpleCharacterSwoosh = Instantiate(Prefab_SimpleCharacter, position, Quaternion.identity, null).GetComponent<SimpleCharacterSwooshEffect>();
                simpleCharacterSwoosh.Comp_SpriteRenderer.color = color;
                simpleCharacterSwoosh.Comp_SpriteRenderer.sortingLayerID = Helpers.SortingLayer.Default;
                simpleCharacterSwoosh.Comp_TrailRenderer.startColor = color;
                simpleCharacterSwoosh.Comp_TrailRenderer.endColor = endColor;
                simpleCharacterSwoosh.Comp_TrailRenderer.sortingLayerID = Helpers.SortingLayer.Default;
                GeneratedObjectList.Add(simpleCharacterSwoosh.gameObject);
                CameraManager.SetTargetGroup(Attacker.CharacterGO.transform, simpleCharacterSwoosh.transform);
                Vector3 startPos = simpleCharacterSwoosh.transform.position;
                Vector3 endPos = new Vector3(startPos.x + (float)(15 * faceDir), groundPositionViaRaycast.y, startPos.z);
                float prevElapsed2 = 0f;
                for (float elapsed2 = 0.1f; prevElapsed2 < elapsed2; prevElapsed2 += BattleController.instance.ActorDeltaTime)
                {
                    simpleCharacterSwoosh.transform.position = Vector3.Lerp(startPos, endPos, prevElapsed2 / elapsed2);
                    yield return null;
                }

                simpleCharacterSwoosh.transform.position = endPos;
                simpleCharacterSwoosh.Comp_SpriteRenderer.enabled = false;
                yield return null;
                Victim.transform.position = simpleCharacterSwoosh.transform.position;
                Victim.InstantiateCharacterObject();
                typeof(BaseCharacter).GetProperty("HitStun", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Victim.CharacterGO, 2f);
                if (Victim.ParticipantDataReference.Health.GetCurrent() <= 0f)
                {
                    Victim.CharacterGO.OnDeath();
                }

                SoundCache.ins.PlaySound(SoundCache.ins.Battle_land_explosively);
                CameraManager.SetShake(0.5f, 0.3f);
                EffectSprite.Create(simpleCharacterSwoosh.transform.position, EffectSprite.Sprites.DustExplosion, isFacingRight: true, 1f, 0f, AnimateUsingUnscaledTime: false, AlwaysAppearAboveFighters: true);
                if (ActiveBattleBackgroundData.Prefab_Crater != null)
                {
                    GameObject crater = Instantiate(ActiveBattleBackgroundData.Prefab_Crater, Victim.CharacterGO.GetGroundPositionViaRaycast(), Quaternion.identity);
                    crater.SetActive(true);
                }

                CameraManager.SetTargetGroup(Victim.transform);
                yield return null;
                Victim.CharacterGO.SetVelocity(15 * faceDir, 10f);
                Attacker.UnloadCharacterObject();
                yield return new BattleController.WaitForActorSeconds(1f);
            }

            MovementRushManager.MRDataStore.ParticipantStatus participantStatus = MovementRushManager.ActiveMovementRush.PlayerStatusesList.Find((MovementRushManager.MRDataStore.ParticipantStatus p) => p.Participant.ParticipantIndex == Attacker.ParticipantDataReference.ParticipantIndex);
            foreach (MovementRushManager.MRDataStore.ParticipantStatus playerStatuses in MovementRushManager.ActiveMovementRush.PlayerStatusesList)
            {
                CharacterControl characterControlByParticipantIndex = BattleController.instance.GetCharacterControlByParticipantIndex(playerStatuses.Participant.ParticipantIndex);
                if (characterControlByParticipantIndex.CharacterGO == null)
                {
                    characterControlByParticipantIndex.InstantiateCharacterObject();
                }

                characterControlByParticipantIndex.InputLockTimer = 1f;
                characterControlByParticipantIndex.CharacterGO.SetDefaultGravityDrag(isMovementRushing: false);
                if (!(characterControlByParticipantIndex == Victim))
                {
                    Vector2 vector = (isAirRush ? new Vector2(-15 * faceDir, 6f) : new Vector2(-10 * faceDir, 6f));
                    Vector2 vector2 = (isAirRush ? new Vector2(10 * faceDir, 6f) : new Vector2(10 * faceDir, 6f));

                    float GroundPositionY = (float)typeof(BattleBackgroundManager).GetField("GroundPositionY", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BackgroundManager);

                    if (playerStatuses.IsRushInitiatingAggressor == participantStatus.IsRushInitiatingAggressor)
                    {
                        characterControlByParticipantIndex.CharacterGO.transform.position = new Vector3(Victim.transform.position.x + vector.x, GroundPositionY + vector.y, characterControlByParticipantIndex.CharacterGO.transform.position.z);
                        characterControlByParticipantIndex.CharacterGO.SetVelocity(vector2.x, vector2.y);
                    }
                    else
                    {
                        characterControlByParticipantIndex.CharacterGO.transform.position = new Vector3(Victim.transform.position.x + vector.x * -1f, GroundPositionY + vector.y, characterControlByParticipantIndex.CharacterGO.transform.position.z);
                        characterControlByParticipantIndex.CharacterGO.SetVelocity(vector2.x * -1f, vector2.y);
                    }
                }
            }

            CameraManager.SetTargetGroup_Default();
            yield return new BattleController.WaitForActorSeconds(0.5f);
            MovementRushManager.DeactivateMovementRush();
            yield return new BattleController.WaitForActorSeconds(0.5f);
            CleanUp();
        }
    }
}