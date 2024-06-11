// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using System.Xml.Linq;

namespace ManageNetworkPeeringInSameSubscription
{
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        /**
         * Azure Network sample for enabling and updating network peering between two virtual networks
         *
         * Summary ...
         *
         * - This sample creates two virtual networks in the same subscription and then peers them, modifying various options on the peering.
         *
         * Details ...
         *
         * 1. Create two virtual networks, network "A" and network "B"...
         * - network A with two subnets
         * - network B with one subnet
         * - the networks' address spaces must not overlap
         * - the networks must be in the same region
         *
         * 2. Peer the networks...
         * - the peering will initially have default settings:
         *   - each network's IP address spaces will be accessible from the other network
         *   - no traffic forwarding will be enabled between the networks
         *   - no gateway transit between one network and the other will be enabled
         *
         * 3. Update the peering...
         * - disable IP address space between the networks
         * - enable traffic forwarding from network A to network B
         * 
         * 4. Delete the peering
         * - the removal of the peering takes place on both networks, as long as they are in the same subscription
         
         * Notes: 
         * - Once a peering is created, it cannot be pointed at another remote network later.
         * - The address spaces of the peered networks cannot be changed as long as the networks are peered.
         * - Gateway transit scenarios as well as peering networks in different subscriptions are possible but beyond the scope of this sample.
         * - Network peering in reality results in pairs of peering objects: one pointing from one network to the other,
         *   and the other peering object pointing the other way. For simplicity though, the SDK provides a unified way to
         *   manage the peering as a whole, in a single command flow, without the need to duplicate commands for both sides of the peering,
         *   while enforcing the required restrictions between the two peerings automatically, as this sample shows. But it is also possible
         *   to modify each peering separately, which becomes required when working with networks in different subscriptions.
         */
        public static async Task RunSample(ArmClient client)
        {
            string vnetAName = Utilities.CreateRandomName("vnetA-");
            string vnetBName = Utilities.CreateRandomName("vnetB-");
            string peeringABName = Utilities.CreateRandomName("peer");

            try
            {
                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the EastUS region
                string rgName = Utilities.CreateRandomName("NetworkSampleRG");
                Utilities.Log($"Creating resource group...");
                ArmOperation<ResourceGroupResource> rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
                ResourceGroupResource resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created a resource group with name: " + resourceGroup.Data.Name);

                //=============================================================
                // Define two virtual networks to peer

                Utilities.Log("Creating two virtual networks in the same region and subscription...");

                VirtualNetworkData vnetAInput = new VirtualNetworkData()
                {
                    Location = resourceGroup.Data.Location,
                    AddressPrefixes = { "10.0.0.0/27" },
                    Subnets =
                    {
                        new SubnetData() { Name = "subnet1", AddressPrefix = "10.0.0.0/28" },
                        new SubnetData() { Name = "subnet2", AddressPrefix = "10.0.0.16/28" },
                    },
                };
                var vnetALro = await resourceGroup.GetVirtualNetworks().CreateOrUpdateAsync(WaitUntil.Completed, vnetAName, vnetAInput);
                VirtualNetworkResource vnetA = vnetALro.Value;
                Utilities.Log($"Created a virtual network: {vnetA.Data.Name}");

                VirtualNetworkData vnetBInput = new VirtualNetworkData()
                {
                    Location = resourceGroup.Data.Location,
                    AddressPrefixes = { "10.1.0.0/27" },
                    Subnets =
                    {
                        new SubnetData() { Name = "subnet3", AddressPrefix = "10.1.0.0/27" },
                    },
                };
                var vnetBLro = await resourceGroup.GetVirtualNetworks().CreateOrUpdateAsync(WaitUntil.Completed, vnetBName, vnetBInput);
                VirtualNetworkResource vnetB = vnetBLro.Value;
                Utilities.Log($"Created a virtual network: {vnetB.Data.Name}");

                //// Print virtual network details
                //foreach (INetwork network in created)
                //{
                //    Utilities.PrintVirtualNetwork(network);
                //    Utilities.Log();
                //}

                //// Retrieve the created networks using their definition keys
                //INetwork vnetA = created.FirstOrDefault(n => n.Key == vnetADefinition.Key);
                //INetwork vnetB = created.FirstOrDefault(n => n.Key == vnetBDefinition.Key);

                //=============================================================
                // Peer the two networks using default settings

                Utilities.Log(
                        "Peering the networks using default settings...\n"
                        + "- Network access enabled\n"
                        + "- Traffic forwarding disabled\n"
                        + "- Gateway use (transit) by the peered network disabled");

                // Create peering in vnetA side
                VirtualNetworkPeeringData peeringInput = new VirtualNetworkPeeringData()
                {
                    AllowVirtualNetworkAccess = true,
                    AllowForwardedTraffic = false,
                    AllowGatewayTransit = false,
                    UseRemoteGateways = false,
                    RemoteVirtualNetworkId = vnetB.Data.Id,
                };
                var peeringLro = await vnetA.GetVirtualNetworkPeerings().CreateOrUpdateAsync(WaitUntil.Completed, peeringABName, peeringInput);
                VirtualNetworkPeeringResource peering = peeringLro.Value;
                Utilities.Log($"Created peering {peering.Data.Name} in vnetA side");
                Utilities.Log("Peering state: " + peering.Data.PeeringState);

                // Create peering in vnetB side
                peeringInput = new VirtualNetworkPeeringData()
                {
                    AllowVirtualNetworkAccess = true,
                    AllowForwardedTraffic = false,
                    AllowGatewayTransit = false,
                    UseRemoteGateways = false,
                    RemoteVirtualNetworkId = vnetA.Data.Id,
                };
                _ = await vnetB.GetVirtualNetworkPeerings().CreateOrUpdateAsync(WaitUntil.Completed, peeringABName, peeringInput);
                Utilities.Log($"Created peering {peering.Data.Name} in vnetB side");

                // Print network details showing new peering
                Utilities.Log("Created a peering");
                await Utilities.PrintVirtualNetwork(vnetA);
                await Utilities.PrintVirtualNetwork(vnetB);

                //=============================================================
                // Update a the peering disallowing access from B to A but allowing traffic forwarding from B to A

                Utilities.Log("Updating the peering ...");

                VirtualNetworkPeeringData updatePeeringInput = peering.Data;
                updatePeeringInput.AllowVirtualNetworkAccess = false;
                updatePeeringInput.AllowForwardedTraffic = true;
                peeringLro = await vnetA.GetVirtualNetworkPeerings().CreateOrUpdateAsync(WaitUntil.Completed, peeringABName, updatePeeringInput);
                peering = peeringLro.Value;

                Utilities.Log("Updated the peering to disallow network access between B and A but allow traffic forwarding from B to A.");

                //=============================================================
                // Show the new network information

                await Utilities.PrintVirtualNetwork(vnetA);
                await Utilities.PrintVirtualNetwork(vnetB);

                //=============================================================
                // Remove the peering

                Utilities.Log("Deleting the peering from the networks...");
                await peering.DeleteAsync(WaitUntil.Completed);

                // This deletes the peering from both networks, if they're in the same subscription
                Utilities.Log("Deleted the peering from both sides.");
                await Utilities.PrintVirtualNetwork(vnetA);
                await Utilities.PrintVirtualNetwork(vnetB);
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group...");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId.Name}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Utilities.Log(ex);
            }
        }
    }
}