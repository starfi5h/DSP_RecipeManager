# Recipe Manager

Allow to move recipes. Right click to craft the new added recipes first.  
Based on [DSPMoreRecipes](https://thunderstore.io/c/dyson-sphere-program/p/appuns/DSPMoreRecipes/) and [DequeCraft](https://thunderstore.io/c/dyson-sphere-program/p/ardnaxelarak/DequeCraft/).  

## Move Recipes

To move recipes in replicator window:    
1. Click "Enter Edit Mode"
2. Click the recipe you want to move.
3. Click the destination grid. The
4. Click "Exit Edit Mode" to save the changes. The settings is stored in `starfi5h.plugin.RecipeManager.cfg`.

In edit mode, all recipes will be shown. The grid with recipe position changed will be highlighted with blue background, and the grid with multiple recipes will be highlighted in red.

## Craft First

Right click on the craft button will add the item to the front of the queue.  
Note: It is implemented by canceling the existing tasks first, then adding them back, so it may fail if the inventory doesn't have enough space.  

## Changelog

\- v1.0.0: Initial released. (DSP 0.10.32.25712)  
