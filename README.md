# WindowsGSM.ConanExiles
![GitHub Release](https://img.shields.io/github/v/release/Soulflare3/WindowsGSM.ConanExiles)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/Soulflare3/WindowsGSM.ConanExiles/total)

🧩 A plugin version of the Conan Exiles Dedicated server for WindowsGSM

## Mod AutoUpdate (only steammods possible)
- Just copy your ModList.txt to the ServerFiles root (click Browse => serverfiles) and click update.
- WGSM will try to load all the mods from steam and copy them into the Mods folder.
- If you need to change the order: edit that file. the Plugin will recreate the ConanExilesSandbox/Mods/Modlist.txt after each update

### Official Documentation
🗃️ https://www.conanexiles.com/dedicated-servers/

### The Game
🕹️ https://store.steampowered.com/app/440900/Conan_Exiles/

### Dedicated server info
🖥️ https://steamdb.info/app/443030

## Requirements
[WindowsGSM](https://github.com/WindowsGSM/WindowsGSM) >= 1.21.0

## Installation
1. Download the [latest](https://github.com/Soulflare3/WindowsGSM.ConanExiles/releases/latest) release
1. Move **ConanExiles.cs** folder to **plugins** folder or import the zip from the plugins tab
1. Click the **[RELOAD PLUGINS]** button or restart WindowsGSM

### Files To Backup
- Save Gane
  - WindowsGSM\servers\%ID%\serverfiles\ConanSandbox\Saved
  - WindowsGSM\servers\%ID%\serverfiles\ConanSandbox\Config
- WindowsGSM Config
  - WindowsGSM\servers\%ID%\configs

### Not having an full IPv4 adress ( named CCNAT or DSL Light )
No game or gameserver supports ipv6 only connections. 
- You need to either buy one (most VPN services provide that option. A pal uses ovpn.net for his server, I know of nordvpn also providing that. Should both cost around 7€ cheaper half of it, if your already having an VPN)
- Or you pay a bit more for your internet and take a contract with full ipv4. (depending on your country)
- There are also tunneling methods, which require acces to a server with a full ipv4. Some small VPS can be obtained, not powerfull enough for the servers themself, but only for forwarding. I think there are some for under 5€), the connection is then done via wireguard. but its a bit configuration heavy to setup) 

Or you connect your friends via VPN to your net and play via local lan then.
Many windowsgsm plugin creators recommend zerotier (should be a free VPN designated for gaming) , see chapter below (or tailscale, but no howto there)

## How can you play with your friends without port forwarding?
- Use [zerotier](https://www.zerotier.com/) folow the basic guide and create network
- Download the client app and join to your network
- Create static IP address for your host machine
- Edit WGSM IP Address to your recently created static IP address
- Give your network ID to your friends
- After they've joined to your network
- They can connect using the IP you've created eg: 10.123.17.1:7777
- Enjoy

### Support
[WGSM](https://discord.com/channels/590590698907107340/645730252672335893)

### License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Soulflare3/WindowsGSM.ConanExiles/blob/master/LICENSE) file for details
