// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Core;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Azure.ResourceManager.Samples.Common
{
    public static class Utilities
    {
        public static Action<string> LoggerMethod { get; set; }
        public static Func<string> PauseMethod { get; set; }
        public static string ProjectPath { get; set; }
        private static Random _random => new Random();

        static Utilities()
        {
            LoggerMethod = Console.WriteLine;
            PauseMethod = Console.ReadLine;
            ProjectPath = ".";
        }

        public static void Log(string message)
        {
            LoggerMethod.Invoke(message);
        }

        public static void Log(object obj)
        {
            if (obj != null)
            {
                LoggerMethod.Invoke(obj.ToString());
            }
            else
            {
                LoggerMethod.Invoke("(null)");
            }
        }

        public static void Log()
        {
            Utilities.Log("");
        }

        public static string ReadLine() => PauseMethod.Invoke();

        public static string CreateRandomName(string namePrefix) => $"{namePrefix}{_random.Next(9999)}";

        public static string CreatePassword() => "azure12345QWE!";

        public static string CreateUsername() => "tirekicker";

        public static async Task PrintVirtualNetwork(VirtualNetworkResource vnet)
        {
            Log($"====Display peering in {vnet.Data.Name}====");
            await foreach (var item in vnet.GetVirtualNetworkPeerings().GetAllAsync())
            {
                Log("Peering name:"+item.Data.Name);
                Log("PeeringState:" + item.Data.PeeringState);
                Log("RemoteVirtualNetwork:" + item.Data.RemoteVirtualNetworkId.Name);
                Log("AllowVirtualNetworkAccess:" + item.Data.AllowVirtualNetworkAccess);
                Log("AllowForwardedTraffic:" + item.Data.AllowForwardedTraffic);
                Log("AllowGatewayTransit:" + item.Data.AllowGatewayTransit);
                Log("UseRemoteGateways:" + item.Data.UseRemoteGateways);
            }
            Log("Done");
            Log();
        }
    }
}
