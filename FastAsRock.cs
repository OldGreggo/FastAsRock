using System;
using System.Reflection;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using Unity.Collections;
using Unity.IO;
using UnityEngine.XR;
using System.Collections.Generic;
using System.IO;

namespace FastAsRock
{
    [BepInPlugin("org.pepper.monkeytag.Rocks", "FastAsRock", "1.0.0.0")]
    [BepInProcess("Gorilla Tag.exe")]
    public class MonkePlugin : BaseUnityPlugin
    {
        private void Awake() => new Harmony("com.pepper.monkeytag.Rocks").PatchAll(Assembly.GetExecutingAssembly());

        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPatch("Update")]
        private class Rocks
        {
            private static float dist = 100f;
            private static Vector3 vel;
            private static Vector3 normal;
            private static float maxD;

            private static LayerMask layers;

            private static bool DoOnce = false;
            private static ConfigEntry<float> max;

            private static GameObject RightI = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            private static GameObject LeftI = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            private static bool LeftClose;
            private static void Postfix(GorillaLocomotion.Player __instance)
            {

                if (!DoOnce)
                {
                    var file = new ConfigFile(Path.Combine(Paths.ConfigPath, "FastAsRock.cfg"), true);
                    max = file.Bind("Configuration", "MaxDistance", 2f, "The max distance you are able to be from the ground as rock lava or normal monkey this will not apply if your using another mod to get away from the ground");
                    maxD = max.Value;

                    layers = (1 << 9);

                    DoOnce = true;
                }

                if (!PhotonNetwork.CurrentRoom.IsVisible || !PhotonNetwork.InRoom)
                {
                    RaycastHit right;
                    Physics.Raycast(__instance.rightHandTransform.position, -__instance.rightHandTransform.right, out right, 100f, layers);


                    RaycastHit left;
                    Physics.Raycast(__instance.leftHandTransform.position, __instance.leftHandTransform.right, out left, 100f, layers);

                    if (left.distance > right.distance)
                    {
                        normal = right.normal;
                        dist = right.distance;
                        LeftClose = false;
                    }
                    else
                    {

                        normal = left.normal;
                        dist = left.distance;
                        LeftClose = true;

                    }

                    if (dist < maxD)
                    {
                        __instance.bodyCollider.attachedRigidbody.useGravity = false;

                        vel = normal * (9.8f * Time.deltaTime);

                        __instance.bodyCollider.attachedRigidbody.velocity -= vel;
                    }
                    else
                    {

                        __instance.bodyCollider.attachedRigidbody.useGravity = true;
                    }

                }
                else
                {
                    __instance.bodyCollider.attachedRigidbody.useGravity = true;
                }
            }
        }
    }
}
