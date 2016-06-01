# Neuron
Neuron is a Unity based modular game, which has an API for plugins designed for extendibility and security.

## Compiling
First you need to get the proprietary assets (currently only [UniStorm](https://www.assetstore.unity3d.com/en/#!/content/2714))

Unpack UniStorm and move the "UniStorm (Desktop)" folder to Assets/Proprietary/Plugins/ (the path may not exist, create it if needed). You won't need the "UniStorm (Mobile)" folder.

**Important: Exclude UniStorm when commiting, since you aren't allowed to redistribute it.**

After this, you'll need to patch Unity, because of a bug in UNet Weaver it will refuse to compile. Download [this file](https://github.com/Trojaner25/Neuron/blob/master/Utils/Unity.UNetWeaver.dll) and replace the one at C:\Program Files\Unity\Editor\Data\Managed

**This will disable UNet Weaver completly! Do not forget to backup that file, in case you need UNet Weaver for other projects.** 

You can now compile this project!
