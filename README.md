# VRSimTk
### Virtual Reality Simulation Toolkit for Unity
**Requires Unity 5.5.4 or higher.**

This project aims to provide a framework for the development of Virtual Environments, able to interoperate with data coming from decoupled simulations.
Currently an XML data exchange module has been developed, but the final goal is to have a framework for the development of other *connectors*.

**This is a work-in-progress.**

## Features
* Entity-relationship model definition
  * entities
  * representations
  * one-to-one, one-to many relationships
  * specialized relationships (inclusion, composition)
* Asset management:
  * asset bundles (via [AssetBundle_Manager_5.5.0](https://github.com/gpvigano/AssetBundle_Manager_5.5.0/tree/feat/custom_configuration))
  * concurrent loading: a loader is created for each model
  * option for importing files as a prefab+assets (meshes and textures)
    in Unity project
  * support for in-scene UI (progress display and messages)
* Integration with Unity Editor
  * context menus with custom commands
  * commands for application deployment
* Examples provided
  * sample environment
  * simulation control UI
  * sample simulation

## Documentation
A sub-menu *Import OBJ model [AsImpL]* is added to the Unity Editor Asset menu.
It opens a window where you can set paths and import settings, then you can press *Import* to start importing the
selected model. A progress bar shows the import progress and phase until
the model has been loaded (or until you press *Cancel* to abort the
process). To the Window menu a sub-menu item `Capture screenshot [AsImpL]` is added to take
a screenshot, the file is named automatically and saved into the main
project folder, then the folder is opened in your file manager.

An example scene is provided to demonstrate how a simulation can be
imported/exported from/to XML. You can find this in the set of scenes
named `TestSim*`.

The code in this project *should* be prepared to be extended for supporting other data sources.
Even if the only supported format is currently XML, the idea is to create a common framework on which the support for other data sources could be developed, allowing the exchange of data with other applications.

You can find the VRSimTk documentation in [Documentation folder].

### Getting Started
To try this project with Unity press the button **Clone or download** and choose [**Download ZIP**](https://github.com/gpvigano/VRSimTk/archive/WIP.zip). Save and unzip the archive to your hard disk and then you can open it with Unity.
Open the scene `VRSimTk/Example/XML/TestSim`, choose `AssetBundles/Build AssetBundles` from the Editor menu, and wait until the process finishes.
Press play in Unity Editor, then press *Load scen* button in the UI. After a while you will be able to control the simulation using the UI on the right side.

### Acknowledgements:

This project was inspired by my previous work focused on [GIOVE Virtual Factory](https://link.springer.com/chapter/10.1007/978-1-84996-172-1_12), carried on in several research projects at [CNR-ITIA](http://www.itia.cnr.it).
The asset bundle management is a slightly [modified version](https://github.com/gpvigano/AssetBundle_Manager_5.5.0/tree/feat/custom_configuration) of *Unity 5.5.0 Unity's AssetBundle Manager*, thanks to [agentc0re](https://github.com/agentc0re), who made [Unity's AssetBundleDemo](https://bitbucket.org/Unity-Technologies/assetbundledemo) (MIT license) available on GitHub and working with Unity 5.5.  
**This project uses [AsImpL](https://github.com/gpvigano/AsImpL) for loading 3D models at run-time.**

Thanks in advance to all the people who will contribute in any way to this project.


### Contributing

Contributions from you are welcome, but please notice that **this is still a work-in-progress**!

If you find bugs or you have any new idea for improvements and new features you can raise an issue on GitHub. To open issues or make pull requests please follow the instructions in [CONTRIBUTING.md](https://github.com/gpvigano/VRSimTk/blob/master/CONTRIBUTING.md).

### License

Code released under the [MIT License](https://github.com/gpvigano/VRSimTk/blob/master/LICENSE.txt).


[Documentation folder]: https://github.com/gpvigano/VRSimTk/blob/WIP/Documentation/


