# Neuron
Neuron is a Unity based modular game, which has an API for plugins designed for extendibility and security.

## C# 6.0
This project uses alexzzzz's great [CSharp60 Support](https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/), so the project source is written in C# 6.0.

**Notice**: some  features of C# 6.0 are not supported because of CLR 2.0 limitations. Please see [here](https://bitbucket.org/alexzzzz/unity-c-5.0-and-6.0-integration/src/531028fa9405927c6ef96c0d8c587b0388130cbf/README.md?at=default&fileviewer=file-view-default) for more information. You're also still limited to .NET 3.5.

## Compiling
First you need to get the proprietary assets (currently only [UniStorm](https://www.assetstore.unity3d.com/en/#!/content/2714))

Unpack UniStorm and move the "UniStorm (Desktop)" folder to Assets/Proprietary/Plugins/ (the path may not exist, create it if needed). You won't need the "UniStorm (Mobile)" folder.

**Important: Exclude UniStorm when commiting, since you aren't allowed to redistribute it.**

After this, you'll need to patch Unity, because of a bug in UNet Weaver it will refuse to compile. Download [this file](https://github.com/Trojaner25/Neuron/blob/master/Utils/Unity.UNetWeaver.dll) and replace the one at C:\Program Files\Unity\Editor\Data\Managed

**This will disable UNet Weaver completly! Do not forget to backup that file, in case you need UNet Weaver for other projects.** (You'll need it only if you use Unity's Networking APIs)

You can now compile this project!

