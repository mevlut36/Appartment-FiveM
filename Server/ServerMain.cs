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
                Debug.WriteLine($"Le joueur {player.Name} est entré dans l'appart {id}");
            } else if (state == "exit")
            {
                SetPlayerRoutingBucket(player.Handle, 0);
                Debug.WriteLine($"Le joueur {player.Name} est sorti de l'appart {id}");
            }
            else
            {
                SetPlayerRoutingBucket(player.Handle, 0);
                Debug.WriteLine("Null, default exit");
            }
        }

        [EventHandler("appart:isMyAppart")]
        public void GetPlayerServerId([FromSource] Player player, int id)
        {
            GetPlayerIdentifier(player.Handle, id);
        }

        [EventHandler("appart:addAppart")]
        public void AddAppart([FromSource] Player player, Vector3 PosEnter, Vector3 PosExit)
        {
            using (var dbContext = new AppartContext())
            {
                var newAppartment = new AppartmentTable
                {
                    Doors_position = $"[[{PosEnter.X}, {PosEnter.Y}, {PosEnter.Z}], [{PosExit.X}, {PosExit.Y}, {PosExit.Z}]]"
                };

                dbContext.Appartment.Add(newAppartment);
                dbContext.SaveChanges();
            }
        }

        [EventHandler("appart:deleteAppart")]
        public void DeleteAppart([FromSource] Player player, int appartId)
        {
            using (var dbContext = new AppartContext())
            {
                var appartment = dbContext.Appartment.FirstOrDefault(a => a.Id_appart == appartId);
                if (appartment != null)
                {
                    dbContext.Appartment.Remove(appartment);
                    dbContext.SaveChanges();
                }
            }
        }

        [EventHandler("appart:getDoorsPositions")]
        public void GetDoorsPositions([FromSource] Player player)
        {
            using (var dbContext = new AppartContext())
            {
                List<AppartmentTable> appartments = dbContext.Appartment.ToList();
                List<Vector3> posEnterList = new List<Vector3>();
                List<Vector3> posExitList = new List<Vector3>();

                foreach (var appartment in appartments)
                {
                    var doorsPosition = JsonConvert.DeserializeObject<List<List<double>>>(appartment.Doors_position);
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


    }
}