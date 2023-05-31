using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Mono.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using static CitizenFX.Core.Native.API;
using LemonUI;
using LemonUI.Menus;

namespace Appartment.Client
{
    public class ClientMain : BaseScript
    {
        public Format Format;
        public ClassProperty Property;
        List<Vector3> startPosList = new List<Vector3>();
        List<Vector3> destPosList = new List<Vector3>();

        Vector3 dressPos = new Vector3(-760.7f, 325.4f, 217.0f);

        Dictionary<string, object> jsonAppart = new Dictionary<string, object>();
        string json = "";
        public string jsonProp = "";
        private string inputText;
        bool InputEntry = false;
        private int keyboardState = -1;
        public ObjectPool Pool = new ObjectPool();

        public ClientMain()
        {
            Debug.WriteLine("Hi from Appartment.Client!");
            Format = new Format(this);
            Property = new ClassProperty(this);
            TriggerServerEvent("appart:getDoorsPositions");
            TriggerServerEvent("appart:getApparts");
        }

        public void AddEvent(string key, System.Delegate value) => this.EventHandlers.Add(key, value);

        [EventHandler("appart:updateDoorsPosition")]
        public void UpdateDoorsPosition(string posEnterArray, string posExitArray)
        {
            List<Vector3> doorsEnterPositionsList = JsonConvert.DeserializeObject<List<Vector3>>(posEnterArray);
            List<Vector3> doorsExitPositionsList = JsonConvert.DeserializeObject<List<Vector3>>(posExitArray);
            startPosList.AddRange(doorsEnterPositionsList);
            destPosList.AddRange(doorsExitPositionsList);
        }

        [EventHandler("appart:updateAppartList")]
        public void UpdateAppartList(string jsonPropertyPlayer, string jsonAppartPlayer)
        {
            List<AppartPlayerData> appartPlayerData = JsonConvert.DeserializeObject<List<AppartPlayerData>>(jsonAppartPlayer);
            foreach (var appartPlayer in appartPlayerData)
            {
                jsonAppart["Id_property"] = appartPlayer.Id_property;
                jsonAppart["playerName"] = appartPlayer.playerName;
                jsonAppart["Id_player"] = appartPlayer.Id_player;
                jsonAppart["isOpen"] = appartPlayer.isOpen;
                jsonAppart["Chest"] = appartPlayer.Chest;
                jsonAppart["Price"] = appartPlayer.Price;
                jsonAppart["Booking"] = appartPlayer.Booking;

                string jsonString = JsonConvert.SerializeObject(jsonAppart);
                json += jsonString;
            }
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
            Format.SetMarker(pointPos, MarkerType.UpsideDownCone);
            var distPoint = playerPos.DistanceToSquared(pointPos);
            if (distPoint <= 5)
            {
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == false)
                {
                    Format.SendTextUI($"~w~Appuyer sur ~r~E ~w~pour {buttonText}");
                    TeleportPressed(teleportPos, state);
                }
            }
        }

        public void DressPoint(Vector3 pointPos)
        {
            Vector3 playerPos = LocalPlayer.Character.Position;
            Format.SetMarker(pointPos, MarkerType.HorizontalSplitArrowCircle);
            var distPoint = playerPos.DistanceToSquared(pointPos);
            if (distPoint <= 5)
            {
                if (IsPedInAnyVehicle(GetPlayerPed(-1), false) == false)
                {
                    Format.SendTextUI($"~w~Appuyer sur ~r~E ~w~pour vous changer");
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

        public async void TestDance()
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, "special_ped@mountain_dancer@base");
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "special_ped@mountain_dancer@base")) await Delay(50);
            Game.PlayerPed.Task.ClearAllImmediately();
            AnimationFlags flags = AnimationFlags.Loop | AnimationFlags.CancelableWithMovement;
            Game.PlayerPed.Task.PlayAnimation("special_ped@mountain_dancer@base", "base", -1, -1, flags);
        }

        /*
         * Open a menu with apparts
         */
        public void AppartmentMenu(Vector3 position, string state)
        {
            List<string> jsonAppartObjects = Format.SplitJsonObjects(json);
            List<string> jsonPropertyObjects = Format.SplitJsonObjects(jsonProp);

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

            var menu = new NativeMenu("Interphone", "Appuyer pour sonner");
            Pool.Add(menu);
            menu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
            menu.Visible = true;
            menu.UseMouse = false;
            menu.HeldTime = 100;

            var bookingItem = new NativeItem("~ws~ | <b>Réserver</b> un appart ~ws~", "Cliquez et patientez une réponse de l'agent immobilier...");
            menu.Add(bookingItem);
            bookingItem.Activated += (sender, e) =>
            {
                TriggerServerEvent("appart:bookingRequest");
            };

            // If Appartment is OPEN
            if (state == "enter")
            {
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
                        // Compare the position of the door with the position of the menu marker
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
                                        btn = new NativeItem($"À louer, Prix: {appartPlayerData.Price}$ /semaine", "", "~r~Fermé");
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
                                            Format.SendNotif("L'appartement est ~r~fermé~w~...");
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            // If Appartment is CLOSE
            else
            {
                SetEntityCoords(GetPlayerPed(-1), position.X, position.Y, position.Z, true, true, true, false);
                TriggerServerEvent("appart:instance", 0, state);
            }

            var appartMenu = new NativeMenu("Appartement", "Créer un appartment (ADMIN)");
            Pool.Add(appartMenu);
            menu.AddSubMenu(appartMenu);
            appartMenu.TitleFont = CitizenFX.Core.UI.Font.ChaletLondon;
            appartMenu.UseMouse = false;
            appartMenu.HeldTime = 100;

            var idProp = new NativeItem("N° de la propriété");
            appartMenu.Add(idProp);
            appartMenu.AcceptsInput = true;
            idProp.Activated += (sender, e) =>
            {
                DisplayOnscreenKeyboard(0, "FMMC_KEY_TIP8", "", "", "", "", "", 64);
                InputEntry = true;
                if(inputText != null)
                {
                    idProp.AltTitle = inputText;
                }
            };

            var player = LocalPlayer.Character.Position;
            Ped closestPed = World.GetClosest(player, World.GetAllPeds());

            var test = new NativeItem("TODO - INDEV");
            appartMenu.Add(test);
            var client = new NativeItem("Pour qui ?", "Approchez-vous du joueur pour récuperer son ID");
            appartMenu.Add(client);

            client.Selected += (sender, e) =>
            {
                float distanceFromPos = player.DistanceToSquared2D(closestPed.Position);
                Debug.WriteLine($"distanceFromPos : {distanceFromPos}");
                Debug.WriteLine($"closestPed : {closestPed.Handle}");
                Debug.WriteLine($"myPed : {LocalPlayer.Character.Handle}");
            };
            client.Activated += (sender, e) =>
            {
                Debug.WriteLine("Activer");
                TriggerServerEvent("appart:getClosestPlayer", closestPed);
            };
        }

        [EventHandler("appart:bookingRequestSuccesfull")]
        public void BookingRequestSuccesfull()
        {
            Format.SendNotif("Votre demande de réservation a bien été envoyé. Veuillez attendre une réponse de l'agent immobilier.");
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
            menu.AcceptsInput = true;
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

        [Tick]
        public Task OnTick()
        {
            Pool.Process();
            Vector3 playerPos = LocalPlayer.Character.Position;
            CheckAndTeleport(playerPos, startPosList, destPosList, "rentrer", "enter");
            CheckAndTeleport(playerPos, destPosList, startPosList, "sortir", "exit");
            CheckDress(playerPos, dressPos);

            if (InputEntry)
            {
                API.HideHudAndRadarThisFrame();

                keyboardState = API.UpdateOnscreenKeyboard();

                if (keyboardState == 3)
                {
                    InputEntry = false;
                }
                else if (keyboardState == 1)
                {
                    string inputText = GetOnscreenKeyboardResult();
                    this.inputText = inputText;
                    Debug.WriteLine($"Resultat: {inputText}");
                    InputEntry = false;
                }
                else if (keyboardState == 2)
                {
                    InputEntry = false;
                }
            }
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
    }

    public class AppartPlayerData
    {
        public int Id_player { get; set; }
        public string playerName { get; set; }
        public int Id_property { get; set; }
        public bool isOpen { get; set; }
        public string Chest { get; set; }
        public int Price { get; set; }
        public int Booking { get; set; }
    }
}
