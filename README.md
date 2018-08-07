---
services: virtual-network
platforms: dotnet
author: martinsawicki
---

# Manage network peering between two virtual networks #

          Azure Network sample for enabling and updating network peering between two virtual networks
         
          Summary ...
         
          - This sample creates two virtual networks in the same subscription and then peers them, modifying various options on the peering.
         
          Details ...
         
          1. Create two virtual networks, network "A" and network "B"...
          - network A with two subnets
          - network B with one subnet
          - the networks' address spaces must not overlap
          - the networks must be in the same region
         
          2. Peer the networks...
          - the peering will initially have default settings:
            - each network's IP address spaces will be accessible from the other network
            - no traffic forwarding will be enabled between the networks
            - no gateway transit between one network and the other will be enabled
         
          3. Update the peering...
          - disable IP address space between the networks
          - enable traffic forwarding from network A to network B
          
          4. Delete the peering
          - the removal of the peering takes place on both networks, as long as they are in the same subscription
         
          Notes: 
          - Once a peering is created, it cannot be pointed at another remote network later.
          - The address spaces of the peered networks cannot be changed as long as the networks are peered.
          - Gateway transit scenarios as well as peering networks in different subscriptions are possible but beyond the scope of this sample.
          - Network peering in reality results in pairs of peering objects: one pointing from one network to the other,
            and the other peering object pointing the other way. For simplicity though, the SDK provides a unified way to
            manage the peering as a whole, in a single command flow, without the need to duplicate commands for both sides of the peering,
            while enforcing the required restrictions between the two peerings automatically, as this sample shows. But it is also possible
            to modify each peering separately, which becomes required when working with networks in different subscriptions.


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-java/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/network-dotnet-manage-network-peering.git

    cd network-dotnet-manage-network-peering

    dotnet restore

    dotnet run

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.