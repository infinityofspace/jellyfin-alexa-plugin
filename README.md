# Jellyfin Alexa Plugin

Alexa skill plugin for Jellyfin

---

[![dev build](https://github.com/infinityofspace/jellyfin-alexa-plugin/actions/workflows/dev-build.yml/badge.svg)](https://github.com/infinityofspace/jellyfin-alexa-plugin/actions/workflows/dev-build.yml) ![GitHub all releases](https://img.shields.io/github/downloads/infinityofspace/jellyfin-alexa-plugin/total?label=total%20downloads)

---

_Note: This project is still in a very early alpha phase, this means not all features are fully functional at this time and features or usage can change significantly between releases.
Moreover, new releases can result in data loss of the plugin configuration.
Changes on the main branch are not final and may brake your setup, please use the tagged versions for more stable beta versions eg. `0.x`.
Please always create a backup of your setup beforehand._

_Note: If you are looking for a more stable and feature-packed Alexa Skill for Jellyfin, then check out this [project](https://github.com/infinityofspace/jellyfin_alexa_skill)._

### Table of Contents
 1. [About](#about)
 2. [Features](#features)
 3. [Requirements](#requirements)
 4. [Installation](#installation)
 5. [Configuration file](#configuration-file)
 6. [Supported languages](#supported-languages)
 7. [Skill speech examples](#skill-speech-examples)
 8. [Project plan](#project-plan)
 9. [Third party notices](#third-party-notices)
 10. [License](#license)

## About

This is a Jellyfin plugin for an Alexa skill to play media from your Jellyfin server. Besides, simple playback, other additional functions like playback of playlists or managing favorites are included.

## Features

- playback control:
    - play a specific media
    - play media from an artist
    - pause/resume/stop/cancel a playback
    - play previous/next song in queue
- playlist:
    - play a specific playlist
- favorite:
    - play favorite media
    - add media to favorites
    - remove media from favorites
- other:
    - multi-language support (see [Supported languages](#supported-languages))
    - multi-user support (any user on the Jellyfin server can use the skill)
    - multi-alexa-user support, allow different Amazon Alexa accounts use the skill

Note: currently only audio is a supported media type

If you have a feature idea, use this [issue template](https://github.com/infinityofspace/jellyfin-alexa-plugin/issues/new?labels=feature&template=feature_request.md) to suggest your idea for implementation.

## Requirements

Before you continue, make sure you meet the following requirements:
- Jellyfin server: 10.8+
- [Amazon developer account](https://developer.amazon.com/en-US/docs/alexa/ask-overviews/create-developer-account.html)
- publicly accessible Jellyfin server with a domain and a valid SSL certificate

## Installation

1. You can either use the prebuilds or build the plugin by yourself:
   1. Use prebuilds, here you have also two options:
      1. Use the plugin repository (recommend):
         1. Open the admin dashboard of your Jellyfin server.
         2. Go to Plugins and select the `Repositories` tab.
         3. Add a new repository with the following URL (name can be anything): `https://raw.githubusercontent.com/infinityofspace/jellyfin-alexa-plugin/master/manifest.json`
         4. Now you can find the plugin under the `General` category in the `Catalog` tab.
      2. Manually download the latest release:
         1. Download the lastes release from the [releases page](https://github.com/infinityofspace/jellyfin-alexa-plugin/releases).
         2. Extract the zip file.
         3. Copy the folder `Jellyfin.Plugin.AlexaSkill` of the extracted zip to the `plugins` folder of your Jellyfin server.
   2. Build the plugin by yourself:
      1. Ensure you have .NET Core 6.0 SDK setup and installed.
      2. Clone the repository and checkout the latest release tag:
            ```bash
            git clone https://github.com/infinityofspace/jellyfin-alexa-plugin.git
            cd jellyfin-alexa-plugin
            git checkout <version>
            ```
      3. Build plugin with the following command:
            ```bash
            dotnet publish --configuration Release
            ```
      4. Create a folder named `Jellyfin.Plugin.AlexaSkill` inside the `plugins` folder of your Jellyfin server data folder.
      5. Copy everything from the folder `Jellyfin.Plugin.AlexaSkill/bin/Release/net6.0/publish/` to the previously created folder `Jellyfin.Plugin.AlexaSkill` in your Jellyfin server `plugin` directory. 
2. Restart your Jellyfin server.
3. Go to the plugin settings page and configure the plugin. You can find [here](https://github.com/infinityofspace/jellyfin-alexa-plugin/wiki/Configuration) more details on how to fill out the required values and configure the plugin.
4. Now go into your Alexa app and link your desired Jellyfin account.
5. You are now ready to use the skill. Let's start the skill with "Alexa, start Jellyfin Player" (assuming the invocation name has not been customized).

## Configuration file

The skill saves data in the plugin config xml file, which is located at `plugins/configurations` path in your root Jellyfin folder and named `Jellyfin.Plugin.AlexaSkill.xml`.

## Supported languages

The skill has support for the following languages:

- English
- German (no localized Alexa responses)

## Skill speech examples

The [wiki](https://github.com/infinityofspace/jellyfin-alexa-plugin/wiki/Interaction-examples) contains examples how to interact with the skill.

## Project plan

Take a look at the [project plan](https://github.com/infinityofspace/jellyfin-alexa-plugin/projects) to see what features and bug fixes are planned and in progress.

## Third party notices

|                Module                |                                         License                                          |                                                    Project                                                     |
| :----------------------------------: | :--------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------: |
|              Alexa.NET               | [License](https://raw.githubusercontent.com/timheuer/alexa-skills-dotnet/master/LICENSE) |                           [Project](https://github.com/timheuer/alexa-skills-dotnet)                           |
|         Alexa.NET.Management         | [License](https://raw.githubusercontent.com/stoiveyp/Alexa.NET.Management/main/LICENSE)  |                          [Project](https://github.com/stoiveyp/Alexa.NET.Management)                           |
|          Amazon.Lambda.Core          |    [License](https://raw.githubusercontent.com/aws/aws-lambda-dotnet/master/LICENSE)     |        [Project](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.Core)        |
|   Amazon.Lambda.Serialization.Json   |    [License](https://raw.githubusercontent.com/aws/aws-lambda-dotnet/master/LICENSE)     | [Project](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.Serialization.Json) |
| Microsoft.Extensions.Logging.Console |                                       [License](https://raw.githubusercontent.com/dotnet/runtime/main/LICENSE.TXT)                                        |                                                  [Project](https://github.com/dotnet/runtime)                                                   |
|         Jellyfin.Controller          |      [License](https://raw.githubusercontent.com/jellyfin/jellyfin/master/LICENSE)       |                                [Project](https://github.com/jellyfin/jellyfin)                                 |
|            Jellyfin.Model            |      [License](https://raw.githubusercontent.com/jellyfin/jellyfin/master/LICENSE)       |                                [Project](https://github.com/jellyfin/jellyfin)                                 |


Furthermore, this readme file contains embeddings of [Shields.io](https://github.com/badges/shields).

## License

[GPL-3.0](https://github.com/infinityofspace/jellyfin-alexa-plugin/blob/main/LICENSE)
