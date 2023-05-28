using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using LemonUI;
using LemonUI.Menus;
using Mono.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace Appartment.Client
{
    public class ClientMain : BaseScript
    {
        List<Vector3> startPosList = new List<Vector3>();
        List<Vector3> destPosList = new List<Vector3>();

        Vector3 dressPos = new Vector3(-760.7f, 325.4f, 217.0f);

        Dictionary<string, object> jsonAppart = new Dictionary<string, object>();
        Dictionary<string, object> jsonProperties = new Dictionary<string, object>();
        string json = "";
        string jsonProp = "";
        public ObjectPool Pool = new ObjectPool();

        private bool isNoClipEnabled = false;

        public ClientMain()
        {
            Debug.WriteLine("Hi from Appartment.Client!");
            RegisterCommand("property", new Action<int, List<object>>(PropertyMenu), false);
            TriggerServerEvent("appart:getDoorsPositions");
            TriggerServerEvent("appart:getApparts");
            TriggerServerEvent("appart:getProperties");
        }

        [EventHandler("appart:updateStartPos")]
        public void UpdateStartPos(string posEnterArray, string posExitArray)
        {
            List<Vector3> doorsEnterPositionsList = JsonConvert.DeserializeObject<List<Vector3>>(posEnterArray);
            List<Vector3> doorsExitPositionsList = JsonConvert.DeserializeObject<List<Vector3>>(posExitArray);
            startPosList.AddRange(doorsEnterPositionsList);
            destPosList.AddRange(doorsExitPositionsList);
        }

        [EventHandler("appart:updateAppartList")]
        public void UpdateAppartList(string jsonProperty, string jsonAppartPlayer)
        {
            List<AppartPlayerData> appartPlayerData = JsonConvert.DeserializeObject<List<AppartPlayerData>>(jsonAppartPlayer);

            foreach (var appartPlayer in appartPlayerData)
            {
                jsonAppart["Id_property"] = appartPlayer.Id_property;
                jsonAppart["playerName"] = appartPlayer.playerName;
                jsonAppart["Id_player"] = appartPlayer.Id_player;
                jsonAppart["isOpen"] = appartPlayer.isOpen;

                string jsonString = JsonConvert.SerializeObject(jsonAppart);
                json += jsonString;
            }
        }

        [EventHandler("appart:updatePropertyList")]
        public void UpdatePropertyList(string jsonProperty)
        {
            List<PropertyData> propertyData = JsonConvert.DeserializeObject<List<PropertyData>>(jsonProperty);

            foreach (var property in propertyData)
            {
                jsonProperties["Id_property"] = property.Id_property;
                jsonProperties["Doors_position"] = property.Doors_position;

                string jsonString = JsonConvert.SerializeObject(jsonProperties);
                jsonProp += jsonString;
            }
        }

        public void PropertyMenu(int source, List<object> args)
        {
            var posEnter = new Vector3();
            var posExit = new Vector3();
            
            var menu = new NativeMenu("Immobilier", "Ajouter un appartement");
            Pool.Add(menu);
            menu.Visible = true;

            var enterItem = new NativeItem("Mettre une entrée");
            menu.Add(enterItem);
            var exitItem = new NativeItem("Mettre une sortie");
            menu.Add(exitItem);

            enterItem.Activated += (sender, e) =>
            {
                posEnter = GetEntityCoords(GetPlayerPed(-1), true);
                SendNotif("Le point d'entrée est bien posé");
            };

            exitItem.Activated += (sender, e) =>
            {
                posExit = GetEntityCoords(GetPlayerPed(-1), true);
                SendNotif("Le point de sortie est bien posé");
            };

            var submit = new NativeItem("Envoyer");
            menu.Add(submit);
            submit.Activated += (sender, e) =>
            {
                if (posEnter != null && posExit != null)
                {
                    TriggerServerEvent("appart:addProperty", posEnter, posExit);
                    SendNotif("Les informations ont bien été envoyé");
                } else
                {
                    SendNotif("Vous ne pouvez pas laisser vide les points d'entrée et de sortie");
                }
            };

        }

        /*
         * Create a quoicouMarker
         * Parameter : position type Vector3    
         */
        public void SetMarker(Vector3 position, MarkerType markerType)
        {
            World.DrawMarker(markerType, position, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), System.Drawing.Color.FromArgb(255, 130, 0), true);
        }

        /* 
         * TeleportPoint function is used to teleport the player character to a specific point in the game world.
         * It takes the following parameters:
         *  pointPos: The position of the teleportation point as a Vector3.
         *  buttonText: The text to be displayed on the UI button for teleportation.
         *  teleportPos: The position where the player character will be teleported as a Vector3.
         *  state: The state or condition associated with the teleportation.
         * 
         * The function performs the following steps:
         * Get the current position of the player character.
         * Set a marker at the specified teleportation point.
         * Calculate the squared 2D distance between the player character and the teleportation point.
         * If the distance is less than or equal to 5 units:
         * a. Check if the player character is not in any vehicle.
         * b. Display a text on the UI instructing the player to press the 'E' key for teleportation.
         * c. Call the TeleportPressed function with the teleportation position and state as parameters.
         */

        public void TeleportPoint(Vector3 pointPos, string buttonText, Vector3 teleportPos, string state)
        {
            Vector3 playerPos = LocalPlayer.Character.Position;
            SetMarker(pointPos, MarkerType.UpsideDownCone);
            var distPoint = playerPos.DistanceToSquared(pointPos);
            if (distPoint <= 5)
            {
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == false)
                {
                    SendTextUI($"~w~Appuyer sur ~r~E ~w~pour {buttonText}");
                    TeleportPressed(teleportPos, state);
                }
            }
        }

        public void DressPoint(Vector3 pointPos)
        {
            Vector3 playerPos = LocalPlayer.Character.Position;
            SetMarker(pointPos, MarkerType.HorizontalSplitArrowCircle);
            var distPoint = playerPos.DistanceToSquared(pointPos);
            if (distPoint <= 5)
            {
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == false)
                {
                    SendTextUI($"~w~Appuyer sur ~r~E ~w~pour vous changer");
                    DressPressed();
                }
            }
        }

        /* 
         * Once the button is pressed, it teleports inside
         * The first parameter is the coordinates (x, y, z)
         * state parameter can only be "enter" or "exit"
         * Specifies whether the player wants to enter or exit the appartment
         */
        public void TeleportPressed(Vector3 position, string state)
        {
            if (IsControlJustPressed(0, 51))
            {
                AppartmentMenu(position, state);
            }
        }

        public void DressPressed()
        {
            if (IsControlJustPressed(0, 51))
            {
                DressMenu();
            }
        }

        /*
         * Open a menu with apparts
         */
        public void AppartmentMenu(Vector3 position, string state)
        {
            List<string> jsonAppartObjects = SplitJsonObjects(json);
            List<string> jsonPropertyObjects = SplitJsonObjects(jsonProp);

            List<AppartPlayerData> appartPlayerDataList = new List<AppartPlayerData>();
            List<PropertyData> propertyDataList = new List<PropertyData>();

            foreach (string jsonObject in jsonAppartObjects)
            {
                AppartPlayerData appartPlayerData = JsonConvert.DeserializeObject<AppartPlayerData>(jsonObject);
                appartPlayerDataList.Add(appartPlayerData);
            }

            foreach (string jsonObject in jsonPropertyObjects)
            {
                PropertyData propertyData = JsonConvert.DeserializeObject<PropertyData>(jsonObject);
                propertyDataList.Add(propertyData);
            }

            if (state == "enter")
            {
                var menu = new NativeMenu("Interphone", "Appuyer pour sonner");
                Pool.Add(menu);
                menu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
                menu.Visible = true;
                menu.UseMouse = false;
                menu.HeldTime = 100;

                bool isOpen;
                string stateAppart;

                foreach (PropertyData propertyData in propertyDataList)
                {
                    if (propertyData == null)
                    {
                        continue;
                    }

                    var doorsPositionList = JsonConvert.DeserializeObject<List<List<double>>>(propertyData.Doors_position);

                    foreach (var doorPosition in doorsPositionList)
                    {
                        if (Math.Abs(doorPosition[0] - position.X) < 0.001 && Math.Abs(doorPosition[1] - position.Y) < 0.001 && Math.Abs(doorPosition[2] - position.Z) < 0.001)
                        {
                            foreach (AppartPlayerData appartPlayerData in appartPlayerDataList)
                            {
                                if (appartPlayerData.Id_property == propertyData.Id_property)
                                {
                                    isOpen = appartPlayerData.isOpen;
                                    stateAppart = isOpen ? "~g~Ouvert" : "~r~Fermé";

                                    NativeItem btn;
                                    if (appartPlayerData.playerName != null)
                                    {
                                        btn = new NativeItem($"Appartement N°{appartPlayerData.Id_player} - {appartPlayerData.playerName}", "", $"{stateAppart}");
                                    }
                                    else
                                    {
                                        btn = new NativeItem("À louer", "", "~r~Fermé");
                                        appartPlayerData.isOpen = false;
                                    }

                                    menu.Add(btn);

                                    btn.Activated += (sender, e) =>
                                    {
                                        if (appartPlayerData.isOpen)
                                        {
                                            menu.Visible = false;
                                            SetEntityCoords(GetPlayerPed(-1), position.X, position.Y, position.Z, true, true, true, false);
                                            TriggerServerEvent("appart:instance", appartPlayerData.Id_player, state);
                                        }
                                        else
                                        {
                                            SendNotif("L'appartement est ~r~fermé");
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SetEntityCoords(GetPlayerPed(-1), position.X, position.Y, position.Z, true, true, true, false);
                TriggerServerEvent("appart:instance", 0, state);
            }
        }




        public void DressMenu()
        {
            Game.Player.ChangeModel(new Model(PedHash.FreemodeMale01));
            SetPedComponentVariation(GetPlayerPed(-1), 8, 15, 0, 2);
            var ped = PlayerPedId();
            var menu = new NativeMenu("Garde robe", "Choisissez vos vêtements");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;
            menu.HeldTime = 100;
            menu.AcceptsInput = true; ;
            if (menu.Visible) {
                {
                    menu.Visible = false;
                } }
            /*
             * 0: Face\ 1: Mask\ 2: Hair\ 3: Torso\ 4: Leg\
             * 5: Parachute / bag\ 6: Shoes\ 7: Accessory\
             * 8: Undershirt\ 9: Kevlar\ 10: Badge\ 11: Torso 2
             */
            var componentDict = new Dictionary<int, string>()
            {
                { 1, "Masque" },
                { 8, "Sous haut" },
                { 11, "Haut" },
                { 3, "Bras" },
                { 4, "Bas" },
                { 6, "Chaussures" },
                { 7, "Accessoires" },
                { 5, "Sacs" }
            };
            foreach(var clothes in componentDict)
            {
                var itemDrawableList = Enumerable.Range(0, GetNumberOfPedDrawableVariations(GetPlayerPed(-1), clothes.Key)).ToList();
                NativeListItem<int> itemDrawable = new NativeListItem<int>(clothes.Value, itemDrawableList.ToArray());
                menu.Add(itemDrawable);
                itemDrawable.ItemChanged += (sender, e) =>
                {
                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex, 0, 1);
                };
                var itemTextureList = Enumerable.Range(0, GetNumberOfPedTextureVariations(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex)).ToList();
                NativeListItem<int> itemTexture = new NativeListItem<int>($"~h~Texture~s~ {clothes.Value}", itemTextureList.ToArray());
                menu.Add(itemTexture);
                itemTexture.ItemChanged += (sender, e) =>
                {
                    SetPedComponentVariation(GetPlayerPed(-1), clothes.Key, itemDrawable.SelectedIndex, itemTexture.SelectedIndex, 1);
                };
            }
            
        }

        /*
         * Notification style
         * Parameter: Text entry
         */
        public void SendNotif(string text)
        {
            BeginTextCommandThefeedPost("STRING");
            AddTextComponentString(text);
            EndTextCommandThefeedPostTicker(true, true);
        }

        /*
         * TextUI Style
         * Parameter: Text entry
         */
        public void SendTextUI(string text)
        {
            SetTextFont(6);
            SetTextScale(0.5f, 0.5f);
            SetTextProportional(false);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextCentre(false);
            SetTextJustification(0);
            SetTextEntry("STRING");
            AddTextComponentString($"{text}");
            int x = 0, y = 0;
            GetScreenActiveResolution(ref x, ref y);
            DrawText(0.50f, 0.80f);
        }

        [Tick]
        public Task OnTick()
        {
            Pool.Process();
            Vector3 playerPos = LocalPlayer.Character.Position;
            CheckAndTeleport(playerPos, startPosList, destPosList, "rentrer", "enter");
            CheckAndTeleport(playerPos, destPosList, startPosList, "sortir", "exit");
            CheckDress(playerPos, dressPos);
            return Task.FromResult(0);
        }

        /*
         * The marker is visible from 50 metres
         */
        private void CheckAndTeleport(Vector3 playerPos, List<Vector3> pointListPos, List<Vector3> teleportListPos, string buttonText, string state)
        {
            for (int i = 0; i < pointListPos.Count; i++)
            {
                var pointPos = pointListPos[i];
                var teleportPos = teleportListPos[i];

                var distPoint = playerPos.DistanceToSquared2D(pointPos);
                if (distPoint <= 50)
                {
                    TeleportPoint(pointPos, buttonText, teleportPos, state);
                }
            }
        }

        private void CheckDress(Vector3 playerPos, Vector3 pointPos)
        {
            var distPoint = playerPos.DistanceToSquared2D(pointPos);
            if (distPoint <= 50)
            {
                DressPoint(pointPos);
            }
        }

        public static List<string> SplitJsonObjects(string jsonString)
        {
            jsonString = jsonString.Replace("}{", "}|{");
            jsonString = jsonString.Replace("}\n{", "}\r\n{");
            string[] jsonObjectsArray = jsonString.Split('|');
            List<string> jsonObjectsList = new List<string>(jsonObjectsArray);
            return jsonObjectsList;
        }

    }

    public class PropertyData
    {
        public int Id_property { get; set; }
        public string Doors_position { get; set; }
    }

    public class AppartPlayerData
    {
        public int Id_player { get; set; }
        public string playerName { get; set; }
        public int Id_property { get; set; }
        public bool isOpen { get; set; }
        public string Chest { get; set; }
    }
}
