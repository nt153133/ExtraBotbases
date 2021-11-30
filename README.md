<a class="bmc-button" target="_blank" href="https://www.buymeacoffee.com/soACz8y"><img src="https://cdn.buymeacoffee.com/buttons/bmc-new-btn-logo.svg" alt="Buy me a coffee"><span style="margin-left:5px;font-size:28px !important;">Buy me a coffee</span></a>

# ExtraBotbases
Llama Utilities botbase for RebornBuddy along with LL orderbot tags

## Installation

### Automatic Setup

The easiest way to install LlamaLibrary is to install the [repoBuddy](https://github.com/Zimgineering/repoBuddy) plugin. It would be installed in the **/plugins** folder of your rebornBuddy folder. It will automatically install the files into the correct folders.

### Manual Setup

For those of you that don't want to use [repoBuddy](https://github.com/Zimgineering/repoBuddy) here's the manual installtion method. 

First off, make sure you remove any previous versions of LlamaLibrary you may have in the **/BotBases** folder.

Download the zip from [ExtraBotbases](https://github.com/nt153133/ExtraBotbases) and create a folder in **/BotBases** called **ExtraBotbases** and either unzip the contents of the zip into that folder, or check out using a SVN client to that folder. These are a few extra botbases that most users will not use.

## Botbases

### GCExpertGrind
This botbase will craft the set item using Lisbeth and turn it in until Max Seals. It can also be used to just turn in every item in your inventory.

#### Settings
* AcceptedRisk - By setting this to True you are accepting that any item in your inventory could potentially be turned in to your GC for seals. If you don't want it destroyed, deposit it somewhere else. You must set this to True for the Botbase to work. 
* Craft - Set this to True if you want the Botbase to craft the item with Lisbeth rather than just turning in the items in your inventory.
* ItemId - This is the Item ID of the item you want Lisbeth to create for seals.
* SealReward - Set this to how many seals you get per turn in of the item, the botbase uses this to determine how many of the above item to make to get to max seals.
