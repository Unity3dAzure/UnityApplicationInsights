# Application Insights for Unity UI and Mixed Reality

Track Unity UI button event and Mixed Reality event telemetry using [Application Insights](https://azure.microsoft.com/en-gb/services/application-insights/)

## Setup

1. Import [Application Insight Plugins](https://github.com/Microsoft/HolographicAcademy/raw/Azure-MixedReality-Labs/Azure%20Mixed%20Reality%20Labs/MR%20and%20Azure%20309%20-%20Application%20insights/AppInsights_LabPlugins.unitypackage) into Unity project.
2. Create new [Application Insights](https://portal.azure.com) service in Azure
3. Copy and paste your Application Insights **Instrumentation Id** into the *Application Insights* Unity scene script. 
4. (Optional) You may also wish to add your **App Id** and (read only) **Key** for running Application Insights Query scripts.

## How it works

**Add one instance of the *UnityApplicationInsights* scripts into your main Unity scene.**

- Unity scene changes are automically captured as a **PageView** event.

- To add logging to Application Insights for Unity UI buttons you can either manually attach the *ButtonTrackerBehaviour* to the Unity button, or enable the *Add Tracker Behaviours* setting in the Inspector which will attach the script automatically to all selectable game objects upon scene change.

## Application Insights sample scenes
| Scene name | Description |
| --- | --- |
| **Scene-UI** | Unity UI Button event telemetry sample scene 
| **Scene-MR** | Interactive hologram event sample scene for Mixed Reality

## Application Insights visualisations
In Application Insights **Usage** section you can visualise telemetry logged from your app.

#### User Flows
Chart user flow across Unity scene changes and split by custom or interaction events.

![App Insights User Flows](https://user-images.githubusercontent.com/1880480/45675692-cf400c80-bb27-11e8-9bc9-207efcc6f80f.PNG)

#### Sessions
View users and events during sessions.

![App Insights User Sessions](https://user-images.githubusercontent.com/1880480/45675721-e1ba4600-bb27-11e8-9355-7faf52e1c33b.PNG)

#### Funnels
Create funnels by creating step by step conditions to get conversion rates.

![App Insights Funnels](https://user-images.githubusercontent.com/1880480/45675730-e5e66380-bb27-11e8-9b02-5dd4a19d2c2c.PNG)

## Custom visualization of Unity UI and MR telemetry 

To create custom visualizations using all the data collected by Application Insights you can use the [Ibex Dashboard](https://github.com/Azure/ibex-dashboard). Fork the project and copy the [Unity UI template](https://gist.github.com/deadlyfingers/2d66cd04ad34a23fc5f5ec006d0fff12) or [MR template](https://gist.github.com/deadlyfingers/e664aaccb748be2f332f462615f6a090) file into the `server/dashboards/preconfigured` directory. You should then be able to create your own dashboard by using these templates in the app.

![Custom Dashboard for Unity UI interactions](https://user-images.githubusercontent.com/1880480/45675769-00204180-bb28-11e8-804c-17e77c4d78bf.PNG)

![Custom Dashboard for Unity MR interactions](https://user-images.githubusercontent.com/1880480/45675798-10382100-bb28-11e8-9b4e-e4d908316f89.PNG)

## Dependencies
- [Application Insight Plugins](https://github.com/Microsoft/HolographicAcademy/raw/Azure-MixedReality-Labs/Azure%20Mixed%20Reality%20Labs/MR%20and%20Azure%20309%20-%20Application%20insights/AppInsights_LabPlugins.unitypackage)

## Optional dependencies
Optional dependencies are included as git submodules which can be installed after cloning:  

`git submodule update --init --recursive`

The Voronoi selection sample code is commented out inside the *Scripts/MR/Tap.cs* script.  
- [Voronoi Diagram C#](https://github.com/PouletFrit/csDelaunay)

Voronoi selection is useful in Mixed Reality scenarios for selecting holograms which may be small and close together or even overlap. In relation to capturing telemetry in MR scenarios we might want to capture Air Taps on holograms. But what happens when a user taps on void spaces which may have missed the target. We can log any useful metrics regarding nearest objects to Application Insights or even enable the closest object to be triggered.

## Related documentation
- [MR and Azure 309: Application insights](https://docs.microsoft.com/en-us/windows/mixed-reality/mr-azure-309)
- [Using Voronoi selection in Mixed Reality](https://www.microsoft.com/developerblog/2018/03/09/voronoi-selection-cancer-drug-network-visualization-mixed-reality/)