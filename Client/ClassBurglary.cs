using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using LemonUI;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Mono.CSharp;

namespace Appartment.Client
{
    public class ClassBurglary
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;

        public bool IsBurglarising = false;
        public ClassBurglary(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
        }

        public void StartBurglary()
        {
            Format.SendNotif("Démarrage du ~r~cambriolage~w~...");
            BurglaryAnimation();
        }
        public async void BurglaryAnimation()
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, "anim@heists@humane_labs@emp@hack_door");
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "anim@heists@humane_labs@emp@hack_door")) await BaseScript.Delay(50);
            Game.PlayerPed.Task.ClearAllImmediately();
            AnimationFlags flags = AnimationFlags.Loop;
            Game.PlayerPed.Task.PlayAnimation("anim@heists@humane_labs@emp@hack_door", "hack_intro", -1, 10000, flags);

            AttemptBurglary();
        }

        public async void AttemptBurglary()
        {
            Random random = new Random();
            int randomInt = random.Next(3);
            Format.SendNotif("~r~Piratage~w~ de la porte...");
            await BaseScript.Delay(10000);
            if (randomInt == 2)
            {
                Format.SendNotif("~g~Réussi");
                SetEntityCoords(GetPlayerPed(-1), -783.9f, 323.7f, 212.1f, true, true, true, false);
                BaseScript.TriggerServerEvent("appart:instance", 0, "enter");
                await BaseScript.Delay(3000);
                IsBurglarising = true;
            }
            else
            {
                Format.SendNotif("~r~Raté...");
            }
        }

        public void OnTick()
        {
            if(IsBurglarising == true)
            {
                var playerSpeed = GetEntitySpeed(GetPlayerPed(-1))*3.6;
                if ((playerSpeed * 12) < 66f)
                {
                    Format.SendTextUI($"~g~{(playerSpeed*12).ToString("F3")}dB");
                }
                else if ((playerSpeed * 12) < 71f)
                {
                    Format.SendTextUI($"~y~{(playerSpeed * 12).ToString("F3")}dB");
                    Format.SendNotif("...Fais attention au bruit de pas...");
                } else if ((playerSpeed * 12) >= 72f)
                {
                    Format.SendTextUI($"~r~{(playerSpeed * 12).ToString("F3")}dB");
                    Format.SendNotif("L'alarme a sonné ~r~!!! La police a été appelé");
                    BaseScript.TriggerServerEvent("appart:callPolice");
                }
            }
        }

    }
}
