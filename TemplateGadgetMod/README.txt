In addition to changing all references to "Template" this and that to a more appropriate name,
you need to add a file called "GamePaths.xml" to the folder ABOVE where you put this template, with the following content:

<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <!-- Set this full path to your game folder. Must contain a slash at the end. -->
    <GamePath>C:\Program Files (x86)\Steam\steamapps\common\Roguelands\</GamePath>

    <!-- Set this partial path to the game's Managed folder. Must contain a slash at the end. -->
    <ManagedFolder>Roguelands_Data\Managed\</ManagedFolder>
  </PropertyGroup>
</Project>

Note that `GamePath` may need to be changed, depending on the nature of your installation.