# Jellyfin Alexa Plugin

Alexa skill plugin for Jellyfin

---



---

_Note: This project is still in a very early alpha phase, this means not all features are fully functional yet and features or usage can change significantly between releases.
Moreover, new releases can result in data loss of the skill database.
Changes on the main branch are not final and may brake your setup, please use the tagged versions for more stable beta versions eg. `v0.x`.
Please always create a backup of your setup beforehand._

_Note: If you are looking for a more stable and feature-packed Alexa Skill for Jellyfin, then check out this [project](https://github.com/infinityofspace/jellyfin_alexa_skill)._

### Table of Contents
 1. [About](#about)
 2. [Features](#features)
 3. [Requirements](#requirements)
 4. [Installation](#installation)
 5. [Database](#database)
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
- other:
    - multi-language support (see [Supported languages](#supported-languages))
    - multi-user support (any user on the Jellyfin server can use the skill)

Note: currently only audio is a supported media type

If you have a feature idea, use this [issue template](https://github.com/infinityofspace/jellyfin-alexa-plugin/issues/new?labels=feature&template=feature_request.md) to suggest your idea for implementation.

## Requirements

Before you continue, make sure you meet the following requirements:
- Jellyfin server: 10.8+
- [Amazon developer account](https://developer.amazon.com/en-US/docs/alexa/ask-overviews/create-developer-account.html)
- publicly accessible domain with a valid SSL certificate pointing to your Jellyfin server

## Installation

1. Clone this repository and checkout the latest release:

    ```shell
    git clone https://github.com/infinityofspace/jellyfin-alexa-plugin.git
    git checkout <version>
    ```

2. Ensure you have .NET Core 6.0 SDK setup and installed

3. Build plugin with following command.

    ```sh
    dotnet publish --configuration Release --output bin
    ```
4. Create a folder `AlexaSkill` on your jellyfin server plugin folder (normally this is the located at <root-dir-of-jellyfin-data>/config/plugins)
5. Stop your Jellyfin server and copy the `Jellyfin.Plugin.AlexaSkill.dll` file from the bin folder into the newly created `AlexaSkill` folder
6. Restart your Jellyfin server
7. Go to the plugin settings page and fill out the required values, then click `Save` and `Rebuild Skill` (the first build may take some time)
8. Now go into your Alexa app and link your desired Jellyfin account

## Database

The skill saves data in an own database, which is located at the `data` folder in your root Jellyfin data folder and named `alexa-skill-plugin.db`.

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
|                LiteDB                |        [License](https://raw.githubusercontent.com/mbdavid/LiteDB/master/LICENSE)        |                                  [Project](https://github.com/mbdavid/litedb)                                  |
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
