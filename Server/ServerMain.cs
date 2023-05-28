using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Microsoft.EntityFrameworkCore;
using static CitizenFX.Core.Native.API;
using Appartment.DataContext;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Appartment.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from Appartment.Server!");
        }

        [EventHandler("appart:instance")]
        public void SetPlayerInInstance([FromSource] Player player, int id, string state)
        {
            if (state == "enter")
            {
                SetPlayerRoutingBucket(player.Handle, id);
            } else if (state == "exit")
            {
                SetPlayerRoutingBucket(player.Handle, 0);
            }
            else
            {
                SetPlayerRoutingBucket(player.Handle, 0);
                Debug.WriteLine("Null, default exit");
            }
        }

        [EventHandler("appart:addProperty")]
        public void AddProperty([FromSource] Player player, Vector3 PosEnter, Vector3 PosExit)
        {
            using (var dbContext = new AppartContext())
            {
                var newProperty = new PropertyTable
                {
                    Doors_position = $"[[{PosEnter.X}, {PosEnter.Y}, {PosEnter.Z}], [{PosExit.X}, {PosExit.Y}, {PosExit.Z}]]"
                };

                dbContext.Property.Add(newProperty);
                dbContext.SaveChanges();
            }
        }

        [EventHandler("appart:deleteProperty")]
        public void DeleteAppart([FromSource] Player player, int propertyId)
        {
            using (var dbContext = new AppartContext())
            {
                var property = dbContext.Property.FirstOrDefault(a => a.Id_property == propertyId);
                if (property != null)
                {
                    dbContext.Property.Remove(property);
                    dbContext.SaveChanges();
                }
            }
        }

        [EventHandler("appart:getDoorsPositions")]
        public void GetDoorsPositions([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                List<PropertyTable> properties = dbContext.Property.ToList();
                List<Vector3> posEnterList = new List<Vector3>();
                List<Vector3> posExitList = new List<Vector3>();

                foreach (var property in properties)
                {
                    var doorsPosition = JsonConvert.DeserializeObject<List<List<double>>>(property.Doors_position);
                    var posEnter = new Vector3((float)doorsPosition[0][0], (float)doorsPosition[0][1], (float)doorsPosition[0][2]);
                    var posExit = new Vector3((float)doorsPosition[1][0], (float)doorsPosition[1][1], (float)doorsPosition[1][2]);
                    posEnterList.Add(posEnter);
                    posExitList.Add(posExit);
                }

                string jsonEnterData = JsonConvert.SerializeObject(posEnterList);
                string jsonExitData = JsonConvert.SerializeObject(posExitList);

                TriggerClientEvent(player, "appart:updateStartPos", jsonEnterData, jsonExitData);
            }
        }

        [EventHandler("appart:getProperties")]
        public void GetProperties([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                List<PropertyTable> properties = dbContext.Property.ToList();
                var propertiesList = new List<object>();
                foreach (var property in properties)
                {
                    var propertyData = new
                    {
                        property.Id_property,
                        property.Doors_position
                    };
                    propertiesList.Add(propertyData);
                }

                string jsonProperty = JsonConvert.SerializeObject(propertiesList);

                TriggerClientEvent("appart:updatePropertyList", jsonProperty);
            }
        }

        [EventHandler("appart:getApparts")]
        public void GetApparts([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                List<AppartPlayerTable> apparts = dbContext.AppartPlayer.ToList();
                List<PropertyTable> properties = dbContext.Property.ToList();
                var appartsList = new List<object>();
                var propertiesList = new List<object>();

                foreach (var appart in apparts)
                {
                    var appartsData = new
                    {
                        appart.Id_player,
                        playerName = GetPlayerName(appart.Id_player.ToString()),
                        appart.Id_property,
                        appart.isOpen,
                        appart.Chest
                    };
                    appartsList.Add(appartsData);
                }

                foreach (var property in properties)
                {
                    var propertyData = new
                    {
                        property.Id_property,
                        property.Doors_position
                    };
                    propertiesList.Add(propertyData);
                }

                string jsonProperty = JsonConvert.SerializeObject(propertiesList);
                string jsonAppartPlayer = JsonConvert.SerializeObject(appartsList);

                TriggerClientEvent("appart:updateAppartList", jsonProperty, jsonAppartPlayer);
            }
        }

    }
}
