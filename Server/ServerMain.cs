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

        /*
         * Set a player in a instance (a dimension in the same world)
         * 
         * Parameter: id (instance ID), state (enter or exit)
         */
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
            }
        }

        /*
         * Add a property in the database
         * 
         * Parameter: PosEnter, PosExit (Vector3)
         */
        [EventHandler("appart:addProperty")]
        public void AddProperty([FromSource] Player player, Vector3 PosEnter, Vector3 PosExit, Vector3 PosDress)
        {
            using (var dbContext = new AppartContext())
            {
                var newProperty = new PropertyTable
                {
                    Doors_position = $"[[{PosEnter.X}, {PosEnter.Y}, {PosEnter.Z}], [{PosExit.X}, {PosExit.Y}, {PosExit.Z}]]",
                    Dress_position = $"[{PosDress.X},{PosDress.Y},{PosDress.Z}]"
                };

                dbContext.Property.Add(newProperty);
                dbContext.SaveChanges();
            }
        }

        /*
         * Delete a property
         * 
         * Parameter: propertyId
         */
        [EventHandler("appart:deleteProperty")]
        public void DeleteProperty([FromSource] Player player, int propertyId)
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

        /*
         * Get all property doors
         */
        [EventHandler("appart:getPropertyPositions")]
        public void GetPropertyPositions([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                List<PropertyTable> properties = dbContext.Property.ToList();
                List<Vector3> posEnterList = new List<Vector3>();
                List<Vector3> posExitList = new List<Vector3>();
                List<Vector3> posDressList = new List<Vector3>();

                foreach (var property in properties)
                {
                    var doorsPosition = JsonConvert.DeserializeObject<List<List<double>>>(property.Doors_position);
                    var dressPosition = JsonConvert.DeserializeObject<List<double>>(property.Dress_position);
                    var posEnter = new Vector3((float)doorsPosition[0][0], (float)doorsPosition[0][1], (float)doorsPosition[0][2]);
                    var posExit = new Vector3((float)doorsPosition[1][0], (float)doorsPosition[1][1], (float)doorsPosition[1][2]);
                    var posDress = new Vector3((float)dressPosition[0], (float)dressPosition[1], (float)dressPosition[2]);
                    posEnterList.Add(posEnter);
                    posExitList.Add(posExit);
                    posDressList.Add(posDress);
                }

                string jsonEnterData = JsonConvert.SerializeObject(posEnterList);
                string jsonExitData = JsonConvert.SerializeObject(posExitList);
                string jsonDressData = JsonConvert.SerializeObject(posDressList);

                TriggerClientEvent(player, "appart:updatePropertyPosition", jsonEnterData, jsonExitData, jsonDressData);
            }
        }

        /*
         * Get all properties in the database
         */
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
                        property.Doors_position,
                        property.Dress_position
                    };
                    propertiesList.Add(propertyData);
                }

                string jsonProperty = JsonConvert.SerializeObject(propertiesList);

                TriggerClientEvent("appart:updatePropertyList", jsonProperty);
            }
        }

        /*
         * Get all appartments in the database
         */
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
                        appart.Id_property,
                        playerName = GetPlayerName(appart.Id_player.ToString()),
                        appart.Id_player,
                        appart.isOpen,
                        appart.Chest,
                        appart.Price,
                        appart.Booking
                    };
                    appartsList.Add(appartsData);
                }

                foreach (var property in properties)
                {
                    var propertyData = new
                    {
                        property.Id_property,
                        property.Doors_position,
                        property.Dress_position
                    };
                    propertiesList.Add(propertyData);
                }

                string jsonProperty = JsonConvert.SerializeObject(propertiesList);
                string jsonAppartPlayer = JsonConvert.SerializeObject(appartsList);

                TriggerClientEvent("appart:updateAppartList", jsonProperty, jsonAppartPlayer);
            }
        }

        [EventHandler("appart:getClosestPlayer")]
        public void GetClosest([FromSource] Player player, Ped closestPedId)
        {
            Debug.WriteLine($"player: {player.Handle}");
            Debug.WriteLine($"closestPedId: {closestPedId.NetworkId}");
            //Debug.WriteLine($"GetPlayerIdentifier: {GetPlayerIdentifier(closestPedId.Handle.ToString(), closestPedId.NetworkId)}");
        }

        [EventHandler("appart:callPolice")]
        public void CallPolice([FromSource] Player player)
        {
            // For example, to remove after
            SetPlayerWantedLevel(player.Handle, 4, false);
        }

        /*
         * Server-side
         * 
         * Allows a player to reserve a appart
         * 
         * Adds the player's ID to the database.
         */
        [EventHandler("appart:bookingRequest")]
        public void BookingRequest([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                var appartPlayer = dbContext.AppartPlayer.SingleOrDefault(a => a.Id_player.ToString() == player.Handle);

                if (appartPlayer != null)
                {
                    appartPlayer.Booking = player.Handle;
                    dbContext.SaveChanges();
                }
            }
            TriggerClientEvent("appart:bookingRequestSuccesfull");
        }

    }
}
