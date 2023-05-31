using CitizenFX.Core;
using LemonUI.Menus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;
using LemonUI;
using Mono.CSharp;

namespace Appartment.Client
{
    public class ClassProperty
    {
        public ClientMain Client;
        public Format Format;
        public ObjectPool Pool = new ObjectPool();
        public BaseScript BaseScript;
        public List<PropertyData> propertyData = new List<PropertyData>();
        public Dictionary<string, object> jsonProperties = new Dictionary<string, object>();
        public ClassProperty(ClientMain caller)
        {
            Pool = caller.Pool;
            Client = caller;
            Format = caller.Format;
            RegisterCommand("property", new Action<int, List<object>>(PropertyMenu), false);
            BaseScript.TriggerServerEvent("appart:getProperties");
            caller.AddEvent("appart:updatePropertyList", new Action<string>(UpdatePropertyList));
        }

        public void UpdatePropertyList(string jsonProperty)
        {
            propertyData = JsonConvert.DeserializeObject<List<PropertyData>>(jsonProperty);

            foreach (var property in propertyData)
            {
                jsonProperties["Id_property"] = property.Id_property;
                jsonProperties["Doors_position"] = property.Doors_position;

                string jsonString = JsonConvert.SerializeObject(jsonProperties);
                Client.jsonProp += jsonString;
            }
        }

        public void PropertyMenu(int source, List<object> args)
        {
            var posPropertyEnter = new Vector3();
            var posPropertyExit = new Vector3();

            var menu = new NativeMenu("Immobilier", "Gérer les propriétés");
            Pool.Add(menu);
            menu.Visible = true;
            menu.UseMouse = false;

            var propertyMenu = new NativeMenu("Ajouter une propriété", "Ajouter une propriété");
            Pool.Add(propertyMenu);
            menu.AddSubMenu(propertyMenu);
            var enterItem = new NativeItem("Placer une entrée");
            propertyMenu.Add(enterItem);
            var exitItem = new NativeItem("Placer une sortie");
            propertyMenu.Add(exitItem);
            enterItem.Activated += (sender, e) =>
            {
                posPropertyEnter = GetEntityCoords(GetPlayerPed(-1), true);
                Format.SendNotif("Le point d'entrée est bien posé");
            };

            exitItem.Activated += (sender, e) =>
            {
                posPropertyExit = GetEntityCoords(GetPlayerPed(-1), true);
                Format.SendNotif("Le point de sortie est bien posé");
            };

            var submit = new NativeItem("Envoyer");
            propertyMenu.Add(submit);
            submit.Activated += (sender, e) =>
            {
                if (posPropertyEnter != null && posPropertyExit != null)
                {
                    BaseScript.TriggerServerEvent("appart:addProperty", posPropertyEnter, posPropertyExit);
                    Format.SendNotif("Les informations ont bien été envoyé");
                }
                else
                {
                    Format.SendNotif("Vous ne pouvez pas laisser vide les points d'entrée et de sortie");
                }
            };
        }
    }

    public class PropertyData
    {
        public int Id_property { get; set; }
        public string Doors_position { get; set; }
    }
}
